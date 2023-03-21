using System.Net;
using System.Runtime.CompilerServices;
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
        return StatusCode((int)HttpStatusCode.GatewayTimeout, new { reason = "GatewayTimeout" });
    }

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> GetDataWithoutCache() =>
        await GetDataWithDelay();

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> InternalCache() =>
        await _memoryCache.GetOrCreateAsync(
            _cacheKey,
            async cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);
                return await GetDataWithDelay();
            });

    [HttpGet]
    public async Task<IEnumerable<WeatherForecast>> DistributedCache()
    {
        IEnumerable<WeatherForecast> list;
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

    private static async Task<IEnumerable<WeatherForecast>> GetDataWithDelay()
    {
        var rand = new Random();
        var delay = rand.Next(10, 1500);

        await Task.Delay(delay);

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