using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Repositories.Interfaces;

public interface ISubjectRepository
{
    List<Subject> GetAll();
    void Add(Subject subject);
}
