using System.Text.Json;

namespace Infrastructure;

internal static class JsonSerializer
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    internal static T Deserialize<T>(string json) => System.Text.Json.JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
}