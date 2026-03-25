using System.Diagnostics;

namespace CineVault.API.Middleware;

public partial class PerformanceLoggingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<PerformanceLoggingMiddleware> logger;

    public PerformanceLoggingMiddleware(RequestDelegate next, ILogger<PerformanceLoggingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await this.next(context);
        }
        finally
        {
            stopwatch.Stop();
            var path = context.Request.Path.Value ?? "";
            var method = context.Request.Method;
            var duration = stopwatch.ElapsedMilliseconds;

            LogRequest(this.logger, method, path, context.Response.StatusCode, duration);

            if (duration > 500)
            {
                LogSlowRequest(this.logger, method, path, duration);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "HTTP {Method} {Path} responded {StatusCode} in {Duration}ms")]
    private static partial void LogRequest(ILogger logger, string method, string path, int statusCode, long duration);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Slow request detected: {Method} {Path} took {Duration}ms")]
    private static partial void LogSlowRequest(ILogger logger, string method, string path, long duration);
}