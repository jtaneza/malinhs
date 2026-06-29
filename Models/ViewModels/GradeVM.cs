namespace MalikongkongNHS.Models.ViewModels
{
    // ── Teacher: grade management index ─────────────────────────
    public class GradeIndexVM
    {
        public int    SectionId   { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string GradeLevel  { get; set; } = string.Empty;
        public bool   IsAdviser   { get; set; }
        public int    Quarter     { get; set; } = 1;

        public List<SectionSwitcherItemVM> AvailableSections { get; set; } = new();
        public List<GradeSubjectGroupVM>   SubjectGroups     { get; set; } = new();
    }

    public class GradeSubjectGroupVM
    {
        public int    SubjectId   { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public bool   IsFinalized { get; set; }

        public List<GradeStudentRowVM> Students { get; set; } = new();
    }

    public class GradeStudentRowVM
    {
        public int      StudentId { get; set; }
        public string   FullName  { get; set; } = string.Empty;
        public string   LRN       { get; set; } = string.Empty;

        public decimal? Q1 { get; set; }
        public decimal? Q2 { get; set; }
        public decimal? Q3 { get; set; }

        public decimal? Average => ComputeAverage();
        private decimal? ComputeAverage()
        {
            var scores = new[] { Q1, Q2, Q3 }.Where(q => q.HasValue).Select(q => q!.Value).ToList();
            if (!scores.Any()) return null;
            return Math.Round(scores.Average(), 2);
        }

        public string Remarks => Average.HasValue ? (Average >= 75 ? "Passed" : "Failed") : "—";
    }

    // ── Teacher: grade entry form ────────────────────────────────
    public class GradeEntryVM
    {
        public int    SectionId   { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int    SubjectId   { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int    Quarter     { get; set; }

        public List<GradeEntryRowVM> Students { get; set; } = new();
    }

    public class GradeEntryRowVM
    {
        public int      StudentId   { get; set; }
        public string   FullName    { get; set; } = string.Empty;
        public string   LRN         { get; set; } = string.Empty;
        public decimal? Score       { get; set; }
        public int?     GradeId     { get; set; }
        public bool     IsFinalized { get; set; }
    }

    // ── Teacher: edit single grade ───────────────────────────────
    public class GradeEditVM
    {
        public int     GradeId     { get; set; }
        public string  StudentName { get; set; } = string.Empty;
        public string  SubjectName { get; set; } = string.Empty;
        public int     Quarter     { get; set; }
        public decimal OldScore    { get; set; }
        public decimal NewScore    { get; set; }
        public int     SectionId   { get; set; }
        public int     SubjectId   { get; set; }
    }

    // ── Student: view own grades ─────────────────────────────────
    public class StudentGradeReportVM
    {
        public string StudentName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string GradeLevel  { get; set; } = string.Empty;

        public List<StudentSubjectGradeVM> Subjects { get; set; } = new();
    }

    public class StudentSubjectGradeVM
    {
        public string   SubjectName { get; set; } = string.Empty;
        public decimal? Q1          { get; set; }
        public decimal? Q2          { get; set; }
        public decimal? Q3          { get; set; }

        public decimal? Average
        {
            get
            {
                var scores = new[] { Q1, Q2, Q3 }.Where(q => q.HasValue).Select(q => q!.Value).ToList();
                return scores.Any() ? Math.Round(scores.Average(), 2) : (decimal?)null;
            }
        }

        public string Remarks => Average.HasValue ? (Average >= 75 ? "Passed" : "Failed") : "—";
    }
}