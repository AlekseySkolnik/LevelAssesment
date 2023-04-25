using System.Text.Json;

namespace Reliability.Client.CustomHttpClient;

public class CustomHttpClient : ICustomHttpClient
{
    private readonly HttpClient _httpClient;

    public CustomHttpClient(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task<IEnumerable<WeatherForecast>> GetDataFromServer(CancellationToken ct)
    {
        // var response = await _httpClient.GetAsync($"api/Timeout", ct); // для таймаута
        // var response = await _httpClient.GetAsync($"api/GetDataWithoutCacheWithRandomInternalServerError", ct); // для ретраев
        // var response = await _httpClient.GetAsync($"api/CircuitBreaker", ct); // для СВ
        var response = await _httpClient.GetAsync($"api/Bulkhead", ct); // для Bulkhead

        response.EnsureSuccessStatusCode();
        var readAsStreamAsync = await response.Content.ReadAsStreamAsync(ct);

        return await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>?>(readAsStreamAsync,
                   new JsonSerializerOptions(), ct) ??
               throw new InvalidCastException("BalancesResponse is null");
    }
}