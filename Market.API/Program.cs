using System.Reflection;
using Market.Infrastructure;
using Market.Application;
using Market.Migration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using DotNetEnv;
using Market.API.Filters;
using Market.API;
using Market.API.Middlewares;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddMigrationRunner(builder.Configuration["ConnectionStrings:DefaultConnection"]!);

builder.Services.AddControllers(options =>
{
    // Add controller logging filter to all controllers
    options.Filters.Add<ControllerLoggingFilter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("The application is running."))
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        healthQuery: "SELECT 1;", // Simple query to test connectivity
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        timeout: TimeSpan.FromSeconds(10))
    .AddCheck("memory", () =>
    {
        var allocated = GC.GetTotalMemory(false);
        var memoryMB = allocated / 1024 / 1024;
        return memoryMB < 512
            ? HealthCheckResult.Healthy($"Memory usage: {memoryMB}MB")
            : HealthCheckResult.Degraded($"High memory usage: {memoryMB}MB");
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<ControllerLoggingFilter>();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market API V1");
        // Enable the authorize button in Swagger UI
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description,
                duration = x.Value.Duration.TotalMilliseconds,
                data = x.Value.Data
            })
        };
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        await context.Response.WriteAsync(jsonResponse);
    }
});

// Liveness check to ensure the application is running
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Name == "self"
});

// Database connectivity check
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Name == "database" || check.Name == "self"
});

app.MapControllers();
app.Run();