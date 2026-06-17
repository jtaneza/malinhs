using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Repositories.Interfaces;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Services.Implementations;

public class GradeLevelService : IGradeLevelService
{
    private readonly IGradeLevelRepository _repository;

    public GradeLevelService(IGradeLevelRepository repository)
    {
        _repository = repository;
    }

    public List<GradeLevel> GetAll()
        => _repository.GetAll();

    public void Add(GradeLevel gradeLevel)
        => _repository.Add(gradeLevel);
}