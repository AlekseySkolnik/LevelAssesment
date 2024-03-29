using System.Text.Json;
using Reliability.Client.CustomHttpClient;

namespace Reliability.Client;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ICustomHttpClient _customHttpClient;

    public Worker(ILogger<Worker> logger, ICustomHttpClient customHttpClient)
    {
        _logger = logger;
        _customHttpClient = customHttpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(5, stoppingToken);

            try
            {
                var dataFromServer = await _customHttpClient.GetDataFromServer(stoppingToken);
                _logger.LogInformation($"Response is success: {JsonSerializer.Serialize(dataFromServer)}");
            }
            catch (TaskCanceledException tce)
            {
                _logger.LogError($"Response failed: Message = {tce.Message}");
            }
            catch (HttpRequestException re)
            {
                _logger.LogError(re, $"Response failed: StatusCode = {re.StatusCode}, Message = {re.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{e.Message}");
            }
        }
    }
}