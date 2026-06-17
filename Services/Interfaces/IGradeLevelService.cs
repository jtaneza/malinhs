using MalikongkongNHS.Models.Entities;

namespace MalikongkongNHS.Services.Interfaces;

public interface IGradeLevelService
{
    List<GradeLevel> GetAll();
    void Add(GradeLevel gradeLevel);
}