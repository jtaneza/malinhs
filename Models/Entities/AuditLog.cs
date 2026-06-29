namespace MalikongkongNHS.Models.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        // Who did it
        public string PerformedBy { get; set; } = string.Empty;   // username
        public string Role        { get; set; } = string.Empty;   // Admin / Cashier / Teacher

        // What happened
        public string Action      { get; set; } = string.Empty;   // Create / Update / Delete / Login / Logout
        public string Module      { get; set; } = string.Empty;   // Student / Payment / Teacher / Section / User
        public string Description { get; set; } = string.Empty;   // human-readable detail

        // Metadata
        public string? IpAddress  { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
