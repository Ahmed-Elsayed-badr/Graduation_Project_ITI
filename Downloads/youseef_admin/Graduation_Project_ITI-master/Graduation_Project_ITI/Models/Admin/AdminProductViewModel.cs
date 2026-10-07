namespace Graduation_Project_ITI.Models.Admin
{
    public class AdminProductViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public string? ShopName { get; set; }
        public int ReviewsCount { get; set; }
        public int OrdersCount { get; set; }

        public bool CanBeRemoved => OrdersCount == 0;
    }
}