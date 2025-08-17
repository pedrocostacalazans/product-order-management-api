using ProductOrderSystem.Domain.Exceptions;

namespace ProductOrderSystem.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    private OrderItem()
    {
        Id = Guid.NewGuid();
        ProductName = string.Empty;
    }

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity) : this()
    {
        if (productId == Guid.Empty)
            throw new BusinessException("ProductId is required");
        if (string.IsNullOrWhiteSpace(productName))
            throw new BusinessException("Product name is required");
        if (unitPrice < 0)
            throw new BusinessException("Unit price cannot be negative");
        if (quantity <= 0)
            throw new BusinessException("Item quantity must be greater than zero");

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("Quantity must be positive");
        Quantity += quantity;
    }

    public void UpdateUnitPrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new BusinessException("Unit price cannot be negative");
        UnitPrice = newPrice;
    }
}