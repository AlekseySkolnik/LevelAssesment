using Microsoft.Extensions.Logging.Abstractions;

namespace Reliability.Client;

public static class ApplicationLogging
{
    private static ILoggerFactory _factory;

    public static ILoggerFactory LoggerFactory =>
        _factory ??= new NullLoggerFactory();

    public static void Configure(IServiceProvider factory)
    {
        _factory = CreateLoggerFactory(factory);
    }

    private static ILoggerFactory CreateLoggerFactory(IServiceProvider factory)
    {
        var instance = (ILoggerFactory) factory.GetService(typeof(ILoggerFactory));
        if (instance == null)
        {
            throw new InvalidOperationException("Unable to found ILoggerFactory implementation");
        }
        return instance;
    }

    public static ILogger For(Type category)
    {
        return LoggerFactory.CreateLogger(category);
    }

    public static ILogger<TCategory> For<TCategory>()
    {
        return LoggerFactory.CreateLogger<TCategory>();
    }
}