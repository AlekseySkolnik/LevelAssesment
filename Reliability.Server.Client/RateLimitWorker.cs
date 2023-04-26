using System.Text.Json;
using Reliability;

namespace WorkerService1;

public class RateLimitWorker : BackgroundService
{
    private readonly ILogger<RateLimitWorker> _logger;
    private readonly HttpClient _httpClientFixed;
    private readonly HttpClient _httpClientConcurrency;

    private const int RateLimiterParallelization = 100; // 30 работает, 31 уже нет

    public RateLimitWorker(ILogger<RateLimitWorker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFixed = httpClientFactory.CreateClient("RateLimiter_fixed");
        _httpClientConcurrency = httpClientFactory.CreateClient("RateLimiter_concurrency");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        int counter = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5, stoppingToken);
            //  await RateLimiter_fixed(counter, stoppingToken);
            await RateLimiter_concurrency(stoppingToken);
            counter++;
        }
    }

    private async Task RateLimiter_fixed(int counter, CancellationToken ct)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();

        var response = await _httpClientFixed.GetAsync("RateLimiter/fixed", ct);
        response.EnsureSuccessStatusCode();
        var readAsStreamAsync = await response.Content.ReadAsStreamAsync(ct);

        timer.Stop();

        var result =
            await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>?>(readAsStreamAsync,
                new JsonSerializerOptions(), ct);

        if (timer.ElapsedMilliseconds > 1000)
        {
            _logger.LogError($"---- {counter} request : fixed!!! time = {timer.ElapsedMilliseconds} ms");
        }
        else
        {
            _logger.LogWarning($"---- {counter} request : success time = {timer.ElapsedMilliseconds} ms");
        }
    }

    private async Task RateLimiter_concurrency(CancellationToken ct)
    {
        var works = new List<Task>();
        
        works.AddRange(Enumerable.Range(0, RateLimiterParallelization / 2)
            .Select(x => TaskRateLimiter_concurrency(x, ct)));
        works.AddRange(Enumerable.Range(0, RateLimiterParallelization / 2)
            .Select(x => TaskRateLimiter_concurrency2(x, ct)));
        
        await Task.WhenAll(works);
    }

    private async Task TaskRateLimiter_concurrency(int counter, CancellationToken ct)
    {
        try
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClientConcurrency.GetAsync("RateLimiter/concurrency", ct);
            response.EnsureSuccessStatusCode();
            var readAsStreamAsync = await response.Content.ReadAsStreamAsync(ct);

            timer.Stop();

            _logger.LogWarning(
                $"{counter} request_1 : status = {response.StatusCode.ToString()} time = {timer.ElapsedMilliseconds} ms");
        }
        catch (Exception e)
        {
            _logger.LogError($"First failed: Message = {e.Message}");
        }
    }

    private async Task TaskRateLimiter_concurrency2(int counter, CancellationToken ct)
    {
        try
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var response = await _httpClientFixed.GetAsync("RateLimiter/concurrency", ct);
            response.EnsureSuccessStatusCode();
            var readAsStreamAsync = await response.Content.ReadAsStreamAsync(ct);

            timer.Stop();

            _logger.LogWarning(
                $"{counter} request_2 : status = {response.StatusCode.ToString()} time = {timer.ElapsedMilliseconds} ms");
        }
        catch (Exception e)
        {
            _logger.LogError($"Second failed: Message = {e.Message}");
        }
      
    }
}