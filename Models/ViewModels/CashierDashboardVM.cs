namespace MalikongkongNHS.Models.ViewModels
{
    public class CashierDashboardVM
    {
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalPayments { get; set; }
        public int PaidStudents { get; set; }
        public int PendingPayments { get; set; }
        public decimal RevenueToday { get; set; }
    }
}
