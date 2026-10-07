using DAL;

namespace BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(Guid id);
        Task<IEnumerable<Product>> GetFilteredProductsAsync(Guid? categoryId, string? searchTerm, bool? sortAscending);
        Task<int> GetAvailableQuantityAsync(Guid productId);
        Task UpdateProductQuantityAsync(Guid productId, int newQuantity);

        // TODO (Auth): replace with the logged-in seller once authentication is implemented.
        Task<Guid?> GetDefaultSellerIdAsync();
    }
}
