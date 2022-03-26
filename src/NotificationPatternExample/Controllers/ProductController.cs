using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NotificationPatternExample.Business;
using NotificationPatternExample.Business.Interfaces;
using NotificationPatternExample.Business.Models;
using NotificationPatternExample.ViewModels;

namespace NotificationPatternExample.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly INotificator _notificator;
    private readonly ProductService _productService;

    public ProductController(INotificator notificator, ProductService productService)
    {
        (_notificator, _productService) = (notificator, productService);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductViewModel>>> GetAll()
    {
        try
        {
            var products = await _productService.List().ConfigureAwait(false);
            var viewModels = products.Select(p => p.ToViewModel());
            return Ok(viewModels);
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
                var errorList = PullErrors(ModelState);
                return BadRequest(errorList);
            }

            var productModel = productViewModel.ToModel();
            await _productService.Add(productModel).ConfigureAwait(false);
            if (_notificator.HasNotification())
            {
                return BadRequest(_notificator.GetNotifications());
            }

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

    private static IEnumerable<Notification> PullErrors(ModelStateDictionary modelStateDictionary)
    {
        return modelStateDictionary.Values
            .SelectMany(v =>
                v.Errors.Select(e => new Notification(e.ErrorMessage)));
    }
}
