using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Repositories.Interfaces;

namespace MalikongkongNHS.Repositories.Implementations;

public class SectionRepository : ISectionRepository
{
    private readonly ApplicationDbContext _context;

    public SectionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Section> GetAll()
    {
        return _context.Sections.ToList();
    }

    public Section? GetById(int id)
    {
        return _context.Sections.Find(id);
    }

    public void Add(Section section)
    {
        _context.Sections.Add(section);
        _context.SaveChanges();
    }

    public void Update(Section section)
    {
        _context.Sections.Update(section);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var section = _context.Sections.Find(id);

        if (section != null)
        {
            _context.Sections.Remove(section);
            _context.SaveChanges();
        }
    }
}