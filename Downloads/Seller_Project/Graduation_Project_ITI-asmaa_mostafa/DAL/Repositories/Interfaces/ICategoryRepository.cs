using DAL;

public interface ICategoryRepository
{
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Guid id);
    Task<bool> HasProductsASync(Guid categoryId);
    Task<bool> CategoryNameExistsAsync(string categoryName, Guid? excludeId = null);
}
