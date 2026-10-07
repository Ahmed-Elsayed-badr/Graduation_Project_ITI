// BLL/SellerOrderImplementation/SellerOrderImplement.cs
using BLL.Configuration;
using Graduation_Project_ITI.Interfaces;
using Graduation_Project_ITI.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.SellerOrderImplementation
{
    public class SellerOrderImplement : ISellerOrderInterface
    {
        private readonly AppDbContext _context;

        private static readonly string[] ValidStatuses =
            { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

        public SellerOrderImplement(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetOrdersCount(Guid sellerId)
        {
            return await _context.OrderItems
                .Where(x => x.SellerId == sellerId)
                .Select(x => x.OrderId)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<SellerOrderListVM>> GetSellerOrders(Guid sellerId)
        {
            var items = await _context.OrderItems
                .Where(x => x.SellerId == sellerId)
                .Include(x => x.order)
                    .ThenInclude(o => o.user)
                .ToListAsync();

            return items
                .GroupBy(x => x.OrderId)
                .Select(g => new SellerOrderListVM
                {
                    OrderId = g.Key,
                    OrderDate = g.First().order.OrderDate,
                    CustomerName = g.First().order.user.Name,
                    ItemsCount = g.Sum(x => x.Quantity),
                    Total = g.Sum(x => x.Quantity * x.UnitPrice),
                    Status = g.Select(x => x.Status).Distinct().Count() == 1
                        ? g.First().Status
                        : "Mixed"
                })
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

        public async Task<SellerOrderDetailsVM?> GetOrderDetails(Guid sellerId, Guid orderId)
        {
            var items = await _context.OrderItems
                .Where(x => x.SellerId == sellerId && x.OrderId == orderId)
                .Include(x => x.product)
                .Include(x => x.order)
                    .ThenInclude(o => o.user)
                .ToListAsync();

            if (!items.Any())
                return null;

            var first = items.First();

            // جيب كل الـ reviews الخاصة بالمنتجات الموجودة في الأوردر ده، في كويري واحدة
            var productIds = items.Select(x => x.productId).Distinct().ToList();

            var reviewStats = await _context.Reviews
                .Where(r => productIds.Contains(r.ProductId))
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Average = g.Average(r => r.Rating),
                    Count = g.Count()
                })
                .ToListAsync();

            return new SellerOrderDetailsVM
            {
                OrderId = orderId,
                OrderDate = first.order.OrderDate,
                CustomerName = first.order.user.Name,
                CustomerEmail = first.order.user.Email,
                 Status = first.order.Status,
                Items = items.Select(x =>
                {
                    var stats = reviewStats.FirstOrDefault(r => r.ProductId == x.productId);

                    return new SellerOrderItemVM
                    {
                        OrderItemId = x.Id,
                        ProductId = x.productId,
                        ProductName = x.product.Name,
                        ImageUrl = x.product.ImageUrl,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice,
                        Status = x.Status,
                        AverageRating = stats != null ? Math.Round(stats.Average, 1) : 0,
                        ReviewsCount = stats?.Count ?? 0
                    };
                }).ToList()
            };
        }

        public async Task<bool> UpdateOrderItemStatus(Guid sellerId, Guid orderItemId, string newStatus)
        {
            if (!ValidStatuses.Contains(newStatus))
                return false;

            // الشرط ده هو اللي بيمنع الـ seller إنه يعدّل على item مش بتاعه
            var item = await _context.OrderItems
                .FirstOrDefaultAsync(x => x.Id == orderItemId && x.SellerId == sellerId);

            if (item == null)
                return false;

            item.Status = newStatus;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}