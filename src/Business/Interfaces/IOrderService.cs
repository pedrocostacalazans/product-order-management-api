using ProductOrderSystem.Domain.Entities;

namespace ProductOrderSystem.Business;

public interface IOrderService
{
    Task<Order> CreateAsync(string customerName, IEnumerable<(Guid productId, int quantity)> items, CancellationToken ct = default);
}