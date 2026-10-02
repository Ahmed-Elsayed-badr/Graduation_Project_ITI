using BLL.Services.Interfaces;
using DAL;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _productRepository.GetProductByIdAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(product.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Invalid category. Please choose an existing category.");
            }

            // TODO (Auth): the seller should come from the logged-in user. Until then, fall back to the first seller.
            if (product.SellerId == Guid.Empty)
            {
                var sellerId = await _productRepository.GetDefaultSellerIdAsync();
                if (sellerId == null)
                {
                    throw new ArgumentException("No seller exists yet. Create a seller before adding products.");
                }
                product.SellerId = sellerId.Value;
            }

            product.Name = product.Name.Trim();
            product.Description = product.Description.Trim();

            await _productRepository.AddProductAsync(product);
            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            // This loads the tracked entity, so we must update THAT instance
            // (attaching a second instance with the same key would throw).
            var existingProduct = await _productRepository.GetProductByIdAsync(product.Id);
            if (existingProduct == null)
            {
                throw new ArgumentException("Product not found.");
            }

            var category = await _categoryRepository.GetCategoryByIdAsync(product.CategoryId);
            if (category == null)
            {
                throw new ArgumentException("Invalid category. Please choose an existing category.");
            }

            existingProduct.Name = product.Name.Trim();
            existingProduct.Description = product.Description.Trim();
            existingProduct.Price = product.Price;
            existingProduct.AvailableQuantity = product.AvailableQuantity;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.CategoryId = product.CategoryId;
            // SellerId is intentionally NOT changed on edit (ownership stays with the original seller).

            await _productRepository.UpdateProductAsync(existingProduct);
            return existingProduct;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            var existingProduct = await _productRepository.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return false;
            }
            await _productRepository.DeleteProductAsync(id);
            return true;
        }

        public async Task<IEnumerable<Product>> GetFilteredProductsAsync(Guid? categoryId, string? searchTerm, bool? sortAscending)
        {
            return await _productRepository.GetFilteredProductsAsync(categoryId, searchTerm, sortAscending);
        }

        public async Task<int> GetAvailableQuantityAsync(Guid productId)
        {
            return await _productRepository.GetAvailableQuantityAsync(productId);
        }

        public async Task UpdateProductQuantityAsync(Guid productId, int newQuantity)
        {
            if (newQuantity < 0)
            {
                throw new ArgumentException("Quantity cannot be negative.");
            }

            var existingProduct = await _productRepository.GetProductByIdAsync(productId);
            if (existingProduct == null)
            {
                throw new ArgumentException("Product not found.");
            }
            await _productRepository.UpdateProductQuantityAsync(productId, newQuantity);
        }

        public async Task<Guid?> GetDefaultSellerIdAsync()
        {
            return await _productRepository.GetDefaultSellerIdAsync();
        }
    }
}
