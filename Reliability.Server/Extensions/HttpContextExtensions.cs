using System.Net;
using System.Security.Claims;

namespace Reliability.Extensions;

public static class HttpContextExtensions
{
    private static class Header
    {
        public const string DeviceId = "DeviceId";
        public const string ClientIp = "X-Real-IP";
        public const string ClientApp = "Client";
        public const string ClientVersion = "ClientVersion";
        public const string ClientVersionBuild = "ClientVersionBuild";
        public const string LanguageCode = "LanguageCode";
        public const string LocalityId = "LocalityId";
        public const string CountryCode = "CountryCode";
        public const string ApiVersion = "ApiVersion";
        public const string RequestId = "x-request-id";
        public const string UserAgent = "User-Agent";
        public const string BlackLabel = "BlackLabel";
        public const string ContentType = "content-type";
        public const string ContentLength = "content-length";
        public const string WorkflowId = "WorkflowId";
        public const string IfModifiedSince = "If-Modified-Since";
        public const string CaptchaToken = "VerificationToken";

        /// <summary>
        /// Indicates that the request was passed through the gateway
        /// </summary>
        public const string XGateway = "X-Gateway";
    }

    private const string ItemIsBotKey = "mapi-isBot";

    public static string GetDeviceId(this HttpContext context)
    {
        return GetHeaderOrDefault(context, Header.DeviceId);
    }

    public static string GetClientIp(this HttpContext context)
    {
        var request = context.Request;
        if (request.Headers.TryGetValue(Header.ClientIp, out var realIp) && realIp.Count > 0)
        {
            return realIp;
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        var remoteIpstring = remoteIp?.ToString() ?? "-";

        return remoteIpstring;
    }

    public static string GetWorkflowId(this HttpContext context)
    {
        return GetHeaderOrDefault(context, Header.WorkflowId);
    }

    public static Guid GetClientUUId(this HttpContext context)
    {
        return Guid.TryParse(GetUserClaimOrDefault(context, ClaimTypes.Name, null), out Guid result)
            ? result
            : Guid.Empty;
    }

    private static string GetHeaderOrDefault(
        HttpContext context,
        string header,
        string defaultValue = "")
    {
        if (context.Request.Headers.TryGetValue(header, out var values) && values.Count > 0)
        {
            return values;
        }

        return defaultValue;
    }

    public static Task BadRequest(this HttpContext context, string statusMessage) =>
        context.Response(HttpStatusCode.BadRequest, statusMessage);

    public static Task Response(this HttpContext context, HttpStatusCode statusCode, string statusMessage)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        return context.Response.WriteAsync(statusMessage, context.RequestAborted);
    }

    private static string GetUserClaimOrDefault(
        HttpContext context,
        string claim,
        string defaultValue = "")
    {
        return context.User
                   ?.FindFirst(claim)
                   ?.Value ??
               defaultValue;
    }
}