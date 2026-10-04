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

        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();

        // True only if the logged-in user bought this product and hasn't reviewed it yet
        public bool CanReview { get; set; }
    }
}