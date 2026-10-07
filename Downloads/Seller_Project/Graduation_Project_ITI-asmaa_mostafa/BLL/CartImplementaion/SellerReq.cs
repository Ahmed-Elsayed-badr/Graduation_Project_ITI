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
    public class SellerReq
    {
        public AppDbContext dbContext;
        public SellerReq(AppDbContext DbContext)
        {
            DbContext = dbContext;
        }
        public SellerRequest sellerRequest(User user)
        {

            if (user.Role == RoleStatus.Customer.ToString())
            {
                var sell = new SellerRequest
                {
                    userId = user.Id,
                    Status = SellerStatus.pending.ToString(),
                    CreatedAt = DateTime.Now
                };
                dbContext.SellerRequests.Add(sell);
                dbContext.SaveChanges();

                return sell;
            }
            return null;
        }

        public SellerRequest UpdateRoleUSer(SellerRequest sell)
        {
            var seller = dbContext.SellerRequests.Include(x => x.user).FirstOrDefault(x => x.Id == sell.Id);
            if (seller == null)
            {
                return null;
            }
            if (sell.Status == SellerStatus.Approved.ToString())
            {
                sell.user.Role = RoleStatus.Seller.ToString();
                dbContext.SaveChanges();
                return sell;
            }
            else
                return null;
        }
    }
}
