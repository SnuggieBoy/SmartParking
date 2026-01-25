using SmartParking.API.DependencyInjection;
using SmartParking.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// CRITICAL: Validate secrets at startup (fail-fast if missing)
ValidateSecrets(builder.Configuration, builder.Environment);

builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartParking API V1");
    });
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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

    // Validate VNPay credentials (production only)
    if (environment.IsProduction())
    {
        var vnpayTmn = configuration["VnPay:TmnCode"];
        var vnpaySecret = configuration["VnPay:HashSecret"];

        if (string.IsNullOrWhiteSpace(vnpayTmn))
        {
            errors.Add("VnPay:TmnCode is not configured. Set environment variable VnPay__TmnCode");
        }
        else if (vnpayTmn.Contains("YOUR_"))
        {
            errors.Add("VnPay:TmnCode appears to be a placeholder.");
        }

        if (string.IsNullOrWhiteSpace(vnpaySecret))
        {
            errors.Add("VnPay:HashSecret is not configured. Set environment variable VnPay__HashSecret");
        }
        else if (vnpaySecret.Contains("YOUR_"))
        {
            errors.Add("VnPay:HashSecret appears to be a placeholder.");
        }
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
