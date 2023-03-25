namespace Reliability.Client.CustomHttpClient;

public interface ICustomHttpClient
{
    Task<IEnumerable<WeatherForecast>> GetDataWithoutCache(CancellationToken ct);
}

public interface IEndpoint
{
    Uri Url { get; }
}