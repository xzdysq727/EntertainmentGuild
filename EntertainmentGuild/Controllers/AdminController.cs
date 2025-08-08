using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EntertainmentGuild.Data;
using EntertainmentGuild.ViewModels;
using EntertainmentGuild.Models.Admin;
using EntertainmentGuild.Models;

namespace EntertainmentGuild.Controllers
{
    // This controller handles all admin-related actions.
    // It is restricted to users with the "Admin" role.
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Constructor - injects database context and user manager for identity handling
        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===========================
        // Product Management Section
        // ===========================

        // Displays the product list with optional category/subcategory filtering
        public async Task<IActionResult> Product(string category = null, string subCategory = null)
        {
            var products = _context.Products.AsQueryable();

            // Apply category filter if provided
            if (!string.IsNullOrEmpty(category))
                products = products.Where(p => p.Category == category);

            // Apply subcategory filter if provided
            if (!string.IsNullOrEmpty(subCategory))
                products = products.Where(p => p.SubCategory == subCategory);

            // Store selected filters in ViewBag for the view
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedSubCategory = subCategory;

            return View(await products.ToListAsync());
        }

        // Handles POST request for adding a new product
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
                // If an image is uploaded, store it as byte[] in the database
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await ImageFile.CopyToAsync(ms);
                    product.ImageData = ms.ToArray();
                    product.ImageMimeType = ImageFile.ContentType;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }

            // Redirect to the product list after adding
            return RedirectToAction("Product", new { category = product.Category });
        }

        // Loads product details for editing
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            return product == null ? NotFound() : View("EditProduct", product);
        }

        // Handles POST request to update product details
        [HttpPost]
        public async Task<IActionResult> EditProduct(Product model, IFormFile? ImageFile)
        {
            var product = await _context.Products.FindAsync(model.Id);
            if (product == null) return NotFound();

            // Update basic product information
            product.Name = model.Name;
            product.Price = model.Price;
            product.Description = model.Description;
            product.Category = model.Category;
            product.SubCategory = model.SubCategory;

            // Update product image if a new one is uploaded
            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await ImageFile.CopyToAsync(ms);
                product.ImageData = ms.ToArray();
                product.ImageMimeType = ImageFile.ContentType;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Product", new { category = product.Category });
        }

        // Deletes a product by ID
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Product");
        }

        // ===========================
        // Account & User Management
        // ===========================

        // Displays the admin's own account details
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var user = await _userManager.GetUserAsync(User);
            return View("AdminAccount", user);
        }

        // Lists active and disabled users for a specific role
        [HttpGet]
        public async Task<IActionResult> Manage(string role = "Customer")
        {
            var allUsers = await _userManager.GetUsersInRoleAsync(role);
            var disabled = await _context.DisabledUsers.Where(d => d.OriginalRole == role).ToListAsync();

            var disabledIds = disabled.Select(d => d.UserId).ToList();
            var activeUsers = allUsers.Where(u => !disabledIds.Contains(u.Id)).ToList();

            ViewBag.Role = role;
            ViewBag.DisabledUsers = disabled;
            return View(activeUsers);
        }

        // Enables or disables a user account
        [HttpPost]
        public async Task<IActionResult> ToggleDisable(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var existing = await _context.DisabledUsers.FindAsync(userId);
            if (existing != null)
            {
                // Re-enable user
                _context.DisabledUsers.Remove(existing);
                await _userManager.AddToRoleAsync(user, existing.OriginalRole);
            }
            else
            {
                // Disable user
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Customer";

                _context.DisabledUsers.Add(new DisabledUser
                {
                    UserId = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    OriginalRole = role
                });
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Manage", new { role = "Customer" });
        }

        // ===========================
        // Top Products Management
        // ===========================

        // Displays current top products (carousel + recommendations)
        [HttpGet]
        public async Task<IActionResult> TopProducts()
        {
            var vm = new TopProductsViewModel
            {
                Carousel = await _context.CarouselTopProducts.ToListAsync(),
                Recommendations = await _context.RecommendedTopProducts.ToListAsync()
            };
            return View(vm);
        }

        // Handles adding a product to the carousel or recommendation section
        [HttpPost]
        public async Task<IActionResult> AddTopProductHandler(IFormCollection form, IFormFile? ImageFile)
        {
            var sectionType = form["SectionType"];
            var name = form["Name"];
            var price = decimal.Parse(form["Price"]);
            var category = form["Category"];
            var subCategory = form["SubCategory"];
            var description = form["Description"];

            byte[]? imageBytes = null;
            string? mimeType = null;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await ImageFile.CopyToAsync(ms);
                imageBytes = ms.ToArray();
                mimeType = ImageFile.ContentType;
            }

            // Add to carousel
            if (sectionType == "Carousel")
            {
                if (await _context.CarouselTopProducts.CountAsync() >= 4)
                    return RedirectToAction("TopProducts");

                _context.CarouselTopProducts.Add(new CarouselTopProduct
                {
                    Name = name,
                    Price = price,
                    Category = category,
                    SubCategory = subCategory,
                    Description = description,
                    ImageData = imageBytes,
                    ImageMimeType = mimeType
                });
            }
            // Add to recommendations
            else if (sectionType == "Recommendation")
            {
                if (await _context.RecommendedTopProducts.CountAsync() >= 2)
                    return RedirectToAction("TopProducts");

                _context.RecommendedTopProducts.Add(new RecommendedTopProduct
                {
                    Name = name,
                    Price = price,
                    Category = category,
                    SubCategory = subCategory,
                    Description = description,
                    ImageData = imageBytes,
                    ImageMimeType = mimeType
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("TopProducts");
        }

        // Deletes a product from the carousel
        [HttpPost]
        public async Task<IActionResult> DeleteCarouselTopProduct(int id)
        {
            var item = await _context.CarouselTopProducts.FindAsync(id);
            if (item != null)
            {
                _context.CarouselTopProducts.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("TopProducts");
        }

        // Deletes a product from the recommendations
        [HttpPost]
        public async Task<IActionResult> DeleteRecommendedTopProduct(int id)
        {
            var item = await _context.RecommendedTopProducts.FindAsync(id);
            if (item != null)
            {
                _context.RecommendedTopProducts.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("TopProducts");
        }

        // Adds a new user with a specific role
        [HttpPost]
        public async Task<IActionResult> AddUser(string Email, string Password, string Role)
        {
            var user = new IdentityUser
            {
                UserName = Email,
                Email = Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Role);
            }

            return RedirectToAction("Manage");
        }
    }
}
