using FluentValidation;

namespace NotificationPatternExample.Business.Models;

public class BaseValidation<T> : AbstractValidator<T> where T : Entity
{
}