using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EntertainmentGuild.Controllers
{
    public class LogoutController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public LogoutController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            await _signInManager.SignOutAsync();

            if (roles.Contains("Admin"))
                return RedirectToAction("Admin", "Login");

            if (roles.Contains("Employee"))
                return RedirectToAction("Employee", "Login");

            return RedirectToAction("Customer", "Login");
        }
    }
}
