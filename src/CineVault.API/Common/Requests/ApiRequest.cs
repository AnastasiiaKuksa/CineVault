namespace CineVault.API.Common.Requests;

public class ApiRequest
{
    public required string RequestId { get; set; }
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    public string? ClientVersion { get; set; }
    public required string ClientName { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ApiRequest<T> : ApiRequest
{
    public T? Data { get; set; }
}