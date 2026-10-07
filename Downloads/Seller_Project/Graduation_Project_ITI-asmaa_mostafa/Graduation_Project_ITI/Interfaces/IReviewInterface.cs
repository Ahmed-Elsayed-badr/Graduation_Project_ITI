using Graduation_Project_ITI.ViewModels;

namespace BLL.ReviewInter
{
    public interface IReviewInterface
    {
        Task<List<ReviewVM>> GetProductReviews(Guid productId);
        Task<double> GetAverageRating(Guid productId);
        Task<bool> HasUserPurchasedProduct(Guid userId, Guid productId);
        Task<bool> HasUserAlreadyReviewed(Guid userId, Guid productId);
        Task<bool> AddReview(Guid userId, Guid productId, int rating, string comment);
    }
}
