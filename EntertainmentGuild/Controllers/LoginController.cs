using EntertainmentGuild.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EntertainmentGuild.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

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

            if (!await _userManager.IsInRoleAsync(user, model.Role))
            {
                ModelState.AddModelError("", $"This user is not a {model.Role}.");
                return View("Login", model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (result.Succeeded)
            {
                switch (model.Role)
                {
                    case "Admin":
                        return RedirectToAction("Dashboard", "Admin");
                    case "Employee":
                        return RedirectToAction("Dashboard", "Employee");
                    case "Customer":
                        return RedirectToAction("Index", "Customer");
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View("Login", model);
        }
    }
}
