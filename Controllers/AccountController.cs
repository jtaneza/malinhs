using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MalikongkongNHS.Models.ViewModels;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService  _userService;
        private readonly IAuditService _audit;

        public AccountController(IUserService userService, IAuditService audit)
        {
            _userService = userService;
            _audit       = audit;
        }

        private string IP => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

        // =========================
        // LOGIN PAGE
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            var username = HttpContext.Session.GetString("Username");

            if (!string.IsNullOrEmpty(username))
            {
                var role = HttpContext.Session.GetString("Role");
                return role switch
                {
                    "Teacher" => RedirectToAction("Teacher", "Dashboard"),
                    "Cashier" => RedirectToAction("Index",   "Payment"),
                    "Student" => RedirectToAction("Student", "Dashboard"),
                    _         => RedirectToAction("Index",   "Dashboard")
                };
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
                return View(model);

            var user = _userService.Login(model);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role",     user.Role);
                HttpContext.Session.SetInt32 ("UserId",   user.UserId);

                _audit.Log(user.Username, user.Role, "Login", "Account",
                    $"{user.FullName} logged in ({user.Role})", IP);

                return user.Role switch
                {
                    "Admin"   => RedirectToAction("Index",   "Dashboard"),
                    "Teacher" => RedirectToAction("Teacher", "Dashboard"),
                    "Cashier" => RedirectToAction("Index",   "Payment"),
                    "Student" => RedirectToAction("Index",   "Dashboard"),
                    _         => RedirectToAction("Index",   "Dashboard")
                };
            }

            ViewBag.Error = "Invalid username or password.";
            return View(model);
        }

//Change Password
        [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ChangePassword(int UserId, string UserRole, string CurrentPassword, string NewPassword, string ConfirmPassword)
{
    if (NewPassword != ConfirmPassword)
    {
        TempData["HeaderPassError"] = "New password and confirmation do not match.";
        return Redirect(Request.Headers["Referer"].ToString());
    }

    if (UserRole == "Teacher")
    {
        // Resolve ITeacherService via HttpContext
        var teacherService = HttpContext.RequestServices.GetService<MalikongkongNHS.Services.Interfaces.ITeacherService>();
        var teacher = teacherService?.GetById(UserId);
        if (teacher == null) { TempData["HeaderPassError"] = "User not found."; return Redirect(Request.Headers["Referer"].ToString()); }
        if (teacher.Password != CurrentPassword) { TempData["HeaderPassError"] = "Current password is incorrect."; return Redirect(Request.Headers["Referer"].ToString()); }
        teacher.Password = NewPassword;
        teacherService!.Update(teacher);
        _audit.Log(teacher.Email ?? teacher.FullName, UserRole, "Update", "Account", $"Teacher ID {UserId} changed password.", IP);
    }
    else // Admin or Cashier — stored in Users table
    {
        var userService = HttpContext.RequestServices.GetService<MalikongkongNHS.Services.Interfaces.IUserService>();
        var user = userService?.GetUserById(UserId);
        if (user == null) { TempData["HeaderPassError"] = "User not found."; return Redirect(Request.Headers["Referer"].ToString()); }
        if (user.Password != CurrentPassword) { TempData["HeaderPassError"] = "Current password is incorrect."; return Redirect(Request.Headers["Referer"].ToString()); }
        user.Password = NewPassword;
        userService!.UpdateUser(user);
        _audit.Log(user.Username, UserRole, "Update", "Account", $"User ID {UserId} changed password.", IP);
    }

    TempData["HeaderPassSuccess"] = "Password updated successfully!";
    return Redirect(Request.Headers["Referer"].ToString());
}


        // =========================
        // LOGOUT
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("Username") ?? "Unknown";
            var role     = HttpContext.Session.GetString("Role")     ?? "Unknown";

            _audit.Log(username, role, "Logout", "Account",
                $"{username} logged out", IP);

            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}