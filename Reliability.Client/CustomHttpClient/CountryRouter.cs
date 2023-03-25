namespace Reliability.Client.CustomHttpClient;

public class CountryRouter : IEndpoint
{
    Uri IEndpoint.Url =>
        new Uri("http://localhost:5089");

    public CountryRouter()
    {
    }
}