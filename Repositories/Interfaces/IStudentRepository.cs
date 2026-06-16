using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        List<Student> GetAllStudents();

        Student? GetById(int id);

        void Add(Student student);

        void Update(Student student);

        void Delete(int id);
    }
}