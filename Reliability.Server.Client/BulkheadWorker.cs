using System.Text.Json;
using Reliability;

namespace WorkerService1;

public class BulkheadWorker : BackgroundService
{
    private readonly ILogger<BulkheadWorker> _logger;
    private readonly HttpClient _httpFailedClient;
    private readonly HttpClient _httpSuccessClient;

    private const int BulkheadMaxParallelization = 20; // 29 работает, 31 уже нет

    public BulkheadWorker(ILogger<BulkheadWorker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpFailedClient = httpClientFactory.CreateClient("Bulkhead_success");
        _httpSuccessClient = httpClientFactory.CreateClient("Bulkhead_failed");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogWarning("Worker running at: {time}", DateTimeOffset.Now);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5, stoppingToken);

            var sucTask = GetDataBulkhead_Success(stoppingToken);
            var failTask = GetDataBulkhead_Failed(stoppingToken);
            await Task.WhenAll(sucTask, failTask);
        }
    }

    private async Task GetDataBulkhead_Success(CancellationToken ct)
    {
        try
        {
            await Task.Delay(10, ct);
            var response = await _httpSuccessClient.GetAsync("api/GetDataSuccess", ct);

            response.EnsureSuccessStatusCode();
            var readAsStreamAsync = await response.Content.ReadAsStreamAsync(ct);
            var result =
                await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>?>(readAsStreamAsync,
                    new JsonSerializerOptions(), ct);
            _logger.LogWarning($"GetDataBulkhead_Success result = {JsonSerializer.Serialize(result)}");
        }
        catch (Exception e)
        {
            _logger.LogError($"GetDataBulkhead_Success failed: Message = {e.Message}");
        }
    }

    private async Task GetDataBulkhead_Failed(CancellationToken ct)
    {
        var taskList = Enumerable.Range(0, BulkheadMaxParallelization).Select(x => TaskBulkhead_Failed(x, ct));
        await Task.WhenAll(taskList);
    }

    private async Task TaskBulkhead_Failed(int x, CancellationToken ct)
    {
        try
        {
            var response = await _httpFailedClient.GetAsync("api/Bulkhead", ct);
            response.EnsureSuccessStatusCode();
            var readAsStreamAsync = await response.Content.ReadAsStreamAsync(ct);
            var result =
                await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>?>(readAsStreamAsync,
                    new JsonSerializerOptions(), ct);
            _logger.LogWarning($"TaskBulkhead_Failed_{x} result = {JsonSerializer.Serialize(result)}");
        }
        catch (Exception e)
        {
            _logger.LogError($"TaskBulkhead_Failed_{x} failed: Message = {e.Message}");
        }
    }
}