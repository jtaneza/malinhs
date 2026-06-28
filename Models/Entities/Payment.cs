// Models/Entities/Payment.cs
namespace MalikongkongNHS.Models.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DatePaid { get; set; }

        // New fields
        public string? FeeType { get; set; }       // Tuition, Miscellaneous, etc.
        public string? Status { get; set; }         // Paid, Pending, Overdue
        public string? Notes { get; set; }
        public string? ReceiptNo { get; set; }      // e.g. REC-000001
        public string? Cashier { get; set; }        // from Session

        public virtual Student? Student { get; set; }
    }
}