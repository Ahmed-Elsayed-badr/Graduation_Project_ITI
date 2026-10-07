namespace Graduation_Project_ITI.Models
{
    public class SellerOrderListVM
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public int ItemsCount { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
    }
}
