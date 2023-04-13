namespace Reliability.Client;

public static class DependenciesConfig
{
    /// <summary>
    /// Configure static class ApplicationLogging
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureStaticApplicationLogging(this IServiceCollection services)
    {
        ApplicationLogging.Configure(services.BuildServiceProvider());
    }
}