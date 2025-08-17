using ProductOrderSystem.Domain.Exceptions;

namespace ProductOrderSystem.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(i => i.UnitPrice * i.Quantity);

    private Order()
    {
        Id = Guid.NewGuid();
        CustomerName = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public Order(string customerName)
        : this()
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new BusinessException("Customer name is required");
        CustomerName = customerName;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("Item quantity must be greater than zero");
        if (unitPrice < 0)
            throw new BusinessException("Unit price cannot be negative");
        if (string.IsNullOrWhiteSpace(productName))
            throw new BusinessException("Product name is required");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is null)
        {
            _items.Add(new OrderItem(productId, productName, unitPrice, quantity));
        }
        else
        {
            existing.IncreaseQuantity(quantity);
        }
    }
}