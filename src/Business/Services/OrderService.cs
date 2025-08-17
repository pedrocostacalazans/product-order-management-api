using Microsoft.EntityFrameworkCore;
using ProductOrderSystem.Data.Contexts;
using ProductOrderSystem.Domain.Entities;
using ProductOrderSystem.Domain.Exceptions;

namespace ProductOrderSystem.Business;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    public OrderService(AppDbContext db) => _db = db;

    public async Task<Order> CreateAsync(string customerName, IEnumerable<(Guid productId, int quantity)> items, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new BusinessException("Customer name is required");

        var itemList = items?.ToList() ?? new List<(Guid productId, int quantity)>();
        if (itemList.Count == 0)
            throw new BusinessException("Order must contain at least one item");

        var productIds = itemList.Select(i => i.productId).ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);
        if (products.Count != productIds.Count)
            throw new BusinessException("One or more products were not found");

        foreach (var (productId, quantity) in itemList)
        {
            if (quantity <= 0)
                throw new BusinessException("Item quantity must be greater than zero");
            var product = products.First(p => p.Id == productId);
            if (product.StockQuantity < quantity)
                throw new BusinessException($"Insufficient stock for product '{product.Name}'");
        }

        var order = new Order(customerName);

        foreach (var (productId, quantity) in itemList)
        {
            var product = products.First(p => p.Id == productId);
            product.DecreaseStock(quantity);

            order.AddItem(product.Id, product.Name, product.Price, quantity);
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        return order;
    }
}