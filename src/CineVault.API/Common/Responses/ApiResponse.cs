namespace CineVault.API.Common.Responses;

public class ApiResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public required string RequestId { get; set; }
    public required string ApiVersion { get; set; } = "v3";
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(string requestId, T data, string message = "Success")
        => new()
        {
            Success = true,
            Data = data,
            Message = message,
            RequestId = requestId,
            ApiVersion = "v3"
        };

    public static ApiResponse<T> Fail(string requestId, string message)
        => new()
        {
            Success = false,
            Message = message,
            RequestId = requestId,
            ApiVersion = "v3"
        };
}