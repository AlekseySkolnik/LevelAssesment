using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Reliability;
using Reliability.Middlewares;

static IEnumerable<WeatherForecast> GetData()
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    return
        Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
            .ToArray();
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Custom services
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = "127.0.0.1:6379"; });

// Fixed window limiter
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        // Допускается не более 4 запросов в каждом 12-секундном окне.
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // FIFO/LIFO
        options.QueueLimit = 2; // Максимальное совокупное количество разрешений для запросов на получение в очереди.
    }));
// Concurrency limiter
builder.Services.AddRateLimiter(limiterOptions =>
{
    limiterOptions.AddConcurrencyLimiter(policyName: "concurrency", options =>
    {
        options.PermitLimit = 10; //Максимальное количество разрешений, которые могут быть предоставлены одновременно.
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 1; // Максимальное совокупное количество разрешений для запросов на получение в очереди.
    });
    limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseRateLimiter();
app.MapGet("RateLimiter/fixed", () => Results.Ok(GetData()))
    .RequireRateLimiting("fixed");

app.MapGet("RateLimiter/concurrency", async () =>
{
    await Task.Delay(50);
    return Results.Ok(GetData());
}).RequireRateLimiting("concurrency");

app.UseMiddleware<BulkheadMiddleware>();

app.MapControllers();

app.Run();