using CreamyFusion.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// get connection string from appsetting.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register AppDbContext with dependency injection (DI)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure JSON serialization to handle reference loops
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Add services to the container.
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:3000",
                "https://kind-rock-0c0897d00.6.azurestaticapps.net") // React app URL
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();  // Force HTTP requests to redirect to HTTPS
    app.UseHsts();  // HTTP Strict Transport Security (HSTS)
}

app.UseCors("AllowReactApp"); // Enable CORS here

app.UseAuthorization();

app.MapControllers();

app.Run();
