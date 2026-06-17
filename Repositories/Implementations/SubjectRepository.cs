using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Repositories.Interfaces;

namespace MalikongkongNHS.Repositories.Implementations;

public class SubjectRepository : ISubjectRepository
{
    private readonly ApplicationDbContext _context;

    public SubjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Subject> GetAll()
        => _context.Subjects.ToList();

    public void Add(Subject subject)
    {
        _context.Subjects.Add(subject);
        _context.SaveChanges();
    }
}
