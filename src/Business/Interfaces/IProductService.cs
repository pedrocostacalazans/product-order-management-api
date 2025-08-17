using ProductOrderSystem.Domain.Entities;

namespace ProductOrderSystem.Business;

public interface IProductService
{
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
}