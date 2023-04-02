using Reliability.Extensions;

namespace Reliability.Services;

public interface IRequestLimitingService
{
    Task<bool> NeedBlockRequest(LimitRequest request, CancellationToken ct);
}