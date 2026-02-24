namespace Gateway.Framework.Core.Models;

/// <summary>
/// Standardized API response wrapper for all gateway responses.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    public List<ApiError>? Errors { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse Ok(object? data = null, string? message = null) =>
        new() { Success = true, StatusCode = 200, Data = data, Message = message };

    public static ApiResponse Created(object? data = null, string? message = null) =>
        new() { Success = true, StatusCode = 201, Data = data, Message = message };

    public static ApiResponse Fail(string message, int statusCode = 400, List<ApiError>? errors = null) =>
        new() { Success = false, StatusCode = statusCode, Message = message, Errors = errors };

    public static ApiResponse ValidationFail(List<ApiError> errors) =>
        new() { Success = false, StatusCode = 422, Message = "Validation failed.", Errors = errors };
}

/// <summary>
/// Typed API response wrapper.
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    public new T? Data { get; set; }

    public static ApiResponse<T> Ok(T? data, string? message = null) =>
        new() { Success = true, StatusCode = 200, Data = data, Message = message };
}
