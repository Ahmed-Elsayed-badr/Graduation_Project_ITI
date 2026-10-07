namespace Graduation_Project_ITI.ViewModels
{
    public class ProductReviewsSummaryVM
    {
        public List<ReviewVM> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
        public bool CanReview { get; set; }
    }
}
