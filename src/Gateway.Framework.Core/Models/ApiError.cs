namespace Gateway.Framework.Core.Models;

/// <summary>
/// Represents a structured error in API responses.
/// </summary>
public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
    public string? Detail { get; set; }

    public ApiError() { }

    public ApiError(string code, string message, string? field = null)
    {
        Code = code;
        Message = message;
        Field = field;
    }
}
