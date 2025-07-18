using System.Linq;
using System.Threading.Tasks;
using EntertainmentGuild.Data;
using EntertainmentGuild.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EntertainmentGuild.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public LoginController(SignInManager<IdentityUser> signInManager,
                               UserManager<IdentityUser> userManager,
                               ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        // ✅ GET: Role-based login pages
        [HttpGet]
        public IActionResult Customer()
        {
            return View("Login", new LoginViewModel { Role = "Customer" });
        }

        [HttpGet]
        public IActionResult Employee()
        {
            return View("Login", new LoginViewModel { Role = "Employee" });
        }

        [HttpGet]
        public IActionResult Admin()
        {
            return View("Login", new LoginViewModel { Role = "Admin" });
        }

        // ✅ POST: Perform login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Login", model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User does not exist.");
                return View("Login", model);
            }

            // ✅ Check if user is disabled
            var disabledUser = await _context.DisabledUsers.FindAsync(user.Id);
            if (disabledUser != null)
            {
                ModelState.AddModelError("", "Your account has been disabled.");
                return View("Login", model);
            }

            // ✅ Check role
            if (!await _userManager.IsInRoleAsync(user, model.Role))
            {
                ModelState.AddModelError("", $"This user is not a {model.Role}.");
                return View("Login", model);
            }

            // ✅ Try login
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return model.Role switch
                {
                    "Admin" => RedirectToAction("Product", "Admin"),
                    "Employee" => RedirectToAction("Product", "Employee"),
                    "Customer" => RedirectToAction("Index", "Customer"),
                    _ => RedirectToAction("Customer") // fallback
                };
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View("Login", model);
        }
    }
}
