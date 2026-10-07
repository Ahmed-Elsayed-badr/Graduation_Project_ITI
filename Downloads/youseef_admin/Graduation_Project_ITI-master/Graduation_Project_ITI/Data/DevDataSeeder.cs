using BLL.Configuration;
using DAL;
using Microsoft.AspNetCore.Identity;

namespace Graduation_Project_ITI.Data
{
    // بيانات تجريبية للتطوير فقط - كل جزء بيتضاف مرة واحدة لو مش موجود
    public static class DevDataSeeder
    {
        public static void Seed(AppDbContext db)
        {
            SeedDemoStore(db);
            SeedAdmin(db);
            SeedDemoCustomer(db);
            SeedPendingSellerRequest(db);
            SeedDemoOrders(db);
        }

        // ===== البائع التجريبي + الأقسام + المنتجات (المنطق الأصلي كما هو) =====
        private static void SeedDemoStore(AppDbContext db)
        {
            if (db.Products.Any())
                return; // في منتجات بالفعل، ما تعملش حاجة

            // ===== البائع التجريبي (السلسلة المطلوبة قبل أي منتج) =====
            var wishList = new WishList();

            var user = new User
            {
                Name = "Demo Seller",
                Email = "seller@demo.com",
                PasswordHash = "demo-only",
                Role = "Seller",
                IsActive = 1,
                WishList = wishList
            };

            var netRole = new NetRole { NormalizedName = "SELLER" };

            var role = new Role
            {
                Name = "Seller",
                user = user,
                netrole = netRole
            };

            var seller = new Seller
            {
                ShopName = "Demo Store",
                user = user,
                IsApproved = 1,
                Status = true
            };

            // ===== الأقسام =====
            var electronics = new Category { Name = "Electronics", Comment = "Phones, laptops and accessories" };
            var fashion = new Category { Name = "Fashion", Comment = "Clothes and shoes" };
            var home = new Category { Name = "Home & Kitchen", Comment = "Kitchen tools and home items" };

            // ===== المنتجات =====
            Product NewProduct(string name, string description, decimal price, int quantity, Category category) =>
                new Product
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    AvailableQuantity = quantity,
                    ImageUrl = "/images/placeholder.png",
                    category = category,
                    Seller = seller,
                    Role = role
                };

            db.Products.AddRange(
                NewProduct("iPhone 15 128GB", "Apple iPhone 15 with 128GB storage, 6.1 inch display and dual camera.", 42000m, 5, electronics),
                NewProduct("Samsung Galaxy A55", "Samsung mid-range phone with 8GB RAM, 256GB storage and 5000mAh battery.", 18500m, 12, electronics),
                NewProduct("Lenovo IdeaPad 3 Laptop", "15.6 inch laptop with Intel Core i5, 8GB RAM and 512GB SSD. Good for study.", 27000m, 7, electronics),
                NewProduct("HP Victus Gaming Laptop", "Gaming laptop with RTX 4050, Intel Core i7, 16GB RAM and 1TB SSD.", 55000m, 3, electronics),
                NewProduct("Sony Wireless Headphones", "Over-ear wireless headphones with noise cancelling and 30 hours battery.", 9500m, 0, electronics),
                NewProduct("Men's Cotton T-Shirt", "Comfortable 100% cotton t-shirt, available in black, white and navy.", 350m, 40, fashion),
                NewProduct("Women's Running Shoes", "Lightweight running shoes with breathable mesh and soft sole.", 1800m, 15, fashion),
                NewProduct("Non-stick Frying Pan 28cm", "28cm non-stick frying pan, suitable for gas and electric stoves.", 650m, 20, home)
            );

            db.SaveChanges();
        }

        // ===== حساب الـ Administrator =====
        // Email: admin@demo.com | Password: Admin@123
        private static void SeedAdmin(AppDbContext db)
        {
            if (db.Users.Any(u => u.Email == "admin@demo.com"))
                return;

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "System Admin",
                Email = "admin@demo.com",
                IsActive = 1,
                Role = "Administrator",
                WishList = new WishList()
            };

            admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, "Admin@123");

            db.Users.Add(admin);
            db.SaveChanges();
        }

        // ===== عميل تجريبي =====
        // Email: customer@demo.com | Password: Customer@123
        private static void SeedDemoCustomer(AppDbContext db)
        {
            if (db.Users.Any(u => u.Email == "customer@demo.com"))
                return;

            var customer = new User
            {
                Id = Guid.NewGuid(),
                Name = "Demo Customer",
                Email = "customer@demo.com",
                phonenumber = "01000000000",
                IsActive = 1,
                Role = "Customer",
                WishList = new WishList()
            };

            customer.PasswordHash = new PasswordHasher<User>().HashPassword(customer, "Customer@123");

            db.Users.Add(customer);
            db.SaveChanges();
        }

        // ===== مستخدم عادي عنده طلب بائع (Pending) =====
        // Email: pending@demo.com | Password: Pending@123
        private static void SeedPendingSellerRequest(AppDbContext db)
        {
            if (db.Users.Any(u => u.Email == "pending@demo.com"))
                return;

            var netRole = db.NetRoles.FirstOrDefault(n => n.NormalizedName == "SELLER");
            if (netRole == null)
            {
                netRole = new NetRole { NormalizedName = "SELLER" };
                db.NetRoles.Add(netRole);
                db.SaveChanges();
            }

            var applicant = new User
            {
                Id = Guid.NewGuid(),
                Name = "Pending Applicant",
                Email = "pending@demo.com",
                IsActive = 1,
                Role = "Customer",
                WishList = new WishList()
            };

            applicant.PasswordHash = new PasswordHasher<User>().HashPassword(applicant, "Pending@123");

            db.Users.Add(applicant);
            db.SaveChanges();

            db.SellerRequests.Add(new SellerRequest
            {
                userId = applicant.Id,
                RoleId = netRole.Id,
                Status = "Pending",
                CreatedAt = DateTime.Now
            });

            db.SaveChanges();
        }

        // ===== طلبات تجريبية للعميل التجريبي =====
        private static void SeedDemoOrders(AppDbContext db)
        {
            if (db.Orders.Any())
                return;

            var customer = db.Users.FirstOrDefault(u => u.Email == "customer@demo.com");
            if (customer == null)
                return;

            var products = db.Products.OrderBy(p => p.Name).Take(3).ToList();
            if (products.Count < 3)
                return;

            OrderItem NewItem(Product product, int quantity) =>
                new OrderItem
                {
                    productId = product.Id,
                    SellerId = product.sellerId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };

            var firstItems = new List<OrderItem> { NewItem(products[0], 1), NewItem(products[1], 2) };
            var secondItems = new List<OrderItem> { NewItem(products[2], 1) };

            var firstOrder = new Order
            {
                userId = customer.Id,
                Status = "Pending",
                OrderDate = DateTime.Now.AddDays(-1),
                OrderItems = firstItems,
                TotalAmount = firstItems.Sum(i => i.UnitPrice * i.Quantity)
            };

            var secondOrder = new Order
            {
                userId = customer.Id,
                Status = "Shipped",
                OrderDate = DateTime.Now.AddDays(-3),
                OrderItems = secondItems,
                TotalAmount = secondItems.Sum(i => i.UnitPrice * i.Quantity)
            };

            db.Orders.AddRange(firstOrder, secondOrder);
            db.SaveChanges();
        }
    }
}