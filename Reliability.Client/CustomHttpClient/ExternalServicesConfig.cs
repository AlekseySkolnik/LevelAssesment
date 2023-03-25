using Microsoft.Extensions.DependencyInjection.Extensions;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;
using Polly.Extensions.Http;
using Polly.Timeout;
using Reliability.Client.HttpClientResiliencePolicies.RetryPolicy;
using Reliability.Client.HttpClientResiliencePolicies;

namespace Reliability.Client.CustomHttpClient;

public static class ExternalServicesConfig
{
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