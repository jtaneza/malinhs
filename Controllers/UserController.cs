using Microsoft.AspNetCore.Mvc;
using MalikongkongNHS.Models.ViewModels;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService  _service;
        private readonly IAuditService _audit;

        public UserController(IUserService service, IAuditService audit)
        {
            _service = service;
            _audit   = audit;
        }

        private string Who  => HttpContext.Session.GetString("Username") ?? "Unknown";
        private string Role => HttpContext.Session.GetString("Role")     ?? "Unknown";
        private string IP   => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

        // ======================
        // USER LIST
        // ======================
        public IActionResult Index()
        {
            var users = _service.GetAllUsers();
            return View(users);
        }

        // ======================
        // CREATE USER
        // ======================
        [HttpPost]
        public IActionResult Create(UserM model)
        {
            if (ModelState.IsValid)
            {
                _service.CreateUser(model);
                _audit.Log(Who, Role, "Create", "User",
                    $"Created user account: {model.FullName} (Role: {model.Role})", IP);
            }
            return RedirectToAction("Index");
        }

        // ======================
        // VIEW USER DETAILS
        // ======================
        public IActionResult View(int id)
        {
            var user = _service.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // ======================
        // EDIT USER (GET)
        // ======================
        public IActionResult Edit(int id)
        {
            var user = _service.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // ======================
        // EDIT USER (POST)
        // ======================
        [HttpPost]
        public IActionResult Edit(UserM model)
        {
            if (ModelState.IsValid)
            {
                _service.UpdateUser(model);
                _audit.Log(Who, Role, "Update", "User",
                    $"Updated user account: {model.FullName} (Role: {model.Role})", IP);
            }
            return RedirectToAction("Index");
        }

        // ======================
        // DELETE USER
        // ======================
        public IActionResult Delete(int id)
        {
            var user = _service.GetUserById(id);
            _service.DeleteUser(id);
            _audit.Log(Who, Role, "Delete", "User",
                $"Deleted user ID: {id}" + (user != null ? $" ({user.FullName})" : ""), IP);
            return RedirectToAction("Index");
        }
    }
}