using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Repositories.Interfaces;

namespace MalikongkongNHS.Repositories.Implementations;

public class GradeLevelRepository : IGradeLevelRepository
{
    private readonly ApplicationDbContext _context;

    public GradeLevelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<GradeLevel> GetAll()
        => _context.GradeLevels.ToList();

    public void Add(GradeLevel gradeLevel)
    {
        _context.GradeLevels.Add(gradeLevel);
        _context.SaveChanges();
    }
}