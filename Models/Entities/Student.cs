namespace MalikongkongNHS.Models.Entities;

public class Student
{
    public int StudentId { get; set; }

    public string? StudentNo { get; set; }
    public string? LRN { get; set; }

    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? ContactNumber { get; set; }
    public string? GuardianName { get; set; }

    // Account Credentials
    public string? Email { get; set; }
    public string? Password { get; set; }

    public int? SectionId { get; set; }
    public bool IsActive { get; set; }

    public virtual Section? Section { get; set; }
}