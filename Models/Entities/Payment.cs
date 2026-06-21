namespace MalikongkongNHS.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public decimal Amount { get; set; }

        public DateTime DatePaid { get; set; }
    }
}