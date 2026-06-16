namespace MalikongkongNHS.Models.Entities;

public class Section
{
    public int SectionId { get; set; }

    public string SectionName { get; set; } = string.Empty;

    public ICollection<Student> Students { get; set; }
        = new List<Student>();
}