using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product.Microservice.Data;

namespace Product.Microservice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public ProductController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Entities.Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChanges();
        return Ok(product.Id);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _context.Products.ToListAsync();
        if (!customers.Any())
        {
            return NotFound();
        }

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products.Where(a => a.Id == id).FirstOrDefaultAsync();
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.Where(a => a.Id == id).FirstOrDefaultAsync();
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChanges();
        return Ok(product.Id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Entities.Product productData)
    {
        var product = _context.Products.FirstOrDefault(a => a.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        product.Name = productData.Name;
        product.Rate = productData.Rate;
        await _context.SaveChanges();
        return Ok(product.Id);
    }
}
