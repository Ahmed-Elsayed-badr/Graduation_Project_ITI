namespace Graduation_Project_ITI.Models.Admin
{
    public class AdminOrderDetailsViewModel
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }

        public List<AdminOrderItemViewModel> Items { get; set; } = new();
    }

    public class AdminOrderItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string SellerName { get; set; } = "-";
        public string? ShopName { get; set; }

        public decimal LineTotal => Quantity * UnitPrice;
    }
}