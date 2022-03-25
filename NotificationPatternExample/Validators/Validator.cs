using FluentValidation;
using FluentValidation.Results;
using NotificationPatternExample.Business.Models;
using NotificationPatternExample.Business.Notifications;

namespace NotificationPatternExample.Validators;

public static class Validator
{
    private static readonly Notificator _notificator = new();

    public static bool Validate<TEntity>(TEntity entity, AbstractValidator<TEntity>? validation = null)
        where TEntity : Entity
    {
        ArgumentNullException.ThrowIfNull(entity);
        validation ??= new BaseValidation<TEntity>();
        var validator = validation.Validate(entity);
        if (validator.IsValid) Notify(validator);
        return validator.IsValid;
    }

    private static void Notify(IEnumerable<Notification> notifications)
    {
        _notificator.Handle(notifications);
    }

    private static void Notify(ValidationResult validationResult)
    {
        var notifications = validationResult.Errors
            .Select(e => e.ErrorMessage)
            .Select(e => new Notification(e));

        Notify(notifications);
    }
}