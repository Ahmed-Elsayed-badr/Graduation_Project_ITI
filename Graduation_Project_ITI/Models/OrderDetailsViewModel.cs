namespace Graduation_Project_ITI.Models
{
    public class OrderItemLineViewModel
    {
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class OrderDetailsViewModel
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemLineViewModel> Items { get; set; } = new List<OrderItemLineViewModel>();
    }
}