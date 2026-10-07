using Graduation_Project_ITI.Models;

namespace Graduation_Project_ITI.Interfaces
{
    public interface ISellerOrderInterface
    {
        Task<int> GetOrdersCount(Guid sellerId);
        Task<List<SellerOrderListVM>> GetSellerOrders(Guid sellerId);
        Task<SellerOrderDetailsVM?> GetOrderDetails(Guid sellerId, Guid orderId);
        Task<bool> UpdateOrderItemStatus(Guid sellerId, Guid orderItemId, string newStatus);
    }
}
