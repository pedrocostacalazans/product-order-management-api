namespace ProductOrderSystem.Api.DTOs;

public record CreateOrderItem(Guid ProductId, int Quantity);
public record CreateOrderRequest(string CustomerName, List<CreateOrderItem> Items);
public record OrderItemResponse(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record OrderResponse(Guid Id, string CustomerName, DateTime CreatedAt, decimal Total, List<OrderItemResponse> Items);