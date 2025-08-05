using System.Linq;
using System.Threading.Tasks;
using EntertainmentGuild.Data;
using EntertainmentGuild.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EntertainmentGuild.ViewModels;

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

        // GET: Login pages for each role
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
                TempData["LoginError"] = "User does not exist.";
                return RedirectToAction(model.Role);
            }

           
            var disabledUser = await _context.DisabledUsers.FindAsync(user.Id);
            if (disabledUser != null)
            {
                TempData["LoginError"] = "Your account has been disabled.";
                return RedirectToAction(model.Role);
            }

          
            if (!await _userManager.IsInRoleAsync(user, model.Role))
            {
                TempData["LoginError"] = $"This user is not a {model.Role}.";
                return RedirectToAction(model.Role);
            }

         
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return model.Role switch
                {
                    "Admin" => RedirectToAction("Product", "Admin"),
                    "Employee" => RedirectToAction("Product", "Employee"),
                    "Customer" => RedirectToAction("Index", "Customer"),
                    _ => RedirectToAction("Customer") 
                };
            }

            TempData["LoginError"] = "Invalid login attempt.";
            return RedirectToAction(model.Role);
        }
    }
}
