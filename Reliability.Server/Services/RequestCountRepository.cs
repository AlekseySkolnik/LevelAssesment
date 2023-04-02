using Microsoft.Extensions.Caching.Distributed;
using Reliability.Extensions;
using System.Text;
using System.Text.Json;

namespace Reliability.Services;

public class RequestCountRepository : IRequestCountRepository
{
    private readonly IDistributedCache _distributedCache;
    private const string RequestLimitSettingsShardKey = "RequestLimitSettings";

    public RequestCountRepository(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }


    public async Task AddRequestInfo(string method, string controller, string action, string clientId, DateTime dateTime,
        int limit, TimeSpan period, CancellationToken ct)
    {
        var _cacheKey = $"{method}_{controller}_{action}_{clientId}";
        // TODO сохранить информацию по клинту
        var info = new {};
        var bytes = JsonSerializer.Serialize(info);
        var data = Encoding.UTF8.GetBytes(bytes);
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(60))
            .SetAbsoluteExpiration(DateTime.Now.AddHours(6));
        
        await _distributedCache.SetAsync(_cacheKey, data, options);
    }

    public async Task<int> GetRequestsCount(string method, string controller, string action, string clientId,
        DateTime currentDateTime, TimeSpan requestsPeriod, CancellationToken ct)
    {
        var count = 0;
        var _cacheKey = $"{method}_{controller}_{action}_{clientId}";
        
        var data = await _distributedCache.GetAsync(_cacheKey);

        if (data != null)
        {
            var bytes = Encoding.UTF8.GetString(data);
            count = JsonSerializer.Deserialize<int>(bytes);
        }

        return count;
    }

    public async Task<RequestLimitSettings> GetRequestLimitSettings(CancellationToken ct) =>
        new RequestLimitSettings()
        {
           DefaultSettings = new RequestLimitSettings.LimitSettings()
           {
               NoLimit = false,
               ByClientIP = true,
               Count = 10,
               ObservingIntervalInSeconds = 10
           }
        };
}