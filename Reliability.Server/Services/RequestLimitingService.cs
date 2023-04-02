using System.Text.Json;
using Reliability.Extensions;

namespace Reliability.Services;

public class RequestLimitingService : IRequestLimitingService
{
    private readonly IRequestCountRepository _requestCountRepository;
    private readonly ILogger<RequestLimitingService> _logger;

    public RequestLimitingService(IRequestCountRepository requestCountRepository,
        ILogger<RequestLimitingService> logger)
    {
        _requestCountRepository = requestCountRepository;
        _logger = logger;
    }

    private static readonly string[] AllowedPathStrings = { "/health/ready", "/health/live", "/metrics", "/api/cache" };

    public async Task<bool> NeedBlockRequest(LimitRequest request, CancellationToken ct)
    {
        var limits = await _requestCountRepository.GetRequestLimitSettings(ct);
        var needCheckLimits = LimitsForRoutingShouldBeSetupCorrectly(limits, request, out var limitByEndpoints) &&
                              !IsPathInIgnoringLimitingList(request.requestPath) &&
                              RoutingForRequestShouldBeValid(request.routingValues);
        if (!needCheckLimits)
        {
            return false;
        }

        var clientIdentifiers = GetLimitingClientIdentifiers(request.clientIdentifiers, limitByEndpoints);
        foreach (var clientIdentifier in clientIdentifiers)
        {
            if (await NeedBlockRequest(request, clientIdentifier, limitByEndpoints, ct))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> NeedBlockRequest(
        LimitRequest request,
        ClientIdentifierInfo clientIdentifier,
        RequestLimitSettings.LimitSettings limitByEndpoints,
        CancellationToken ct)
    {
        var requestsPeriod = TimeSpan.FromSeconds(limitByEndpoints.ObservingIntervalInSeconds);
        var requestsLimit = limitByEndpoints.Count;
        var shouldBeBlocked = await ShouldBeBlocked(request, clientIdentifier, limitByEndpoints, ct);
   
        await _requestCountRepository.AddRequestInfo(
            request.routingValues.method,
            request.routingValues.controller,
            request.routingValues.action,
            clientIdentifier.Value,
            DateTime.UtcNow,
            requestsLimit,
            requestsPeriod,
            ct);
        
        return shouldBeBlocked;
    }

    private async Task<bool> ShouldBeBlocked(
        LimitRequest request,
        ClientIdentifierInfo clientIdentifier,
        RequestLimitSettings.LimitSettings limitByEndpoints,
        CancellationToken ct)
    {
        var requestsPeriod = TimeSpan.FromSeconds(limitByEndpoints.ObservingIntervalInSeconds);
        var requestsLimit = limitByEndpoints.Count;
        var requestsCount = await _requestCountRepository.GetRequestsCount(
            request.routingValues.method,
            request.routingValues.controller,
            request.routingValues.action,
            clientIdentifier.Value,
            DateTime.UtcNow,
            requestsPeriod,
            ct);

        var shouldBeBlocked = requestsCount >= requestsLimit;
        
        if (shouldBeBlocked)
        {
            LogRequestWasLimited(clientIdentifier, request, requestsCount, requestsLimit, requestsPeriod);
        }

        return shouldBeBlocked;
    }


    public void LogRequestWasLimited(
        ClientIdentifierInfo blockingIdentifier,
        LimitRequest request,
        int actualRequestsCount,
        int limitCountSetting,
        TimeSpan limitPeriodSetting)
    {
        var scope = new Dictionary<string, object>
        {
            ["details"] = JsonSerializer.Serialize(new
                { request, actualRequestsCount, limitCountSetting, limitPeriodSetting }),
        };
        using (_logger.BeginScope(scope))
        {
            _logger.LogInformation("Request blocked {blockingIdentifierType} {blockingIdentifier}",
                blockingIdentifier.Type.ToString(), blockingIdentifier.Value);
        }
    }


    private bool LimitsForRoutingShouldBeSetupCorrectly(RequestLimitSettings limits, LimitRequest request,
        out RequestLimitSettings.LimitSettings limitByEndpoints)
    {
        limitByEndpoints =
            limits.SettingsByEndpoints.GetValueOrDefault(BuildSettingsKey(request.routingValues.method,
                request.requestPath)) ??
            limits.DefaultSettings;

        if (limitByEndpoints.NoLimit)
        {
            return false;
        }

        if (!limitByEndpoints.IsApplicable())
        {
            _logger.LogWarning("Options UserRequestCounterOptions for RequestLimiter is not set or invalid");

            return false;
        }

        return true;
    }

    private static bool RoutingForRequestShouldBeValid(RoutingValues routingValues) =>
        routingValues.controller != null && routingValues.action != null;

    private bool IsPathInIgnoringLimitingList(string path)
    {
        return AllowedPathStrings.Contains(path) || path.StartsWith("/api/v1/admin");
    }

    private static ClientIdentifierInfo[] GetLimitingClientIdentifiers(
        List<ClientIdentifierInfo> clientIdentifierInfos,
        RequestLimitSettings.LimitSettings limitByEndpoints)
    {
        var types = new List<ClientIdentifierTypes>();
        if (limitByEndpoints.ByDeviceId) types.Add(ClientIdentifierTypes.DeviceId);
        if (limitByEndpoints.ByClientIP) types.Add(ClientIdentifierTypes.ClientIp);
        if (limitByEndpoints.ByClientUUId) types.Add(ClientIdentifierTypes.ClientUUId);
        if (limitByEndpoints.ByWorkflowId) types.Add(ClientIdentifierTypes.WorkflowId);
        return clientIdentifierInfos.Where(x => types.Contains(x.Type)).ToArray();
    }

    private string BuildSettingsKey(string method, string path) =>
        $"{method}_{path}".ToLower();
}