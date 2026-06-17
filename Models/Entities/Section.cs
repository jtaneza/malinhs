namespace MalikongkongNHS.Models.Entities;

public class Section
{
    public int SectionId { get; set; }

    public string SectionName { get; set; } = string.Empty;

    public string? GradeLevel { get; set; }
    public string? Adviser { get; set; }
    public int? Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Student> Students { get; set; } = new List<Student>();
}