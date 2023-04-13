using Reliability.Client;
using Reliability.Client.CustomHttpClient;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.ConfigureStaticApplicationLogging();
        services
            .AddScoped<CountryRouter>()
            .AddScoped<IEndpoint>(sp => sp.GetRequiredService<CountryRouter>());
        
        services.AddHostedService<Worker>();
        
        // services.AddCustomHttpClient_WithTimeout();
        // services.AddCustomHttpClient_WithRetry();
         services.AddCustomHttpClient_WithCircuitBreaker();
    })
    .Build();

host.Run();