namespace MalikongkongNHS.Models.Entities;

public class Attendance
{
    public int AttendanceId { get; set; }
    public int StudentId   { get; set; }
    public int SectionId   { get; set; }
    public int TeacherId   { get; set; }
    public DateTime Date   { get; set; }

    // "Present", "Absent", "Late", "Excused"
    public string Status { get; set; } = "Present";

    public virtual Student? Student { get; set; }
    public virtual Section? Section { get; set; }
}