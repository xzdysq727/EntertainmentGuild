using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EntertainmentGuild.Controllers
{
    // Controller to handle user logout and redirect based on role
    public class LogoutController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        // Constructor injecting SignInManager and UserManager
        public LogoutController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Action to log out the current user and redirect to appropriate login page
        public async Task<IActionResult> Index()
        {
            // Get current logged-in user
            var user = await _userManager.GetUserAsync(User);
            // Get roles assigned to the user
            var roles = await _userManager.GetRolesAsync(user);

            // Sign out the user
            await _signInManager.SignOutAsync();

            // Redirect based on user role
            if (roles.Contains("Admin"))
                return RedirectToAction("Admin", "Login");

            if (roles.Contains("Employee"))
                return RedirectToAction("Employee", "Login");

            // Default redirect to customer login
            return RedirectToAction("Customer", "Login");
        }
    }
}
