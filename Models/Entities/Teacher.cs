namespace MalikongkongNHS.Models.Entities;

public class Teacher
{
    public int TeacherId { get; set; }
    public int? SubjectId { get; set; }
    public string FullName { get; set; } = string.Empty;

    // Educational Credentials
    public string? Credentials { get; set; }

    // Account Credentials
    public string? Email { get; set; }
    public string? Password { get; set; }
}