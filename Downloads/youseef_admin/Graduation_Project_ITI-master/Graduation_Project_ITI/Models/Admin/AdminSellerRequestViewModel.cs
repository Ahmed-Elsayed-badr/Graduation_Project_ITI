namespace Graduation_Project_ITI.Models.Admin
{
    public class AdminSellerRequestViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? ShopName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ReviewedAt { get; set; }

        public bool IsPending =>
            string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);

        public bool HasBeenReviewed => ReviewedAt.Year > 1;
    }
}