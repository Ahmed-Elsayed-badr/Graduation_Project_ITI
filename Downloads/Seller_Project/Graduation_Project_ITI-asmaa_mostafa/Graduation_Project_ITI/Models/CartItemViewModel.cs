namespace Graduation_Project_ITI.Models
{
    public class CartItemViewModel
    {
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}