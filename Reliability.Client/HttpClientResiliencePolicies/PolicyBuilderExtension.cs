using Polly;
using Reliability.Client.HttpClientResiliencePolicies.RetryPolicy;

namespace Reliability.Client.HttpClientResiliencePolicies;

public static class PolicyBuilderExtension
{
    public static IAsyncPolicy<HttpResponseMessage> WaitAndRetryAsync(
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
    //
    // public static IAsyncPolicy<HttpResponseMessage> AdvancedCircuitBreakerAsync(
    //     this PolicyBuilder<HttpResponseMessage> policyBuilder,
    //     CircuitBreakerPolicySettings settings)
    // {
    //     return policyBuilder
    //         .AdvancedCircuitBreakerAsync(
    //             settings.FailureThreshold,
    //             settings.SamplingDuration,
    //             settings.MinimumThroughput,
    //             settings.DurationOfBreak,
    //             settings.OnBreak,
    //             settings.OnReset,
    //             settings.OnHalfOpen);
    // }
}