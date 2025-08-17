namespace ProductOrderSystem.Api.DTOs;

public record CreateProductRequest(string Name, string Description, decimal Price, int StockQuantity);
public record ProductResponse(Guid Id, string Name, string Description, decimal Price, int StockQuantity);