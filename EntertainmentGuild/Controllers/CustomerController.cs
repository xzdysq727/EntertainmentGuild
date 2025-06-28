using System.Linq;
using System.Threading.Tasks;
using EntertainmentGuild.Data;
using EntertainmentGuild.Models;
using EntertainmentGuild.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntertainmentGuild.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CustomerController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // 首页
        public async Task<IActionResult> Index()
        {
            var vm = new TopProductsViewModel
            {
                Carousel = await _context.CarouselTopProducts.ToListAsync(),
                Recommendations = await _context.RecommendedTopProducts.ToListAsync()
            };

            return View(vm);
        }

        // 显示账户信息页面
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            var user = await _userManager.GetUserAsync(User);
            return View("CustomerAccount", user);
        }

        // 保存地址到数据库
        [HttpPost]
        public async Task<IActionResult> AddAddress(string Address, string City, string State, string PostalCode)
        {
            var user = await _userManager.GetUserAsync(User);

            var address = new Address
            {
                UserId = user.Id,
                AddressLine = Address,
                City = City,
                State = State,
                PostalCode = PostalCode
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            TempData["AddressSaved"] = "Address saved successfully.";

            return View("CustomerAccount", user);
        }

        [HttpGet]
        public async Task<IActionResult> ViewAddress()
        {
            var user = await _userManager.GetUserAsync(User);

            var addresses = await _context.Addresses
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            return View("ViewAddress", addresses);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
                TempData["Deleted"] = "Address deleted successfully.";
            }

            return RedirectToAction("ViewAddress");
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("ChangePassword");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


    }
}
