namespace MalikongkongNHS.Models.ViewModels
{
    public class AttendanceIndexVM
    {
        public string SectionName          { get; set; } = string.Empty;
        public int    SectionId            { get; set; }
        public bool   AttendanceTakenToday { get; set; }
        public int    PresentToday         { get; set; }
        public int    AbsentToday          { get; set; }
        public int    LateToday            { get; set; }
        public int    ExcusedToday         { get; set; }
        public int    TotalStudents        { get; set; }
        public bool   IsAdviser            { get; set; }   // whether teacher is adviser of selected section

        // All sections this teacher is associated with (for dropdown)
        public List<SectionSwitcherItemVM> AvailableSections { get; set; } = new();

        public List<StudentAttendanceSummaryVM> StudentSummaries { get; set; } = new();
        public List<AttendanceHistoryDayVM>     RecentHistory    { get; set; } = new();
    }

    public class SectionSwitcherItemVM
    {
        public int    SectionId   { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string GradeLevel  { get; set; } = string.Empty;
        public bool   IsAdviser   { get; set; }
    }

    public class StudentAttendanceSummaryVM
    {
        public int     StudentId          { get; set; }
        public string  FullName           { get; set; } = string.Empty;
        public string  LRN                { get; set; } = string.Empty;
        public int     Present            { get; set; }
        public int     Absent             { get; set; }
        public int     Late               { get; set; }
        public int     Excused            { get; set; }
        public int     Total              { get; set; }

        // ── NEW ──
        public int?    TodayAttendanceId  { get; set; }
        public string? TodayStatus        { get; set; }

        public double AttendanceRate => Total == 0 ? 0 : Math.Round((Present + Late) * 100.0 / Total, 1);
    }

    public class AttendanceHistoryDayVM
    {
        public DateTime                 Date    { get; set; }
        public List<AttendanceRecordVM> Records { get; set; } = new();
    }

    public class AttendanceRecordVM
    {
        public string FullName { get; set; } = string.Empty;
        public string LRN      { get; set; } = string.Empty;
        public string Status   { get; set; } = string.Empty;
    }

    public class AttendanceTakeVM
    {
        public int      SectionId   { get; set; }
        public string   SectionName { get; set; } = string.Empty;
        public DateTime Date        { get; set; } = DateTime.Today;
        public List<StudentAttendanceInputVM> Students { get; set; } = new();
    }

    public class StudentAttendanceInputVM
    {
        public int    StudentId { get; set; }
        public string FullName  { get; set; } = string.Empty;
        public string LRN       { get; set; } = string.Empty;
        public string Status    { get; set; } = "Present";
    }
}