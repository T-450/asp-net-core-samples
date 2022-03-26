using FluentValidation;
using FluentValidation.Results;

namespace NotificationPatternExample.Business.Models.Validations;

public class ProductValidation : AbstractValidator<Product>
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

    public new (bool isValid, IEnumerable<Notification> errors) Validate(Product product)
    {
        var result = base.Validate(product);
        var errors = result.Errors ?? new List<ValidationFailure>();
        return (result.IsValid, errors.Select(e => new Notification(e.ErrorMessage)));
    }
}
