using System.Threading.RateLimiting;
using SmartParking.API.DependencyInjection;
using SmartParking.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// CRITICAL: Validate secrets at startup (fail-fast if missing)
ValidateSecrets(builder.Configuration, builder.Environment);

builder.Services.AddApi(builder.Configuration);

// Health checks for load balancer / monitoring
builder.Services.AddHealthChecks();

// Rate limiting: auth endpoints stricter, general API moderate
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<Microsoft.AspNetCore.Http.HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.StatusCode = 429;
        await ctx.HttpContext.Response.WriteAsJsonAsync(new { message = "Quá nhiều yêu cầu. Vui lòng thử lại sau." }, ct);
    };
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();

// Swagger: Bật cả Development và Production (giống TechStore - dễ test trên Azure)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartParking API V1");
    c.RoutePrefix = string.Empty; // Swagger làm trang chủ: https://yourapp.azurewebsites.net/
});
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseCors();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health endpoints: /health (liveness), /health/ready (readiness - same for now)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();

/// <summary>
/// Validates that all required secrets are configured via environment variables.
/// SECURITY: Fail-fast on startup if secrets are missing or invalid.
/// </summary>
static void ValidateSecrets(IConfiguration configuration, IWebHostEnvironment environment)
{
    var errors = new List<string>();

    // Validate JWT Secret
    var jwtSecret = configuration["JwtSettings:SecretKey"];
    if (string.IsNullOrWhiteSpace(jwtSecret))
    {
        errors.Add("JwtSettings:SecretKey is not configured. Set environment variable JwtSettings__SecretKey");
    }
    else if (jwtSecret.Length < 32)
    {
        errors.Add($"JwtSettings:SecretKey must be at least 32 characters. Current length: {jwtSecret.Length}");
    }
    else if (jwtSecret.Contains("your-super-secret") || jwtSecret.Contains("placeholder"))
    {
        errors.Add("JwtSettings:SecretKey appears to be a placeholder. Generate a secure random secret.");
    }

    if (errors.Any())
    {
        var errorMessage = string.Join(Environment.NewLine, errors);
        throw new InvalidOperationException(
            $"CRITICAL CONFIGURATION ERROR:{Environment.NewLine}{errorMessage}{Environment.NewLine}{Environment.NewLine}" +
            $"Set secrets via environment variables:{Environment.NewLine}" +
            $"  Windows: $env:JwtSettings__SecretKey = \"your-secret\"{Environment.NewLine}" +
            $"  Linux/Mac: export JwtSettings__SecretKey=\"your-secret\"{Environment.NewLine}" +
            $"  Docker: -e JwtSettings__SecretKey=\"your-secret\"{Environment.NewLine}" +
            $"  Azure: Set in Application Settings");
    }
}
