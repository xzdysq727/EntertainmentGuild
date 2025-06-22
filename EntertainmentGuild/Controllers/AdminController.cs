using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EntertainmentGuild.Models;
using EntertainmentGuild.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

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

        // 商品页：可筛选分类+子类
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
        // AdminController.cs
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var user = await _userManager.GetUserAsync(User);
            return View("AdminAccount", user); 
        }


        // 添加商品
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

        // 删除商品
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Product", new { category = product?.Category ?? "" });
        }

        // [GET] 进入编辑页面
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View("EditProduct", product);
        }

        // [POST] 保存编辑
        [HttpPost]
        public async Task<IActionResult> EditProduct(Product model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
                return View("EditProduct", model);

            var product = await _context.Products.FindAsync(model.Id);
            if (product == null) return NotFound();

            product.Name = model.Name;
            product.Price = model.Price;
            product.Description = model.Description;
            product.Category = model.Category;
            product.SubCategory = model.SubCategory;

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

                product.ImageUrl = relativePath;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Product", new { category = product.Category });
        }
        // AdminController.cs

        [HttpGet]
        public async Task<IActionResult> Manage(string role = "Customer")
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            ViewBag.Role = role;
            return View("Manage", users);
        }


    }
}
