#nullable enable

using System.Net;
using Polly;
using Polly.Bulkhead;

namespace Reliability.Middlewares;

public class BulkheadMiddleware
{
    private readonly ILogger<BulkheadMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly AsyncPolicy _bulkhead;

    /// <summary>
    /// максимальное распараллеливание исполнений через Bulkhead;
    /// </summary>
    private const int BulkheadMaxParallelization = 60;
    
    /// <summary>
    /// максимальное количество действий, которые могут быть в очереди (ожидании получения слота выполнения) в любое время.
    /// </summary>
    private const int BulkheadMaxQueuingActions = 20;

    private static readonly List<string> _notSkipBulkheadEndpoints = new()
    {
        Controllers.ReliabilityController.BulkheadUrl
    };

    private static bool SkipBulkhead(HttpContext context) =>
        !(context.Request.Path.HasValue &&
          _notSkipBulkheadEndpoints
              .Any(
                  endpoint =>
                      context.Request.Path.StartsWithSegments(endpoint, StringComparison.InvariantCultureIgnoreCase)));

    public BulkheadMiddleware(ILogger<BulkheadMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
        _bulkhead = Policy.BulkheadAsync(BulkheadMaxParallelization, BulkheadMaxQueuingActions);
    }

    public async Task Invoke(HttpContext context)
    {
        if (SkipBulkhead(context))
        {
            await _next(context);
            return;
        }

        try
        {
            await _bulkhead.ExecuteAsync(() => _next(context));
        }
        catch (BulkheadRejectedException)
        {
            _logger.LogWarning(
                "Bulkhead rejected request: {Path}. Remote IP: {RemoteIpAddress}",
                context.Request.Path,
                context.Connection.RemoteIpAddress);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        }
    }
}