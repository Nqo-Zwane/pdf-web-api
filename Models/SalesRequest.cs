namespace Models
{
    public class SalesRequest
    {
        public List<SalesItem> SalesItems { get; set; }
        public string ReportTitle { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
    }
    public class SalesItem
    {
        public string ProductName { get; set; } // The name of the product or service.
        public int Quantity { get; set; }       // The quantity of the product or service sold.
        public decimal Revenue { get; set; }    // The revenue generated from selling the product or service.
    }

}
