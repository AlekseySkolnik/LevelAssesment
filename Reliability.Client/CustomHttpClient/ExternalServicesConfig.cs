using Microsoft.Extensions.DependencyInjection.Extensions;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;
using Polly.Extensions.Http;
using Polly.Timeout;
using Reliability.Client.HttpClientResiliencePolicies.RetryPolicy;
using Reliability.Client.HttpClientResiliencePolicies;

namespace Reliability.Client.CustomHttpClient;

public static class ExternalServicesConfig
{
    private static readonly Uri _baseAddress = new Uri("http://localhost:5089");

    public static IServiceCollection AddCustomHttpClient_WithTimeout(this IServiceCollection services)
    {
        services
            .AddHttpClient(
                "reliability",
                client =>
                {
                    client.BaseAddress = _baseAddress;
                    client.Timeout = TimeSpan.FromMilliseconds(500);
                })
            .TryAddTypedClient<ICustomHttpClient>((_, client) => new CustomHttpClient(client));

        return services;
    }

    public static IServiceCollection AddCustomHttpClient_WithRetry(this IServiceCollection services)
    {
        services
            .AddHttpClient(
                "reliability",
                client =>
                {
                    client.BaseAddress = _baseAddress;
                    client.Timeout = TimeSpan.FromMilliseconds(900);
                })
            // Запрос с 2мя ретраями
            .AddRetryPolicy(RetryPolicySettings.Jitter(2, TimeSpan.FromMilliseconds(100)))
            .TryAddTypedClient<ICustomHttpClient>((_, client) => new CustomHttpClient(client));

        return services;
    }

    public static void TryAddTypedClient<TClient>(this IHttpClientBuilder builder,
        Func<IEndpoint, HttpClient, TClient> factory) where TClient : class
    {
        builder.Services.TryAddTransient(
            provider =>
            {
                var scope = provider.CreateScope();
                var endpoint = scope.ServiceProvider.GetService<IEndpoint>();
                var client = provider.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(builder.Name);

                return factory(endpoint, client);
            }
        );
    }

    public static IHttpClientBuilder AddRetryPolicy(
        this IHttpClientBuilder clientBuilder,
        RetryPolicySettings settings)
    {
        return clientBuilder.AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
            .Or<TimeoutRejectedException>().WaitAndRetryAsync(settings));
    }
}