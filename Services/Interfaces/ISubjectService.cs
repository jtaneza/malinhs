using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Services.Interfaces;

public interface ISubjectService
{
    List<Subject> GetAll();
    void Add(Subject subject);
}
