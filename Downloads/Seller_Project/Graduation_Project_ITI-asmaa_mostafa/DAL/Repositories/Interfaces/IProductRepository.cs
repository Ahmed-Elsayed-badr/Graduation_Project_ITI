using DAL;

public interface IProductRepository
{
    Task<Product?> GetProductByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(Guid id);
    Task<IEnumerable<Product>> GetFilteredProductsAsync(Guid? categoryId, string? searchTerm, bool? sortAscending);
    Task<int> GetAvailableQuantityAsync(Guid productId);
    Task UpdateProductQuantityAsync(Guid productId, int newQuantity);

    // TODO (Auth): remove once the logged-in seller is available from the user session.
    // Returns the first seller in the database so products can be created before login is implemented.
    Task<Guid?> GetDefaultSellerIdAsync();
}
