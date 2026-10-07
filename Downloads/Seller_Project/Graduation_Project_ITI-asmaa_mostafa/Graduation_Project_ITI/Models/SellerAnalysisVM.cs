
namespace Graduation_Project_ITI.Models
{
    public class SellerAnalysisVM
    {
        // ============================================
        // Seller Info
        // ============================================

        public string SellerName { get; set; }


        // ============================================
        // Main KPIs
        // ============================================

        public decimal TotalSales { get; set; }

        public int TotalOrders { get; set; }

        public int TotalProductsSold { get; set; }

        public int TotalProducts { get; set; }

        public decimal AverageOrderValue { get; set; }


        // ============================================
        // Order Status
        // ============================================

        public int CompletedOrders { get; set; }

        public int PendingOrders { get; set; }

        public int CancelledOrders { get; set; }

        public int ConfirmedOrders { get; set; }

        public int ShippedOrders { get; set; }


        // ============================================
        // Growth
        // ============================================

        public decimal SalesGrowthPercentage { get; set; }

        public decimal OrdersGrowthPercentage { get; set; }


        // ============================================
        // Trend Data
        // ============================================

        public List<string> TrendLabels { get; set; } = new();

        public List<decimal> SalesTrend { get; set; } = new();

        public List<int> OrdersTrend { get; set; } = new();


        // ============================================
        // Top Products
        // ============================================

        public List<SellerAnalysisProductVM> TopProducts { get; set; }
            = new();


        // ============================================
        // Category Analytics
        // ============================================

        public List<SellerAnalysisCategoryVM> CategorySales { get; set; }
            = new();


        // ============================================
        // Recent Orders
        // ============================================

        public List<SellerAnalysisOrderVM> RecentOrders { get; set; }
            = new();


        // ============================================
        // Ratings
        // ============================================

        public decimal AverageRating { get; set; }

        public int TotalReviews { get; set; }

        public List<SellerAnalysisRatingVM> ProductRatings { get; set; }
            = new();
    }


    // ==================================================
    // TOP PRODUCT
    // ==================================================

    public class SellerAnalysisProductVM
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public string ImageUrl { get; set; }

        public int QuantitySold { get; set; }

        public decimal Sales { get; set; }
    }


    // ==================================================
    // CATEGORY
    // ==================================================

    public class SellerAnalysisCategoryVM
    {
        public string CategoryName { get; set; }

        public int QuantitySold { get; set; }

        public decimal Sales { get; set; }
    }


    // ==================================================
    // RECENT ORDER
    // ==================================================

    public class SellerAnalysisOrderVM
    {
        public Guid OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public string CustomerName { get; set; }

        public string Status { get; set; }

        public decimal Total { get; set; }

        public int ItemsCount { get; set; }
    }


    // ==================================================
    // PRODUCT RATING
    // ==================================================

    public class SellerAnalysisRatingVM
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public string ImageUrl { get; set; }

        public decimal AverageRating { get; set; }

        public int ReviewCount { get; set; }
    }
}

