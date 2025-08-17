using Microsoft.AspNetCore.Mvc;
using ProductOrderSystem.Api.DTOs;
using ProductOrderSystem.Business;
using ProductOrderSystem.Domain.Entities;

namespace ProductOrderSystem.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.CreateAsync(
            new Product(request.Name, request.Description, request.Price, request.StockQuantity), ct);

        var response = new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<ProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(CancellationToken ct)
    {
        var products = await _productService.GetAllAsync(ct);
        var response = products.Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.StockQuantity));
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product is null) return NotFound();
        return Ok(new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.StockQuantity));
    }
}