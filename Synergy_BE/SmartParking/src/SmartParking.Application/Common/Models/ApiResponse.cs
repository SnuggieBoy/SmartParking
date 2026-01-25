namespace SmartParking.Application.Common.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public T? Data { get; init; }
    public IEnumerable<string>? Errors { get; init; }
    public DateTime Timestamp { get; init; }

    private ApiResponse(bool success, string message, T? data, IEnumerable<string>? errors)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors;
        Timestamp = DateTime.UtcNow;
    }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request completed successfully")
    {
        return new ApiResponse<T>(true, message, data, null);
    }

    public static ApiResponse<T> FailureResponse(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>(false, message, default, errors);
    }

    public static ApiResponse<T> FailureResponse(string message, string error)
    {
        return new ApiResponse<T>(false, message, default, new[] { error });
    }
}

// For endpoints that don't return data
public sealed class ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public IEnumerable<string>? Errors { get; init; }
    public DateTime Timestamp { get; init; }

    private ApiResponse(bool success, string message, IEnumerable<string>? errors)
    {
        Success = success;
        Message = message;
        Errors = errors;
        Timestamp = DateTime.UtcNow;
    }

    public static ApiResponse SuccessResponse(string message = "Request completed successfully")
    {
        return new ApiResponse(true, message, null);
    }

    public static ApiResponse FailureResponse(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse(false, message, errors);
    }

    public static ApiResponse FailureResponse(string message, string error)
    {
        return new ApiResponse(false, message, new[] { error });
    }
}
