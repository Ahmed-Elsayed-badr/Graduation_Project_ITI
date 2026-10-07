using BLL.Configuration;
using DAL;
using Microsoft.EntityFrameworkCore;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        // Include Category so the views can show the category name.
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProductAsync(Product product)
    {
        // If the entity is already tracked (loaded earlier in the same request) EF detects the changes by itself.
        // Calling Update() on a tracked graph would also mark related entities (e.g. Category) as modified.
        if (_context.Entry(product).State == EntityState.Detached)
        {
            _context.Products.Update(product);
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Product>> GetFilteredProductsAsync(Guid? categoryId, string? searchTerm, bool? sortAscending)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(p => p.Name.Contains(term));
        }

        // Default order is by name so the list is stable when no price sort is chosen.
        query = sortAscending switch
        {
            true => query.OrderBy(p => p.Price),
            false => query.OrderByDescending(p => p.Price),
            _ => query.OrderBy(p => p.Name)
        };

        return await query.ToListAsync();
    }

    public async Task<int> GetAvailableQuantityAsync(Guid productId)
    {
        var product = await _context.Products.FindAsync(productId);
        return product?.AvailableQuantity ?? 0;
    }

    public async Task UpdateProductQuantityAsync(Guid productId, int newQuantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            product.AvailableQuantity = newQuantity;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Guid?> GetDefaultSellerIdAsync()
    {
        return await _context.Sellers
            .AsNoTracking()
            .OrderBy(s => s.CreatedAt)
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync();
    }
}
