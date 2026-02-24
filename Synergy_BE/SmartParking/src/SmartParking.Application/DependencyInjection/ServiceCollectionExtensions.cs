using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Application.Services;

namespace SmartParking.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<GoogleOAuthSettings>(configuration.GetSection("GoogleOAuth"));
        services.Configure<VnPaySettings>(configuration.GetSection("VnPay"));
        services.Configure<CommissionSettings>(configuration.GetSection("CommissionSettings"));
        services.Configure<OwnerSubscriptionSettings>(configuration.GetSection("OwnerSubscriptionSettings"));

        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IParkingLotService, ParkingLotService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IParkingLocationService, ParkingLocationService>();
        services.AddScoped<IOwnerUpgradeService, OwnerUpgradeService>();
        services.AddScoped<IOwnerBankAccountService, OwnerBankAccountService>();
        services.AddScoped<IWalletService, WalletService>();
        
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        return services;
    }
}

