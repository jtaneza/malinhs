using System.Collections.Generic;

namespace MalikongkongNHS.Models.ViewModels
{
    public class TeacherClassVM
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public string Adviser { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool IsAdviser { get; set; }      // true = advisory class, false = subject teacher only
        public List<StudentListItemVM> Students { get; set; } = new();
    }

    public class StudentListItemVM
    {
        public int StudentId { get; set; }
        public string StudentNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string GuardianName { get; set; } = string.Empty;
    }
}