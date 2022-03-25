using FluentValidation;

namespace NotificationPatternExample.Business.Models.Validations;

public class ProductValidation : BaseValidation<Product>
{
    public ProductValidation()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("The field {PropertyName} must be informed")
            .Length(2, 200)
            .WithMessage("The field {PropertyName} must have between {MinLength} and {MaxLength} characters");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("The field {PropertyName} must be informed")
            .Length(2, 1000)
            .WithMessage("The field {PropertyName} must have between {MinLength} and {MaxLength} characters");

        RuleFor(c => c.Price)
            .GreaterThan(0).WithMessage("The field {PropertyName} must be greater than {ComparisonValue}");
    }
}