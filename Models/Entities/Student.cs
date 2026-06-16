namespace MalikongkongNHS.Models.Entities;

public class Student
{
    public int StudentId { get; set; }

    public string? StudentNo { get; set; }

    public string LRN { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? ContactNumber { get; set; }

    public string? GuardianName { get; set; }

    public int? SectionId { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Section? Section { get; set; }
}