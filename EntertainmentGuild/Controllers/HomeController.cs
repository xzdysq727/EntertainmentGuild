using System.Diagnostics;
using EntertainmentGuild.Models;
using Microsoft.AspNetCore.Mvc;

namespace EntertainmentGuild.Controllers
{
    // Controller for basic site pages like home, privacy, and error pages
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        // Constructor injecting a logger for the controller
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Action to display the home page (Index)
        public IActionResult Index()
        {
            return View();
        }

        // Action to display the privacy policy page
        public IActionResult Privacy()
        {
            return View();
        }

        // Action to handle and display error details
        // Disables response caching to ensure fresh error info
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Passes an ErrorViewModel with the current request ID for diagnostics
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
