using MalikongkongNHS.Data;
using MalikongkongNHS.Models.Entities;
using MalikongkongNHS.Services.Interfaces;

namespace MalikongkongNHS.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Log(string performedBy, string role, string action, string module, string description, string? ipAddress = null)
        {
            var log = new AuditLog
            {
                PerformedBy = performedBy,
                Role        = role,
                Action      = action,
                Module      = module,
                Description = description,
                IpAddress   = ipAddress,
                Timestamp   = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();
        }
    }
}
