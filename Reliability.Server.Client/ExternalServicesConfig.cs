using Dodo.HttpClientResiliencePolicies;
using Dodo.HttpClientResiliencePolicies.CircuitBreakerPolicy;
using Dodo.HttpClientResiliencePolicies.RetryPolicy;

namespace WorkerService1;

public static class ExternalServicesConfig
{
    private static readonly Uri BaseAddress = new Uri("http://localhost:5089");

    public static IServiceCollection AddCustomHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient("Bulkhead_success", client => { client.BaseAddress = BaseAddress; });

        services
            .AddHttpClient(
                "Bulkhead_failed",
                client =>
                {
                    client.BaseAddress = BaseAddress;
                })
            .AddResiliencePolicies(
                new ResiliencePoliciesSettings
                {
                    OverallTimeout = TimeSpan.FromMilliseconds(10000),
                    TimeoutPerTry = TimeSpan.FromMilliseconds(1000),
                    RetryPolicySettings = RetryPolicySettings.Jitter(10, TimeSpan.FromMilliseconds(50)),
                    CircuitBreakerPolicySettings = new CircuitBreakerPolicySettings()
                });
        
        services.AddHttpClient("RateLimiter_fixed", client => { client.BaseAddress = BaseAddress; });
        services.AddHttpClient("RateLimiter_concurrency", client => { client.BaseAddress = BaseAddress; });

        return services;
    }
}