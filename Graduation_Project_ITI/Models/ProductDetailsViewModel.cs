namespace Graduation_Project_ITI.Models
{
    public class ProductDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
        public string SellerName { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
    }
}