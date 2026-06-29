using Microsoft.EntityFrameworkCore;
using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Repositories.Interfaces;

namespace MalikongkongNHS.Repositories.Implementations
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Student> GetAllStudents()
        {
            return _context.Students
                           .Include(s => s.Section)
                           .ToList();
        }

        public List<Section> GetSections()
        {
            return _context.Sections
                .Select(s => new Section
                {
                    SectionId   = s.SectionId,
                    SectionName = s.SectionName ?? "No Section"
                })
                .ToList();
        }

        public Student? GetById(int id)
        {
            return _context.Students
                           .Include(s => s.Section)   // <-- FIXED: loads GradeLevel & SectionName
                           .FirstOrDefault(x => x.StudentId == id);
        }

        public void Add(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();
        }

        public void Update(Student student)
        {
            _context.Students.Update(student);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var student = _context.Students.Find(id);

            if (student != null)
            {
                _context.Students.Remove(student);
                _context.SaveChanges();
            }
        }

        public void ToggleStatus(int id)
        {
            var student = _context.Students.Find(id);

            if (student != null)
            {
                student.IsActive = !student.IsActive;
                _context.SaveChanges();
            }
        }
    }
}
