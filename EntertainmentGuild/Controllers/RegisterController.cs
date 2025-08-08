using EntertainmentGuild.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EntertainmentGuild.Controllers
{
    // Controller to handle user registration
    public class RegisterController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        // Constructor injecting UserManager for identity operations
        public RegisterController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // GET action to display registration form
        [HttpGet]
        public IActionResult Index()
        {
            return View("Register", new RegisterViewModel());
        }

        // POST action to handle form submission for new user registration
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Validate model state
            if (!ModelState.IsValid)
                return View("Register", model);

            // Create new IdentityUser with provided email
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            // Attempt to create user with password
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                // Add errors to model state if creation failed
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("Register", model);
            }

            // Assign the new user to the "Customer" role
            await _userManager.AddToRoleAsync(user, "Customer");

            // Return success view with role info
            var successModel = new RegisterSuccessViewModel { Role = "Customer" };
            return View("RegisterSuccess", successModel);
        }
    }
}
