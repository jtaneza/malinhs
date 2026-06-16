using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MalikongkongNHS.Models.ViewModels;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        // =========================
        // LOGIN PAGE
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            var username = HttpContext.Session.GetString("Username");

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // =========================
        // LOGIN PROCESS
        // =========================

        [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Login(LoginViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var user = _userService.Login(model);

    if (user != null)
    {
        HttpContext.Session.SetString(
            "Username",
            user.Username);

        HttpContext.Session.SetString(
            "Role",
            user.Role);

        return user.Role switch
        {
            "Admin" =>
                RedirectToAction(
                    "Index",
                    "Dashboard"),

            "Teacher" =>
                RedirectToAction(
                    "Index",
                    "Teacher"),

            "Cashier" =>
                RedirectToAction(
                    "Index",
                    "Payment"),

            "Student" =>
                RedirectToAction(
                    "Index",
                    "Student"),

            _ =>
                RedirectToAction(
                    "Index",
                    "Dashboard")
        };
    }

    ViewBag.Error =
        "Invalid username or password.";

    return View(model);
}

        // =========================
        // LOGOUT
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }
    }
}