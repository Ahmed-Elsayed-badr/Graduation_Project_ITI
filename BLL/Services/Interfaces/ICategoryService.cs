using DAL;

namespace BLL.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<bool> HasProductsAsync(Guid categoryId);
        Task<bool> CategoryNameExistsAsync(string categoryName, Guid? excludeId = null);
    }
}
