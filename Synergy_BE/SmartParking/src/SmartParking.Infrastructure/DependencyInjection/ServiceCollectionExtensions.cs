using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Infrastructure.Data;
using SmartParking.Infrastructure.Repositories;
using SmartParking.Infrastructure.Services;

namespace SmartParking.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register HttpContextAccessor for audit fields
        services.AddHttpContextAccessor();

        services.AddHttpClient();

        // Cloudinary: bind settings from appsettings, CloudinaryService creates client from config
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        // DbContext
        services.AddDbContext<SmartParkingDBContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAuthRepository, UserAuthRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IEmailOtpRepository, EmailOtpRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IExtensionRequestRepository, ExtensionRequestRepository>();
        services.AddScoped<IParkingLotRepository, ParkingLotRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IParkingLocationRepository, ParkingLocationRepository>();
        services.AddScoped<IOwnerUpgradeRequestRepository, OwnerUpgradeRequestRepository>();
        services.AddScoped<IOwnerBankAccountRepository, OwnerBankAccountRepository>();
        services.AddScoped<IUserWalletRepository, UserWalletRepository>();

        // Services
        services.AddScoped<ISePayService, SePayService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDevicePushTokenService, DevicePushTokenService>();
        services.AddScoped<IExpoPushService, ExpoPushService>();

        return services;
    }
}

