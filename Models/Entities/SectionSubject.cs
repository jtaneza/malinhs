namespace MalikongkongNHS.Models.Entities;

public class SectionSubject
{
    public int Id         { get; set; }
    public int SectionId  { get; set; }
    public int SubjectId  { get; set; }
    public int TeacherId  { get; set; }

    public string? Time { get; set; }
    public string? RoomNumber { get; set; }

    public virtual Section? Section  { get; set; }
    public virtual Subject? Subject  { get; set; }
    public virtual Teacher? Teacher  { get; set; }
}
