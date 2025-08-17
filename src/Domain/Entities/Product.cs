using ProductOrderSystem.Domain.Exceptions;

namespace ProductOrderSystem.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }

    private Product()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
        Description = string.Empty;
    }

    public Product(string name, string description, decimal price, int stockQuantity) : base()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Product name is required");
        if (price < 0)
            throw new BusinessException("Product price cannot be negative");
        if (stockQuantity < 0)
            throw new BusinessException("Stock quantity cannot be negative");

        Name = name;
        Description = description ?? string.Empty;
        Price = price;
        StockQuantity = stockQuantity;
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new BusinessException("Product price cannot be negative");
        Price = newPrice;
    }

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Product name is required");
        Name = name;
        Description = description ?? string.Empty;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("Quantity to increase must be positive");
        StockQuantity += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessException("Quantity to decrease must be positive");
        if (StockQuantity < quantity)
            throw new BusinessException($"Insufficient stock. Available: {StockQuantity}, Requested: {quantity}");
        StockQuantity -= quantity;
    }
}