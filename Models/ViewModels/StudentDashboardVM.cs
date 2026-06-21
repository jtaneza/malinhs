namespace MalikongkongNHS.Models.ViewModels
{
    public class StudentDashboardVM
    {
        public string FullName { get; set; } = string.Empty;
        public int Subjects { get; set; }
        public double AttendanceRate { get; set; }
        public double AverageGrade { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
