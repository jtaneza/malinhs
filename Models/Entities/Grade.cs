namespace MalikongkongNHS.Models.Entities
{
    public class Grade
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int ClassId { get; set; }

        public double Score { get; set; }
        public bool IsFinalized { get; set; }
    }
}