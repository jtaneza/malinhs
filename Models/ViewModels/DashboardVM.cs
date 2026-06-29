namespace MalikongkongNHS.Models.ViewModels
{
    public class DashboardVM
    {
        public int TotalStudents  { get; set; }
        public int TotalTeachers  { get; set; }
        public int TotalSections  { get; set; }
        public int TotalUsers     { get; set; }

        // Monthly enrollment data for chart (last 8 months)
        public List<string> MonthLabels        { get; set; } = new();
        public List<int>    MonthlyEnrollment  { get; set; } = new();

        // Recent activities from the Activities table
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
    }

    public class RecentActivityItem
    {
        public string Message   { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}