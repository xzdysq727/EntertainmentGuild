using Microsoft.AspNetCore.Mvc;

public class LoginController : Controller
{
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
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model.Role, model);

       
        switch (model.Role)
        {
            case "Admin": return RedirectToAction("Dashboard", "Admin");
            case "Employee": return RedirectToAction("Dashboard", "Employee");
            default: return RedirectToAction("Index", "Customer");
        }
    }

}
