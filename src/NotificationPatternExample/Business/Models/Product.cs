using NotificationPatternExample.ViewModels;

namespace NotificationPatternExample.Business.Models;

public class Product : Entity
{
    public Guid ProductId { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool InStock { get; set; }

    public ProductViewModel ToViewModel()
    {
        var productViewModel = new ProductViewModel
        {
            Name = Name, Description = Description, Price = Price, InStock = InStock
        };

        return productViewModel;
    }
}
