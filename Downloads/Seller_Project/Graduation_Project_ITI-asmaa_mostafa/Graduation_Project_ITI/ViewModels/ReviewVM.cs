namespace Graduation_Project_ITI.ViewModels
{
    public class ReviewVM
    {
        public Guid Id { get; set; }
        public string ReviewerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
