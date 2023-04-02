using Reliability.Extensions;

namespace Reliability.Services;

public interface IRequestCountRepository
{
    Task AddRequestInfo(
        string method,
        string controller,
        string action,
        string clientId,
        DateTime dateTime,
        int limit,
        TimeSpan period,
        CancellationToken ct);

    Task<int> GetRequestsCount(
        string method,
        string controller,
        string action,
        string clientId,
        DateTime currentDateTime,
        TimeSpan requestsPeriod,
        CancellationToken ct);
    Task<RequestLimitSettings> GetRequestLimitSettings(CancellationToken ct);
}