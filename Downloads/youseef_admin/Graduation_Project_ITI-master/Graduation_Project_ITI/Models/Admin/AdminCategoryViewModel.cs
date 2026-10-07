namespace Graduation_Project_ITI.Models.Admin
{
    public class AdminCategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ProductsCount { get; set; }

        public bool CanBeDeleted => ProductsCount == 0;
    }
}