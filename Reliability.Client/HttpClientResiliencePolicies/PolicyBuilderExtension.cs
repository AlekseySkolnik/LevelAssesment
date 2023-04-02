using Polly;
using Reliability.Client.HttpClientResiliencePolicies.RetryPolicy;
using Polly.Extensions.Http;
using Polly.Timeout;
using Reliability.Client.HttpClientResiliencePolicies.CircuitBreakerPolicy;

namespace Reliability.Client.HttpClientResiliencePolicies;

public static class PolicyBuilderExtension
{
    public static IHttpClientBuilder CustomAddRetryPolicy(
        this IHttpClientBuilder clientBuilder,
        RetryPolicySettings settings)
    {
        return clientBuilder.AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError()
            .Or<TimeoutRejectedException>().CustomWaitAndRetryAsync(settings));
    }
    
    public static IAsyncPolicy<HttpResponseMessage> CustomWaitAndRetryAsync(
        this PolicyBuilder<HttpResponseMessage> policyBuilder,
        RetryPolicySettings settings)
    {
        var handler = new RetryPolicyHandler(settings);
        return policyBuilder
            .WaitAndRetryAsync(
                handler.RetryCount,
                handler.SleepDurationProvider,
                handler.OnRetry);
    }
    
    public static IAsyncPolicy<HttpResponseMessage> CustomCircuitBreakerAsync(
        this PolicyBuilder<HttpResponseMessage> policyBuilder,
        CircuitBreakerPolicySettings settings)
    {
        return policyBuilder
            .AdvancedCircuitBreakerAsync(
                settings.FailureThreshold,
                settings.SamplingDuration,
                settings.MinimumThroughput,
                settings.DurationOfBreak,
                settings.OnBreak,
                settings.OnReset,
                settings.OnHalfOpen);
    }
}