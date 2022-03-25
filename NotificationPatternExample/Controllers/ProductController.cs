using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationPatternExample.Business.Interfaces;
using NotificationPatternExample.Business.Models;
using NotificationPatternExample.Validators;
using NotificationPatternExample.ViewModels;
using DbContext = NotificationPatternExample.Data.DbContext;

namespace NotificationPatternExample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly DbContext _context;
    private readonly INotificator _notificator;

    public ProductController(DbContext context, INotificator notificator)
    {
        (_context, _notificator) = (context, notificator);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductViewModel>>> GetAll()
    {
        try
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();
            return Ok(products);
        }
        catch (Exception e)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "The house is burning down!"
            );
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductViewModel>> Create(ProductViewModel productViewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                return BadRequest(errors);
            }

            var productModel = productViewModel.ToModel();
            var isValidProduct = Validator.Validate(productModel);
            if (!isValidProduct)
                return BadRequest(_notificator.GetNotification());

            _context.Products.Add(productModel);
            await _context.SaveChangesAsync();
            return Ok(productViewModel);
        }
        catch (Exception e)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "The house is burning down!"
            );
        }
    }
}