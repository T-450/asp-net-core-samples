using System.Text.Json;

namespace WebApiFundamentals.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    ///     To json.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="options"></param>
    /// <returns>The Json of any object.</returns>
    public static string ToJson(this object value, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return JsonSerializer.Serialize(value, options);
    }
}
