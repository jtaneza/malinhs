using Microsoft.AspNetCore.Mvc;
using MalikongkongNHS.Models.ViewModels;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

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
            }

            return RedirectToAction("Index");
        }

        // ======================
        // VIEW USER DETAILS
        // ======================
        public IActionResult View(int id)
        {
            var user = _service.GetUserById(id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // ======================
        // EDIT USER (GET)
        // ======================
        public IActionResult Edit(int id)
        {
            var user = _service.GetUserById(id);

            if (user == null)
                return NotFound();

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
            }

            return RedirectToAction("Index");
        }

        // ======================
        // DELETE USER
        // ======================
        public IActionResult Delete(int id)
        {
            _service.DeleteUser(id);
            return RedirectToAction("Index");
        }
    }
}