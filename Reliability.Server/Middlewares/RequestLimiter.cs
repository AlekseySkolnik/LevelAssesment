using JetBrains.Annotations;
using Reliability.Extensions;
using Reliability.Services;

namespace Reliability.Middlewares;

public class RequestLimiter
{
    private readonly RequestDelegate _next;
		
    public RequestLimiter(RequestDelegate next)
    {
        _next = next;
    }

    [UsedImplicitly]
    public async Task InvokeAsync(HttpContext context, IRequestLimitingService requestLimitingService)
    {
        var request = CreateLimitRequest(context);
        var needBlock = await requestLimitingService.NeedBlockRequest(request, context.RequestAborted);

        if (needBlock)
        {
            await context.BadRequest("blocked");
            return;
        }
			
        await _next(context);
    }

    private static LimitRequest CreateLimitRequest(HttpContext context)
    {
        var routingValues = context.GetRouteData().Values;
        var clientIdentifiers = ExtractClientIdentifiers(context).ToList();
        return new LimitRequest(
            context.Request.Path.ToString(),
            new RoutingValues(context.Request.Method,
                routingValues.GetValueOrDefault("controller") as string,
                routingValues.GetValueOrDefault("action") as string),
            clientIdentifiers,
            context.GetDeviceId());
    }

    private static IEnumerable<ClientIdentifierInfo> ExtractClientIdentifiers(HttpContext context)
    {
        yield return new (ClientIdentifierTypes.DeviceId, context.GetDeviceId());
        yield return new (ClientIdentifierTypes.ClientIp, context.GetClientIp());
			
        var userId = context.GetClientUUId();
        if (userId != null)
        {
            yield return new (ClientIdentifierTypes.ClientUUId, userId.ToString());
        }
        var workflowId = context.GetWorkflowId();
        if (Guid.TryParse(workflowId, out _))
        {
            yield return new (ClientIdentifierTypes.WorkflowId, workflowId);
        }
    }
}