namespace Reliability.Extensions;

public class RequestLimitSettings
{
    /// <summary>
    /// if no SettingsByEndpoints then use Default
    /// </summary>
    public LimitSettings DefaultSettings { get; set; } = new()
    {
        NoLimit = true
    };

    /// <summary>
    /// Endpoint limit settings. Endpoints must be specified in lower case.
    /// </summary>
    public Dictionary<string, LimitSettings> SettingsByEndpoints { get; set; } = new();

    /// <summary>
    /// Requests count (per period) threshold that switches service to an in memory
    /// rate limiting mode. Requests are counted separately for each distinct app instance.
    /// </summary>
    public int? InMemoryKickoffCount { get; set; }

    public int InMemoryStorageCleanIntervalSeconds { get; set; } = 10;

    public class LimitSettings
    {
        public bool NoLimit { get; set; }
        public int Count { get; set; }
        public int ObservingIntervalInSeconds { get; set; }
        public bool ByClientUUId { get; set; }
        public bool ByDeviceId { get; set; }
        public bool ByWorkflowId { get; set; }
        public bool ByClientIP { get; set; }
			
        public bool IsApplicable()
        {
            return Count > 0 && ObservingIntervalInSeconds > 0;
        }
    }
}