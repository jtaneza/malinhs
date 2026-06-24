
namespace MalikongkongNHS.Models.Entities
{
    public class Grade
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int ClassId { get; set; }          // ClassId = SectionId
        public Section Section { get; set; }

        public int SubjectId { get; set; }         // which subject this grade is for
        public virtual Subject? Subject { get; set; }

        public int Quarter { get; set; }          // 1, 2, 3, 4

        public double Score { get; set; }
        public bool IsFinalized { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}