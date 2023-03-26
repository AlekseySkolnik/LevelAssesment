using System.Text.Json;

namespace Reliability.Client.CustomHttpClient;

public class CustomHttpClient : ICustomHttpClient
{
    private readonly HttpClient _httpClient;

    public CustomHttpClient(HttpClient client)
    {
        _httpClient = client;
    }
    
    public async Task<IEnumerable<WeatherForecast>> GetDataWithoutCache(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync($"api/GetDataWithoutCacheWithRandomInternalServerError", ct);

        response.EnsureSuccessStatusCode();
        var readAsStreamAsync = await response.Content.ReadAsStreamAsync(ct);

        return await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>?>(readAsStreamAsync, new JsonSerializerOptions(), ct) ??
               throw new InvalidCastException("BalancesResponse is null");
    }
}