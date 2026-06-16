using Microsoft.AspNetCore.Mvc;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ApplicationDbContext _context;

        public StudentController(
            IStudentService studentService,
            ApplicationDbContext context)
        {
            _studentService = studentService;
            _context = context;
        }

        // =========================
        // LIST
        // =========================

        public IActionResult Index()
        {
            var students = _studentService.GetAllStudents();

            return View(students);
        }

        // =========================
        // CREATE
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Sections = _context.Sections.ToList();

            return View(new Student());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Student student)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sections = _context.Sections.ToList();

                return View(student);
            }

            _studentService.Add(student);

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT
        // =========================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var student = _studentService.GetById(id);

            if (student == null)
            {
                return NotFound();
            }

            ViewBag.Sections = _context.Sections.ToList();

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Student student)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sections = _context.Sections.ToList();

                return View(student);
            }

            _studentService.Update(student);

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _studentService.Delete(id);

            return RedirectToAction(nameof(Index));
        }
    }
}