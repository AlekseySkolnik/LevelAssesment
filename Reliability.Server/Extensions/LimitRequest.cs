namespace Reliability.Extensions;

public record LimitRequest(string requestPath, RoutingValues routingValues, List<ClientIdentifierInfo> clientIdentifiers, string deviceId);
public record RoutingValues(string method, string controller, string action);