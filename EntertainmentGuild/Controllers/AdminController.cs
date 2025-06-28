using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EntertainmentGuild.Models;
using EntertainmentGuild.Data;
using EntertainmentGuild.ViewModels;

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

        // 商品页面
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
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var user = await _userManager.GetUserAsync(User);
            return View("AdminAccount", user);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
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

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View("EditProduct", product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product model)
        {
            var product = await _context.Products.FindAsync(model.Id);
            if (product == null) return NotFound();

            product.Name = model.Name;
            product.Price = model.Price;
            product.Description = model.Description;
            product.ImageUrl = model.ImageUrl;
            product.Category = model.Category;
            product.SubCategory = model.SubCategory;

            await _context.SaveChangesAsync();
            return RedirectToAction("Product", new { category = product.Category });
        }

        // ✅ 用户管理
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

        // ✅ 顶部产品页 (方法一 ViewModel)
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
        public async Task<IActionResult> AddTopProductHandler(IFormCollection form, IFormFile? ImageFile)
        {
            var sectionType = form["SectionType"];
            var name = form["Name"];
            var price = decimal.Parse(form["Price"]);
            var category = form["Category"];
            var subCategory = form["SubCategory"];
            var description = form["Description"];

            string imageUrl = "";
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", fileName);
                var relativePath = $"/images/products/{fileName}";

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                imageUrl = relativePath;
            }

            if (sectionType == "Carousel")
            {
                if (await _context.CarouselTopProducts.CountAsync() >= 4)
                {
                    TempData["Error"] = "Only 4 Carousel items allowed.";
                    return RedirectToAction("TopProducts");
                }

                var newItem = new CarouselTopProduct
                {
                    Name = name,
                    Price = price,
                    Category = category,
                    SubCategory = subCategory,
                    Description = description,
                    ImageUrl = imageUrl
                };

                _context.CarouselTopProducts.Add(newItem);
            }
            else if (sectionType == "Recommendation")
            {
                if (await _context.RecommendedTopProducts.CountAsync() >= 2)
                {
                    TempData["Error"] = "Only 2 Recommended items allowed.";
                    return RedirectToAction("TopProducts");
                }

                var newItem = new RecommendedTopProduct
                {
                    Name = name,
                    Price = price,
                    Category = category,
                    SubCategory = subCategory,
                    Description = description,
                    ImageUrl = imageUrl
                };

                _context.RecommendedTopProducts.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("TopProducts");
        }

    }
}
