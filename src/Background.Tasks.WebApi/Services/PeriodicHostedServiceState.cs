namespace Background.Tasks.WebApi.Services;

/// <summary>
///     Represent the current state of the background
/// </summary>
/// <param name="IsEnabled"></param>
public record PeriodicHostedServiceState(bool IsEnabled);