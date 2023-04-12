namespace Reliability.Client.CustomHttpClient;

public interface ICustomHttpClient
{
    Task<IEnumerable<WeatherForecast>> GetDataFromServer(CancellationToken ct);
}

public interface IEndpoint
{
    Uri Url { get; }
}