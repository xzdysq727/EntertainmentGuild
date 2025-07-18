using EntertainmentGuild.Data;
using EntertainmentGuild.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntertainmentGuild.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }
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

    }
}
