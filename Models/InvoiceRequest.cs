namespace Models
{
    public class InvoiceRequest
    {
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Address { get; set; }
        public List<InvoiceItem> Items { get; set; }
        public string Date {get; set;}
    }
    public class InvoiceItem
    {
        public string ItemName { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

}

