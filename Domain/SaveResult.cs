using System.Diagnostics.CodeAnalysis;

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
    
    public override string ToString() => Success ? "Success" : $"Error: {ErrorMessage}";
}

public class SaveResult<T>
{
    [MemberNotNullWhen(true, nameof(Data))]
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool Success { get; private init; }
    public string? ErrorMessage { get; private init; }
    public T? Data { get; private init; }
    
    public static SaveResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static SaveResult<T> Fail(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };
    
    public override string ToString() => Success ? $"Success {Data?.ToString()}" : $"Error: {ErrorMessage}";
}