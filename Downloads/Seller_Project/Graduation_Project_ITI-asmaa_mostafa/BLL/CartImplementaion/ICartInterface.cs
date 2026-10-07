using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.CartInter
{
    public interface ICartInterface
    {
        Task<bool> AddToCart(Guid userId, Guid productId, int quantity);
        Task<bool> RemoveFromCart(Guid userId, Guid productId);
        Task<bool> UpdateCartItem(Guid userId, Guid productId, int quantity);
        Task<List<CartIItem>> GetCartItems(Guid userId);
        Task<decimal> GetCartTotal(Guid userId);
        public Task<bool> Checkout(Guid userId);
        public Task<List<Order>> GetOrderHistory(Guid userId);
    }
}
