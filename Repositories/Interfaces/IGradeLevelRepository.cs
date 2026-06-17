using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Repositories.Interfaces;

public interface IGradeLevelRepository
{
    List<GradeLevel> GetAll();
    void Add(GradeLevel gradeLevel);
}