namespace MalikongkongNHS.Models.ViewModels
{
    public class StudentAttendanceVM
    {
        public string StudentName   { get; set; } = string.Empty;
        public string SectionName   { get; set; } = string.Empty;
        public int    TotalDays     { get; set; }
        public int    Present       { get; set; }
        public int    Absent        { get; set; }
        public int    Late          { get; set; }
        public int    Excused       { get; set; }
        public double AttendanceRate { get; set; }

        public List<StudentAttendanceDayVM> Records { get; set; } = new();
    }

    public class StudentAttendanceDayVM
    {
        public DateTime Date   { get; set; }
        public string   Status { get; set; } = string.Empty;
    }
}
