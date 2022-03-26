using FluentValidation;
using FluentValidation.Results;
using NotificationPatternExample.Business.Interfaces;
using NotificationPatternExample.Business.Models;
using NotificationPatternExample.Business.Notifications;

namespace NotificationPatternExample.Validators;

public static class Validator
{
    private static readonly Notificator Notificator;

    public static ValidationResult Validate<TValidation, TEntity>(TEntity entity, TValidation validation)
        where TValidation: AbstractValidator<TEntity>
        where TEntity : Entity
    {
        ArgumentNullException.ThrowIfNull(entity);
        return validation.Validate(entity);
    }

    public static ValidationResult ValidateAndNotify<TValidation, TEntity>(TEntity entity, TValidation validation)
        where TValidation: AbstractValidator<TEntity>
        where TEntity : Entity
    {
        ArgumentNullException.ThrowIfNull(entity);
        var result = validation.Validate(entity);
        if (!result.IsValid) Notify(result);
        return result;
    }

    private static void Notify(IEnumerable<Notification> notifications)
    {
        Notificator.Handle(notifications);
    }

    private static void Notify(ValidationResult validationResult)
    {
        var notifications = validationResult.Errors
            .Select(e => e.ErrorMessage)
            .Select(e => new Notification(e));

        Notify(notifications);
    }
}
