namespace Domain;

public class SaveResult
{
    public bool Success { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static SaveResult Ok() => new() { Success = true };

    public static SaveResult Fail(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };
}