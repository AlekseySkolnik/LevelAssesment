using System.Net;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.CircuitBreaker;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Reliability.Client.HttpClientResiliencePolicies;
using Reliability.Client.HttpClientResiliencePolicies.CircuitBreakerPolicy;
using Reliability.Client.HttpClientResiliencePolicies.RetryPolicy;

namespace Reliability.Client.CustomHttpClient;

public static class ExternalServicesConfig
{
    private static readonly Uri _baseAddress = new Uri("http://localhost:5089");

    public static IServiceCollection AddCustomHttpClient_WithTimeout(this IServiceCollection services)
    {
        // Решение без Polly
        // services
        //     .AddHttpClient(
        //         "reliability",
        //         client =>
        //         {
        //             client.BaseAddress = _baseAddress;
        //             client.Timeout = TimeSpan.FromMilliseconds(500);
        //         })
        //     .TryAddTypedClient<ICustomHttpClient>((_, client) => new CustomHttpClient(client));

        // Решение с Polly 
        services
            .AddHttpClient(
                "reliability",
                client => { client.BaseAddress = _baseAddress; })
            .AddPolicyHandler((services, request) =>
            {
                return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(500),
                    onTimeoutAsync: async (context, span, arg3, arg4) =>
                    {
                        services.GetService<ILogger<CustomHttpClient>>()?
                            .LogWarning("Timeout {span}ms",
                                span.TotalMilliseconds);
                    });
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
                    client.Timeout = TimeSpan.FromMilliseconds(1200);
                })
            .AddPolicyHandler((services, request) =>
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .Or<TimeoutRejectedException>()
                    .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromMilliseconds(50),
                            TimeSpan.FromMilliseconds(150),
                            TimeSpan.FromMilliseconds(500)
                        },
                        onRetry: (outcome, timespan, retryAttempt, context) =>
                        {
                            services.GetService<ILogger<CustomHttpClient>>()?
                                .LogWarning("Delaying for {delay}ms, then making retry {retry}.",
                                    timespan.TotalMilliseconds, retryAttempt);
                        }))
            .TryAddTypedClient<ICustomHttpClient>((_, client) => new CustomHttpClient(client));

        // services
        //     .AddHttpClient(
        //         "reliability",
        //         client =>
        //         {
        //             client.BaseAddress = _baseAddress;
        //             client.Timeout = TimeSpan.FromMilliseconds(900);
        //         })
        //     .CustomAddRetryPolicy(RetryPolicySettings.Jitter(2, TimeSpan.FromMilliseconds(100)))
        //     .TryAddTypedClient<ICustomHttpClient>((_, client) => new CustomHttpClient(client));

        return services;
    }

    public static IServiceCollection AddCustomHttpClient_WithCircuitBreaker(this IServiceCollection services)
    {
        services
            .AddHttpClient(
                "reliability",
                client =>
                {
                    client.BaseAddress = _baseAddress;
                })
            .AddPolicyHandler(
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .Or<TimeoutRejectedException>()
                    .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
                    .AdvancedCircuitBreakerAsync(
                        failureThreshold: 0.5, // Break on >=50% actions result in handled exceptions...
                        samplingDuration: TimeSpan.FromSeconds(5), // ... over any 10 second period
                        minimumThroughput: 8, // ... provided at least 8 actions in the 10 second period.
                        durationOfBreak: TimeSpan.FromSeconds(5), // Break for 30 seconds.
                        onBreak: (result, state, arg3, arg4) =>
                        {
                            ApplicationLogging
                                .For<CircuitBreakerPolicy>()
                                .LogWarning($"---onBreak--- Circuit breaker is open. Prev state = {state}");
                        },
                        onReset: context =>
                        {
                            ApplicationLogging
                                .For<CircuitBreakerPolicy>()
                                .LogWarning("---onReset--- Circuit breaker is closed");
                        },
                        onHalfOpen: () =>
                        {
                            ApplicationLogging
                                .For<CircuitBreakerPolicy>()
                                .LogWarning("---onHalfOpen--- Circuit breaker is half open");
                        }
                    ))
            .TryAddTypedClient<ICustomHttpClient>((_, client) => new CustomHttpClient(client));

        // services
        //     .AddHttpClient(
        //         "reliability",
        //         client =>
        //         {
        //             client.BaseAddress = _baseAddress;
        //             client.Timeout = TimeSpan.FromMilliseconds(1200);
        //         })
        //     .AddPolicyHandler((services, request) =>
        //         HttpPolicyExtensions
        //             .HandleTransientHttpError()
        //             .Or<TimeoutRejectedException>()
        //             .OrResult(r => r.StatusCode == (HttpStatusCode) 429) // Too Many Requests
        //             .CustomCircuitBreakerAsync(new CircuitBreakerPolicySettings()))
        //     .TryAddTypedClient<ICustomHttpClient>((_, client) => new CustomHttpClient(client));

        return services;
    }
    
    public static IServiceCollection AddCustomHttpClient_ForBulkhead(this IServiceCollection services)
    {
        services.AddHttpClient("Bulkhead_success", client =>
        {
            client.BaseAddress = _baseAddress;
        });

        services
            .AddHttpClient(
                "Bulkhead_failed",
                client =>
                {
                    client.BaseAddress = _baseAddress;
                    client.Timeout = TimeSpan.FromMilliseconds(5000);
                })
            .CustomAddRetryPolicy(RetryPolicySettings.Jitter(10, TimeSpan.FromMilliseconds(10)));

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
                var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(builder.Name);
                return factory(endpoint, client);
            }
        );
    }
}