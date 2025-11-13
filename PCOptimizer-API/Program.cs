using PCOptimizer.Services;
using PCOptimizer_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5000",
            "http://localhost:5173",  // Vite dev server
            "http://127.0.0.1:3000",
            "http://127.0.0.1:5000",
            "http://127.0.0.1:5173"   // Vite dev server (loopback)
        )
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register backend services as singletons
builder.Services.AddSingleton<PerformanceMonitor>();
builder.Services.AddSingleton<OptimizerService>();
builder.Services.AddSingleton<AnomalyDetectionService>();
builder.Services.AddSingleton<ThemeManager>();
builder.Services.AddSingleton<ProfileService>();
builder.Services.AddSingleton<GameDetectionService>();
builder.Services.AddSingleton<PeripheralService>();
builder.Services.AddSingleton<BehaviorMonitor>();
builder.Services.AddSingleton<ConversationLogger>();

// Register monitoring background service - automatically starts data collection
builder.Services.AddHostedService<MonitoringBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");
app.MapControllers();

app.Run();
