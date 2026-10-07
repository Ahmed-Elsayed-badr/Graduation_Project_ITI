namespace Graduation_Project_ITI.Models
{
    public class SellerOrderItemVM
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Status { get; set; }
        public decimal TotalPrice { get; set; }

        // new
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
    }
}
