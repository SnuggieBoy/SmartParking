namespace SmartParking.Application.DTOs.Vehicle;

public sealed record VehicleDto(
    Guid VehicleId,
    string LicensePlate,
    string VehicleType,
    string? Brand,
    string? Model,
    string? Color,
    bool IsActive,
    DateTime CreatedAt
);

public sealed record CreateVehicleDto(
    string LicensePlate,
    int VehicleType,
    string? Brand,
    string? Model,
    string? Color
);

public sealed record UpdateVehicleDto(
    string LicensePlate,
    int VehicleType,
    string? Brand,
    string? Model,
    string? Color,
    bool IsActive
);
