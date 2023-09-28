namespace Models
{
    public class ReportRequest
    {
        public string period { get; set; }
        public decimal totalRevenue { get; set; }
        public decimal totalExpenses { get; set; }
        public decimal netIncome { get; set; }
    }

}