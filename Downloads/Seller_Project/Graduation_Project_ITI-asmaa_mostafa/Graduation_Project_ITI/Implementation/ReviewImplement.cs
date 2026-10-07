// BLL/ReviewImplementation/ReviewImplement.cs
using BLL.Configuration;
using BLL.ReviewInter;
using DAL;
using Graduation_Project_ITI.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BLL.ReviewImplementation
{
    public class ReviewImplement : IReviewInterface
    {
        private readonly AppDbContext _context;

        public ReviewImplement(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReviewVM>> GetProductReviews(Guid productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.user)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewVM
                {
                    Id = r.Id,
                    ReviewerName = r.user.Name,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<double> GetAverageRating(Guid productId)
        {
            var ratings = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Select(r => r.Rating)
                .ToListAsync();

            return ratings.Any() ? Math.Round(ratings.Average(), 1) : 0;
        }

        public async Task<bool> HasUserPurchasedProduct(Guid userId, Guid productId)
        {
            // العميل لازم يكون عنده OrderItem للمنتج ده جوه أوردر بتاعه
            return await _context.OrderItems
                .Include(oi => oi.order)
                .AnyAsync(oi =>
                    oi.productId == productId &&
                    oi.order.userId == userId);
        }

        public async Task<bool> HasUserAlreadyReviewed(Guid userId, Guid productId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.ProductId == productId && r.userId == userId);
        }

        public async Task<bool> AddReview(Guid userId, Guid productId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
                return false;

            if (string.IsNullOrWhiteSpace(comment))
                return false;

            var purchased = await HasUserPurchasedProduct(userId, productId);
            if (!purchased)
                return false;

            var alreadyReviewed = await HasUserAlreadyReviewed(userId, productId);
            if (alreadyReviewed)
                return false;

            var review = new Reviews
            {
                ProductId = productId,
                userId = userId,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}