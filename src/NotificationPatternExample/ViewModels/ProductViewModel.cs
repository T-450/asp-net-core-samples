using System.ComponentModel.DataAnnotations;
using NotificationPatternExample.Business.Models;

namespace NotificationPatternExample.ViewModels;

public class ProductViewModel
{
    [Required(ErrorMessage = "The field {0} is required")]
    [StringLength(200, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 2)]
    public string Name { get; set; }

    [Required(ErrorMessage = "The field {0} is required")]
    [StringLength(1000, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 2)]
    public string Description { get; set; }

    [Required(ErrorMessage = "The field {0} is required")]
    public decimal Price { get; set; }

    [ScaffoldColumn(false)] public DateTime CreatedAt { get; set; }

    public bool InStock { get; set; } = true;

    public Product ToModel()
    {
        var product = new Product
        {
            Name = Name,
            Description = Description,
            Price = Price,
            InStock = InStock
        };
        return product;
    }
}