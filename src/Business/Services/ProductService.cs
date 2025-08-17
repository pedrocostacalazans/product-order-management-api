using Microsoft.EntityFrameworkCore;
using ProductOrderSystem.Data.Contexts;
using ProductOrderSystem.Domain.Entities;
using ProductOrderSystem.Domain.Exceptions;

namespace ProductOrderSystem.Business;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    public ProductService(AppDbContext db) => _db = db;

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new BusinessException("Product name is required");
        if (product.Price < 0)
            throw new BusinessException("Product price cannot be negative");
        if (product.StockQuantity < 0)
            throw new BusinessException("Stock quantity cannot be negative");

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
        => _db.Products.AsNoTracking().ToListAsync(ct);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
}