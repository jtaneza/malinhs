namespace MalikongkongNHS.Services.Interfaces
{
    public interface IAuditService
    {
        void Log(string performedBy, string role, string action, string module, string description, string? ipAddress = null, int userId = 0);
    }
}
