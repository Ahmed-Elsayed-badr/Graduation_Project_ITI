using BLL.Services.Interfaces;
using DAL;

namespace BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository; // kept for upcoming product-related category logic

        public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.Name = (category.Name ?? string.Empty).Trim();
            category.Comment = (category.Comment ?? string.Empty).Trim();

            if (await _categoryRepository.CategoryNameExistsAsync(category.Name))
            {
                throw new ArgumentException("A category with this name already exists.");
            }

            await _categoryRepository.AddCategoryAsync(category);
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(id);
            if (existingCategory == null)
            {
                return false;
            }

            if (await _categoryRepository.HasProductsASync(id))
            {
                throw new InvalidOperationException("Cannot delete a category that still has products. Move or delete its products first.");
            }

            await _categoryRepository.DeleteCategoryAsync(id);
            return true;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _categoryRepository.GetCategoryByIdAsync(id);
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            // This loads the tracked entity, so we must update THAT instance
            // (attaching a second instance with the same key would throw).
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(category.Id);
            if (existingCategory == null)
            {
                throw new ArgumentException("Category not found.");
            }

            var name = (category.Name ?? string.Empty).Trim();
            if (await _categoryRepository.CategoryNameExistsAsync(name, category.Id))
            {
                throw new ArgumentException("A category with this name already exists.");
            }

            // Only editable fields are copied, so CreatedAt is preserved.
            existingCategory.Name = name;
            existingCategory.Comment = (category.Comment ?? string.Empty).Trim();

            await _categoryRepository.UpdateCategoryAsync(existingCategory);
            return existingCategory;
        }

        public async Task<bool> HasProductsAsync(Guid categoryId)
        {
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (existingCategory == null)
            {
                throw new ArgumentException("Category not found.");
            }
            return await _categoryRepository.HasProductsASync(categoryId);
        }

        public async Task<bool> CategoryNameExistsAsync(string categoryName, Guid? excludeId = null)
        {
            return await _categoryRepository.CategoryNameExistsAsync(categoryName, excludeId);
        }
    }
}
