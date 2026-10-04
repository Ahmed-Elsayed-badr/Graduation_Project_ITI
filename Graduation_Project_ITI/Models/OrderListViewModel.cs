namespace Graduation_Project_ITI.Models
{
    public class OrderListViewModel
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsCount { get; set; }
    }
}