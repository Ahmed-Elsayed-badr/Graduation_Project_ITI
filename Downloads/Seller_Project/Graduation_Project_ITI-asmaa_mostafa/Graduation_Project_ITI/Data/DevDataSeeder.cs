using BLL.Configuration;
using DAL;

namespace Graduation_Project_ITI.Data
{
    // بيانات تجريبية للتطوير فقط - بتتضاف مرة واحدة لو جدول المنتجات فاضي
    // Development-only demo data. Runs only when the Products table is empty.
    public static class DevDataSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (db.Products.Any())
                return; // في منتجات بالفعل، ما تعملش حاجة

            // ===== البائع التجريبي (السلسلة المطلوبة قبل أي منتج) =====
            // Re-use an existing seller if there is one, otherwise create a demo seller.
            var seller = db.Sellers.FirstOrDefault();
            if (seller == null)
            {
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

                // NOTE: Role / NetRole rows are not needed to add products; the auth module will create them.

                seller = new Seller
                {
                    ShopName = "Demo Store",
                    user = user,
                    IsApproved = 1,
                    Status = true
                };
            }

            // ===== الأقسام (get-or-create لأن اسم القسم Unique) =====
            Category GetOrCreateCategory(string name, string comment) =>
                db.Categories.FirstOrDefault(c => c.Name == name)
                ?? new Category { Name = name, Comment = comment };

            var electronics = GetOrCreateCategory("Electronics", "Phones, laptops and accessories");
            var fashion = GetOrCreateCategory("Fashion", "Clothes and shoes");
            var home = GetOrCreateCategory("Home & Kitchen", "Kitchen tools and home items");

            // ===== المنتجات =====
            Product NewProduct(string name, string description, decimal price, int quantity, Category category) =>
                new Product
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    AvailableQuantity = quantity,
                    ImageUrl = "/images/placeholder.png",
                    Category = category,
                    Seller = seller
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
    }
}
