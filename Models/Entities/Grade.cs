
namespace MalikongkongNHS.Models.Entities
{
    public class Grade
    {
        public int GradeId { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int SectionId { get; set; }          // was ClassId
        public Section Section { get; set; } = null!;

        public int SubjectId { get; set; }
        public virtual Subject? Subject { get; set; }

        public int Quarter { get; set; }             // 1, 2, 3, 4

        public decimal GradeValue { get; set; }      // was Score
        public bool IsFinalized { get; set; }
    }
}