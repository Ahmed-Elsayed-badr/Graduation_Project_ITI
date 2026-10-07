namespace Graduation_Project_ITI.Models
{
    public class SellerOrderDetailsVM
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }

        public List<SellerOrderItemVM> Items { get; set; }
    }
}
