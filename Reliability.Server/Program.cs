using Reliability.Extensions;
using Reliability.Middlewares;
using Reliability.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Custom services
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "127.0.0.1:6379";
});
builder.Services.AddScoped<IRequestCountRepository, RequestCountRepository>();
builder.Services.AddScoped<IRequestLimitingService, RequestLimitingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//app.UseMiddleware<BulkheadMiddleware>();

app.MapControllers();

app.Run();