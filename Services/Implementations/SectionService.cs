using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Repositories.Interfaces;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Services.Implementations;

public class SectionService : ISectionService
{
    private readonly ISectionRepository _sectionRepository;

    public SectionService(ISectionRepository sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public List<Section> GetAll()
    {
        return _sectionRepository.GetAll();
    }

    public Section? GetById(int id)
    {
        return _sectionRepository.GetById(id);
    }

    public void Add(Section section)
    {
        _sectionRepository.Add(section);
    }

    public void Update(Section section)
    {
        _sectionRepository.Update(section);
    }

    public void Delete(int id)
    {
        _sectionRepository.Delete(id);
    }
}