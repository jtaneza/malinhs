// Models/Entities/AuditLog.cs
namespace MalikongkongNHS.Models.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int UserId { get; set; } = 0;       // FK to Users/Teachers/Students table
        public string PerformedBy { get; set; } = "";
        public string Role { get; set; } = "";
        public string Action { get; set; } = "";   // Create, Update, Delete, Login, Logout
        public string Module { get; set; } = "";   // Students, Teachers, Payments, etc.
        public string Description { get; set; } = "";
        public string IpAddress { get; set; } = "";
    }
}