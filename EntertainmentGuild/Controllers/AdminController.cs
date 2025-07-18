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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 商品页
        public async Task<IActionResult> Product(string category = null, string subCategory = null)
        {
            var products = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(category))
                products = products.Where(p => p.Category == category);
            if (!string.IsNullOrEmpty(subCategory))
                products = products.Where(p => p.SubCategory == subCategory);

            ViewBag.SelectedCategory = category;
            ViewBag.SelectedSubCategory = subCategory;

            return View(await products.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
            {
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

            return RedirectToAction("Product", new { category = product.Category });
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            return product == null ? NotFound() : View("EditProduct", product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product model, IFormFile? ImageFile)
        {
            var product = await _context.Products.FindAsync(model.Id);
            if (product == null) return NotFound();

            product.Name = model.Name;
            product.Price = model.Price;
            product.Description = model.Description;
            product.Category = model.Category;
            product.SubCategory = model.SubCategory;

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

        // 管理员账户页
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var user = await _userManager.GetUserAsync(User);
            return View("AdminAccount", user);
        }

        // 用户管理
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

        [HttpPost]
        public async Task<IActionResult> ToggleDisable(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var existing = await _context.DisabledUsers.FindAsync(userId);
            if (existing != null)
            {
                _context.DisabledUsers.Remove(existing);
                await _userManager.AddToRoleAsync(user, existing.OriginalRole);
            }
            else
            {
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

        // Top Products 管理
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


