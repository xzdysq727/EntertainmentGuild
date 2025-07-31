using EntertainmentGuild.Data;
using EntertainmentGuild.Models;
using EntertainmentGuild.Models.Admin;
using EntertainmentGuild.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntertainmentGuild.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ 商品库存管理
        [HttpGet]
        public async Task<IActionResult> Product(string? subCategory)
        {
            var products = await _context.Products.ToListAsync();

            if (!string.IsNullOrEmpty(subCategory))
            {
                products = products
                    .Where(p => !string.IsNullOrEmpty(p.SubCategory) &&
                                p.SubCategory.Trim().Equals(subCategory.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.SelectedSubCategory = subCategory;
            return View(products);
        }

        // ✅ 修改商品数量
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int newQuantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            product.Quantity = newQuantity;
            await _context.SaveChangesAsync();

            return RedirectToAction("Product");
        }

        // ✅ 修改密码页面
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // ✅ 提交修改密码请求
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Login");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully.";
                return View();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ✅ 查看订单
        [HttpGet]
        public async Task<IActionResult> ManageOrders(string? userEmail)
        {
            var customers = await _userManager.GetUsersInRoleAsync("Customer");

            List<Order> orders = new();
            if (!string.IsNullOrEmpty(userEmail))
            {
                orders = await _context.Orders
                    .Where(o => o.UserEmail == userEmail)
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .ToListAsync();
            }

            ViewBag.Customers = customers;
            ViewBag.SelectedEmail = userEmail;

            return View(orders);
        }

        // ✅ 进入编辑物流信息页面
        [HttpGet]
        public async Task<IActionResult> EditShippingStatus(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            return View(order);
        }

        // ✅ 提交物流修改
        [HttpPost]
        public async Task<IActionResult> EditShippingStatus(Order model)
        {
            var order = await _context.Orders.FindAsync(model.Id);
            if (order == null) return NotFound();

            order.ShippingMethod = model.ShippingMethod;
            order.ShippingStatus = model.ShippingStatus;
            order.Courier = model.Courier;
            order.TrackingNumber = model.TrackingNumber;
            order.Remarks = model.Remarks;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Shipping info updated!";
            return RedirectToAction("ManageOrders", new { userEmail = order.UserEmail });
        }
    }
}
