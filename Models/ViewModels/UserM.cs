namespace MalikongkongNHS.Models.ViewModels
{
    public class UserM
    {
        public int UserId { get; set; }
        
        public string FullName { get; set; }
        
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Student";
    }
}