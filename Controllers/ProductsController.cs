using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsApi.Models;
using System.Security.Claims;

namespace ProductsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private static List<Product> _products = new List<Product>();
    private static int _nextId = 1;

    public ProductsController()
    {
        if (!_products.Any())
        {
            _products.Add(new Product 
            { 
                Id = _nextId++, 
                Name = "Test Product 1", 
                Description = "Description 1", 
                Price = 99.99m, 
                Stock = 10,
                CreatedAt = DateTime.UtcNow
            });
            _products.Add(new Product 
            { 
                Id = _nextId++, 
                Name = "Test Product 2", 
                Description = "Description 2", 
                Price = 149.99m, 
                Stock = 5,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetAll()
    {
        var responses = _products.Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            CreatedAt = p.CreatedAt
        });
        
        return Ok(responses);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public IActionResult GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt
        };
        
        return Ok(response);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        var product = new Product
        {
            Id = _nextId++,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };

        _products.Add(product);

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateProductRequest request)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CreatedAt = product.CreatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        _products.Remove(product);
        return NoContent();
    }
}
