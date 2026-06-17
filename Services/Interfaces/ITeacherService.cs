using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Services.Interfaces
{
    public interface ITeacherService
    {
        List<Teacher> GetAllTeachers();
        Teacher GetById(int id);
        void Add(Teacher teacher);
        void Update(Teacher teacher);
        void Delete(int id);
    }
}