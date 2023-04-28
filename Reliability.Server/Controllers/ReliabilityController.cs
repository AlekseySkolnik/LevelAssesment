using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Reliability.Controllers;

[ApiController]
[Route("api/[action]")]
public class ReliabilityController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private const string _cacheKey = "_cacheKey";
    internal const string BulkheadUrl = "/api/Bulkhead";

    private readonly ILogger<ReliabilityController> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;

    public ReliabilityController(ILogger<ReliabilityController> logger, IMemoryCache memoryCache,
        IDistributedCache distributedCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
    }

    [HttpGet]
    public ActionResult Timeout()
    {
        return DateTimeOffset.UtcNow.Second % 3 == 0
            ? StatusCode((int)HttpStatusCode.GatewayTimeout, new { reason = "GatewayTimeout" })
            : StatusCode((int)HttpStatusCode.RequestTimeout, new { reason = "RequestTimeout" });
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>?> GetDataSuccess()
    {
        return await GetData();
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>?> GetDataWithoutCache()
    {
        return await GetDataWithDelay();
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>?> InternalCache()
    {
        return
            await _memoryCache.GetOrCreateAsync(
                _cacheKey,
                async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                    return await GetDataWithDelay();
                });
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>?> DistributedCache()
    {
        IEnumerable<WeatherForecast>? list;
        string bytes;

        var data = await _distributedCache.GetAsync(_cacheKey);

        if (data != null)
        {
            bytes = Encoding.UTF8.GetString(data);
            list = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(bytes);
        }
        else
        {
            list = await GetDataWithDelay();
            bytes = JsonSerializer.Serialize(list);
            data = Encoding.UTF8.GetBytes(bytes);
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(DateTime.Now.AddHours(6));

            await _distributedCache.SetAsync(_cacheKey, data, options);
        }

        return list;
    }

    [HttpGet]
    public async Task<IActionResult> GetDataWithoutCacheWithRandomInternalServerError()
    {
        var data = await GetDataWithDelay();

        return DateTimeOffset.UtcNow.Second % 9 == 0
            ? StatusCode((int)HttpStatusCode.InternalServerError, new { reason = "InternalServerError" })
            : Ok(data);
    }

    [HttpGet]
    public async Task<IActionResult> CircuitBreaker()
    {
        return DateTimeOffset.UtcNow.Second switch
        {
            < 25 => StatusCode((int)HttpStatusCode.TooManyRequests, new { reason = "TooManyRequests" }),
            > 25 and > 45 => DateTimeOffset.UtcNow.Second % 3 == 0
                ? StatusCode((int)HttpStatusCode.TooManyRequests, new { reason = "TooManyRequests" })
                : Ok(await GetData()),
            _ => Ok(await GetData())
        };
    }


    [HttpGet]
    public async Task<IActionResult> Bulkhead()
    {
        var task = Task.Factory.StartNew(() =>
        {
            long res = 1;

            for (var i = 0; i < 100000000; i++)
            {
                res += 2 ^ i;
            }

            _logger.LogInformation($"res = {res}");
        });

        await task;

        var data = await GetDataWithDelay();

        return Ok(data);
    }


    private static async Task<IEnumerable<WeatherForecast>?> GetDataWithDelay()
    {
        var rand = new Random();
        var delay = rand.Next(100, 900);
        await Task.Delay(delay);

        return await GetData();
    }

    private static async Task<IEnumerable<WeatherForecast>?> GetData()
    {
        return
            Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
    }
}