using System.Collections;
using System.Collections.Immutable;
using System.Text.Json;
using Reliability;

namespace WorkerService1;

public class BulkheadWorker : BackgroundService
{
    private readonly ILogger<BulkheadWorker> _logger;
    private readonly HttpClient _httpFailedClient;
    private readonly HttpClient _httpSuccessClient;

    public BulkheadWorker(ILogger<BulkheadWorker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpFailedClient = httpClientFactory.CreateClient("Bulkhead_success");
        _httpSuccessClient = httpClientFactory.CreateClient("Bulkhead_failed");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
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
            _logger.LogInformation($"GetDataBulkhead_Success result = {JsonSerializer.Serialize(result)}");
        }
        catch (Exception e)
        {
            _logger.LogError($"GetDataBulkhead_Success failed: Message = {e.Message}");
        }
    }

    private async Task GetDataBulkhead_Failed(CancellationToken ct)
    {
        var taskList = Enumerable.Range(0, 100).Select(x => TaskBulkhead_Failed(x, ct));
        await Task.WhenAll(taskList);
    }

    private async Task TaskBulkhead_Failed(int x, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation($"TaskBulkhead_Failed_{x}");
            var response = await _httpFailedClient.GetAsync("api/Bulkhead", ct);
            var readAsStreamAsync = await response.Content.ReadAsStreamAsync(ct);
            var result =
                await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>?>(readAsStreamAsync,
                    new JsonSerializerOptions(), ct);
            _logger.LogInformation($"TaskBulkhead_Failed_{x} result = {JsonSerializer.Serialize(result)}");
        }
        catch (Exception e)
        {
            _logger.LogError($"TaskBulkhead_Failed_{x} failed: Message = {e.Message}");
        }
    }
}