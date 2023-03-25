using Reliability.Client;
using Reliability.Client.CustomHttpClient;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
            .AddScoped<CountryRouter>()
            .AddScoped<IEndpoint>(sp => sp.GetRequiredService<CountryRouter>());
        services.AddHostedService<Worker>();

        services
            .AddHttpClient(
                "loyalty",
                client =>
                {
                    client.BaseAddress = new Uri("http://localhost:5089");
                    client.Timeout = TimeSpan.FromMilliseconds(500);
                })
            .TryAddTypedClient<ICustomHttpClient>((_, client) => new CustomHttpClient(client));
    })
    .Build();

host.Run();