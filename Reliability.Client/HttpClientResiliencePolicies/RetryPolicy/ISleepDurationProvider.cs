namespace Reliability.Client.HttpClientResiliencePolicies.RetryPolicy;

public interface ISleepDurationProvider
{
    int RetryCount { get; }
    IEnumerable<TimeSpan> Durations { get; }
}