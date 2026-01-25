using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.DTOs.Vehicle;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;
using SmartParking.Domain.Enums;

namespace SmartParking.Application.Services;

public sealed class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleService(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<VehicleDto> GetByIdAsync(Guid vehicleId, Guid userId, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, ct);
        if (vehicle == null)
        {
            throw new NotFoundException(Messages.Vehicle.NotFound);
        }

        if (vehicle.UserId != userId)
        {
            throw new UnauthorizedException(Messages.Common.Forbidden);
        }

        return MapToDto(vehicle);
    }

    public async Task<IEnumerable<VehicleDto>> GetMyVehiclesAsync(Guid userId, bool activeOnly = true, CancellationToken ct = default)
    {
        var vehicles = await _vehicleRepository.GetByUserIdAsync(userId, activeOnly, ct);
        return vehicles.Select(MapToDto);
    }

    public async Task<VehicleDto> CreateAsync(CreateVehicleDto request, Guid userId, CancellationToken ct = default)
    {
        var existing = await _vehicleRepository.GetByLicensePlateAsync(request.LicensePlate, ct);
        if (existing != null)
        {
            throw new BadRequestException(Messages.Vehicle.PlateAlreadyExists);
        }

        var vehicle = new Vehicle
        {
            VehicleId = Guid.NewGuid(),
            UserId = userId,
            LicensePlate = request.LicensePlate,
            VehicleType = request.VehicleType,
            Brand = request.Brand,
            Model = request.Model,
            Color = request.Color,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _vehicleRepository.CreateAsync(vehicle, ct);
        return MapToDto(created);
    }

    public async Task<VehicleDto> UpdateAsync(Guid vehicleId, UpdateVehicleDto request, Guid userId, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, ct);
        if (vehicle == null)
        {
            throw new NotFoundException(Messages.Vehicle.NotFound);
        }

        if (vehicle.UserId != userId)
        {
            throw new UnauthorizedException(Messages.Common.Forbidden);
        }

        if (vehicle.LicensePlate != request.LicensePlate)
        {
            var existing = await _vehicleRepository.GetByLicensePlateAsync(request.LicensePlate, ct);
            if (existing != null && existing.VehicleId != vehicleId)
            {
                throw new BadRequestException(Messages.Vehicle.PlateAlreadyExists);
            }
        }

        vehicle.LicensePlate = request.LicensePlate;
        vehicle.VehicleType = request.VehicleType;
        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.Color = request.Color;
        vehicle.IsActive = request.IsActive;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _vehicleRepository.UpdateAsync(vehicle, ct);
        return MapToDto(vehicle);
    }

    public async Task DeleteAsync(Guid vehicleId, Guid userId, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, ct);
        if (vehicle == null)
        {
            throw new NotFoundException(Messages.Vehicle.NotFound);
        }

        if (vehicle.UserId != userId)
        {
            throw new UnauthorizedException(Messages.Common.Forbidden);
        }

        await _vehicleRepository.DeleteAsync(vehicleId, ct);
    }

    private static VehicleDto MapToDto(Vehicle vehicle)
    {
        return new VehicleDto(
            vehicle.VehicleId,
            vehicle.LicensePlate,
            GetVehicleTypeName(vehicle.VehicleType),
            vehicle.Brand,
            vehicle.Model,
            vehicle.Color,
            vehicle.IsActive ?? true,
            vehicle.CreatedAt
        );
    }

    private static string GetVehicleTypeName(int vehicleType)
    {
        return vehicleType switch
        {
            0 => nameof(VehicleType.Motorcycle),
            1 => nameof(VehicleType.Car),
            2 => nameof(VehicleType.Truck),
            3 => nameof(VehicleType.Van),
            4 => nameof(VehicleType.Bus),
            _ => "Unknown"
        };
    }
}
