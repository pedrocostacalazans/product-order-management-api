using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductOrderSystem.Api.DTOs;
using ProductOrderSystem.Business;
using ProductOrderSystem.Data.Contexts;

namespace ProductOrderSystem.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.CreateAsync(request.CustomerName, request.Items.Select(i => (i.ProductId, i.Quantity)), ct);

        var response = new OrderResponse(
            order.Id,
            order.CustomerName,
            order.CreatedAt,
            order.Total,
            order.Items.Select(i => new OrderItemResponse(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()
        );

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, [FromServices] AppDbContext db, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return NotFound();
        var response = new OrderResponse(
            order.Id,
            order.CustomerName,
            order.CreatedAt,
            order.Total,
            order.Items.Select(i => new OrderItemResponse(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity)).ToList()
        );
        return Ok(response);
    }
}