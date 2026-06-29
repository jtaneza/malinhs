namespace MalikongkongNHS.Models.Entities
{
    public class Activity
    {
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;  // ← initialized

        public DateTime CreatedAt { get; set; }
    }
}