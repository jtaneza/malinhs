namespace MalikongkongNHS.Models.Entities;
 
public class Notification
{
    public int    Id        { get; set; }
    public int    UserId    { get; set; }   // matches Session UserId
    public string Role      { get; set; } = "Student"; // Student | Teacher | Admin | All
    public string Title     { get; set; } = string.Empty;
    public string Message   { get; set; } = string.Empty;
    public string Type      { get; set; } = "general"; // attendance | grade | system | general
    public bool   IsRead    { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
 