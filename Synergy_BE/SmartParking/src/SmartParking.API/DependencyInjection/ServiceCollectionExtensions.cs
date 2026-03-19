using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartParking.API.Authorization.Policies;
using SmartParking.API.Filters;
using SmartParking.API.Middlewares;
using SmartParking.Application.DependencyInjection;
using SmartParking.Domain.Constants;
using SmartParking.Infrastructure.DependencyInjection;
using System.Text;

namespace SmartParking.API.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(options =>
            {
                // Add validation filter globally
                options.Filters.Add<ValidationFilter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // Suppress automatic 400 response to allow custom filter handling
                options.SuppressModelStateInvalidFilter = false;
                
                // Custom 400 response shape for validation errors
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Có lỗi xác thực dữ liệu.",
                        Instance = context.HttpContext.Request.Path,
                        Type = "https://httpstatuses.com/400"
                    };

                    problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    return new BadRequestObjectResult(problemDetails);
                };
            });

        services.AddEndpointsApiExplorer();
        
        // Swagger with JWT Bearer
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SmartParking API",
                Version = "v1",
                Description = "SmartParking System API with JWT Authentication"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        // JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
                ClockSkew = TimeSpan.Zero
            };
            // Read token from httpOnly cookie (Web) when Authorization header is empty (Mobile uses Bearer)
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    var token = ctx.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
                    if (string.IsNullOrEmpty(token) && ctx.Request.Cookies.TryGetValue(Helpers.AuthCookieHelper.AccessTokenCookieName, out var cookieToken))
                        ctx.Token = cookieToken;
                    else if (!string.IsNullOrEmpty(token))
                        ctx.Token = token;
                    return Task.CompletedTask;
                }
            };
        });

        // Policy-Based Authorization
        services.AddAuthorization(options =>
        {
            // User (Driver) only policy
            options.AddPolicy(AuthorizationPolicies.UserOnly, policy =>
                policy.RequireRole(AuthConstants.Roles.User));

            // Owner only policy
            options.AddPolicy(AuthorizationPolicies.OwnerOnly, policy =>
                policy.RequireRole(AuthConstants.Roles.Owner));

            // Admin only policy
            options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                policy.RequireRole(AuthConstants.Roles.Admin));

            // Owner OR Admin policy
            options.AddPolicy(AuthorizationPolicies.OwnerOrAdmin, policy =>
                policy.RequireRole(AuthConstants.Roles.Owner, AuthConstants.Roles.Admin));

            // User OR Admin policy
            options.AddPolicy(AuthorizationPolicies.UserOrAdmin, policy =>
                policy.RequireRole(AuthConstants.Roles.User, AuthConstants.Roles.Admin));

            // User OR Owner OR Admin policy
            options.AddPolicy(AuthorizationPolicies.UserOrOwnerOrAdmin, policy =>
                policy.RequireRole(AuthConstants.Roles.User, AuthConstants.Roles.Owner, AuthConstants.Roles.Admin));
        });

        // CORS - Production: dùng AllowedOrigins từ config. Development: AllowAll
        var allowedOrigins = configuration.GetSection("CORS")["AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                if (allowedOrigins is { Length: > 0 })
                    builder.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials().WithExposedHeaders("*");
                else
                    builder.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader().AllowCredentials().WithExposedHeaders("*");
            });
        });

        // Filters
        services.AddScoped<ValidationFilter>();

        // Middlewares
        services.AddScoped<ExceptionHandlingMiddleware>();

        // Cross-layer DI
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);

        return services;
    }
}

