using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Repositories.Interfaces;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repository;

    public SubjectService(ISubjectRepository repository)
    {
        _repository = repository;
    }

    public List<Subject> GetAll()
        => _repository.GetAll();

    public void Add(Subject subject)
        => _repository.Add(subject);
}
