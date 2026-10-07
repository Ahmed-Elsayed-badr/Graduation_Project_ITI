namespace Graduation_Project_ITI.Models
{
    public class ProductListViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        public int AvailableQuantity { get; set; }
    }
}