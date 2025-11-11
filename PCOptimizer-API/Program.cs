using PCOptimizer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5000", "http://127.0.0.1:3000", "http://127.0.0.1:5000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register backend services as singletons
builder.Services.AddSingleton<PerformanceMonitor>();
builder.Services.AddSingleton<OptimizerService>();
builder.Services.AddSingleton<AnomalyDetectionService>();
builder.Services.AddSingleton<ThemeManager>();

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
