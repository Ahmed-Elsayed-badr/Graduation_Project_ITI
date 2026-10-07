using BLL.CartInter;
using BLL.Configuration;
using DAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.CartImplementaion
{
    public  class CartImplement : ICartInterface
    {
        private readonly AppDbContext Context;
        public CartImplement(AppDbContext context)
        {
            Context = context;
        }
        public async Task<List<CartIItem>> GetCartItems(Guid userId)
        {
            return await Context.CartIItems.Where(x => x.cart.userId == userId)
                                .Include(x => x.product).ToListAsync();
        }

        public async Task<bool> AddToCart(Guid userId, Guid productId, int quantity)
        {
            if (quantity <= 0)
                return false;

            var cart = await Context.Carts.FirstOrDefaultAsync(x => x.userId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    userId = userId
                };

                Context.Carts.Add(cart);
                await Context.SaveChangesAsync();
            }

            var product = await Context.Products
                .FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
                return false;

            var cartItem = await Context.CartIItems
                .FirstOrDefaultAsync(x =>
                    x.CartId == cart.Id &&
                    x.productId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartIItem
                {
                    CartId = cart.Id,
                    productId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };

                Context.CartIItems.Add(cartItem);
            }

            await Context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveFromCart(Guid userId, Guid productId)
        {
            var rem = Context.CartIItems
                .FirstOrDefault(x => x.cart.userId == userId && x.productId == productId);
            if (rem == null)
            {
                return false;
            }

            Context.CartIItems.Remove(rem);
            Context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartItem(Guid userId, Guid productId, int quantity)
        {
            if (quantity <= 0)
                return false;

            var item = await Context.CartIItems.FirstOrDefaultAsync(x =>
                    x.cart.userId == userId &&
                    x.productId == productId);

            if (item == null)
                return false;

            item.Quantity = quantity;

            await Context.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetCartTotal(Guid userId)
        {
            return await Context.CartIItems.Where(x => x.cart.userId == userId)
                                           .SumAsync(x => x.Quantity * x.UnitPrice);
        }

        public async Task<bool> Checkout(Guid userId)
        {

            var cart = await Context.Carts
                                    .Include(c => c.cartIItems)
                                    .ThenInclude(x => x.product)
                                    .FirstOrDefaultAsync(c => c.userId == userId);

            if (cart == null || !cart.cartIItems.Any())
                return false;

            var totalAmount = cart.cartIItems
                .Sum(x => x.Quantity * x.UnitPrice);

            var order = new Order
            {
                userId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.pending.ToString(),
                TotalAmount = totalAmount
            };

            foreach (var item in cart.cartIItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    productId = item.productId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SellerId = item.product.SellerId,
                    Status = OrderStatus.pending.ToString()
                };

                order.OrderItems.Add(orderItem);
            }

            Context.Orders.Add(order);

            Context.CartIItems.RemoveRange(cart.cartIItems);

            await Context.SaveChangesAsync();

            return true;
        }

        public async Task<List<Order>> GetOrderHistory(Guid userId)
        {
            return await Context.Orders.Where(x => x.userId == userId)
                                .Include(x => x.OrderItems)
                                .ThenInclude(x => x.product)
                                .OrderByDescending(x => x.OrderDate)
                                .ToListAsync();
        }
    }
}
