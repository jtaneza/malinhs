using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Services.Interfaces
{
    public interface IStudentService
    {
        List<Student> GetAllStudents();
        List<Section> GetSections();

        Student? GetById(int id);

        void Add(Student student);

        void Update(Student student);

        void Delete(int id);

        void ToggleStatus(int id);
    }
}