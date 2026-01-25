using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Helpers;
using SmartParking.Application.DTOs.ParkingLocation;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Services;

/// <summary>
/// Service implementation for parking location management.
/// Handles geospatial queries using Haversine formula for distance calculation.
/// </summary>
public sealed class ParkingLocationService : IParkingLocationService
{
    private readonly IParkingLocationRepository _repository;

    public ParkingLocationService(IParkingLocationRepository repository)
    {
        _repository = repository;
    }

    public async Task CreateAsync(ParkingLocationCreateDto dto, CancellationToken ct = default)
    {
        // Validate coordinates
        if (!GeoDistanceHelper.IsValidLatitude(dto.Latitude))
        {
            throw new BadRequestException("Latitude must be between -90 and 90");
        }

        if (!GeoDistanceHelper.IsValidLongitude(dto.Longitude))
        {
            throw new BadRequestException("Longitude must be between -180 and 180");
        }

        // Validate slots
        if (dto.AvailableSlots > dto.TotalSlots)
        {
            throw new BadRequestException("Available slots cannot exceed total slots");
        }

        if (dto.TotalSlots <= 0)
        {
            throw new BadRequestException("Total slots must be greater than 0");
        }

        if (dto.PricePerHour < 0)
        {
            throw new BadRequestException("Price per hour cannot be negative");
        }

        var entity = new ParkingLocation
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Province = dto.Province,
            District = dto.District,
            Ward = dto.Ward,
            Street = dto.Street,
            Area = dto.Area,
            FullAddress = dto.FullAddress,
            TotalSlots = dto.TotalSlots,
            AvailableSlots = dto.AvailableSlots,
            PricePerHour = dto.PricePerHour,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, ct);
    }

    public async Task<IEnumerable<ParkingLocationResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        var locations = await _repository.GetAllAsync(ct);
        return locations.Select(loc => MapToDto(loc));
    }

    public async Task<IEnumerable<ParkingLocationResponseDto>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusInMeters = 3000,
        CancellationToken ct = default)
    {
        // Validate user coordinates
        if (!GeoDistanceHelper.IsValidLatitude(latitude))
        {
            throw new BadRequestException("Latitude must be between -90 and 90");
        }

        if (!GeoDistanceHelper.IsValidLongitude(longitude))
        {
            throw new BadRequestException("Longitude must be between -180 and 180");
        }

        if (radiusInMeters <= 0)
        {
            throw new BadRequestException("Radius must be greater than 0");
        }

        // Get active locations with available slots
        var locations = await _repository.GetActiveWithSlotsAsync(ct);

        // Calculate distance for each location and filter by radius
        var nearbyLocations = locations
            .Select(loc => new
            {
                Location = loc,
                Distance = GeoDistanceHelper.CalculateDistanceInMeters(
                    latitude, longitude,
                    loc.Latitude, loc.Longitude)
            })
            .Where(x => x.Distance <= radiusInMeters)
            .OrderBy(x => x.Distance)
            .Select(x => MapToDto(x.Location, x.Distance))
            .ToList();

        return nearbyLocations;
    }

    /// <summary>
    /// Maps ParkingLocation entity to DTO without distance.
    /// </summary>
    private static ParkingLocationResponseDto MapToDto(ParkingLocation entity)
    {
        return new ParkingLocationResponseDto(
            Id: entity.Id,
            Name: entity.Name,
            Description: entity.Description,
            Latitude: entity.Latitude,
            Longitude: entity.Longitude,
            Province: entity.Province,
            District: entity.District,
            Ward: entity.Ward,
            FullAddress: entity.FullAddress,
            AvailableSlots: entity.AvailableSlots,
            TotalSlots: entity.TotalSlots,
            PricePerHour: entity.PricePerHour,
            IsActive: entity.IsActive,
            Distance: null
        );
    }

    /// <summary>
    /// Maps ParkingLocation entity to DTO with calculated distance.
    /// Used for nearby search results.
    /// </summary>
    private static ParkingLocationResponseDto MapToDto(ParkingLocation entity, double distance)
    {
        return new ParkingLocationResponseDto(
            Id: entity.Id,
            Name: entity.Name,
            Description: entity.Description,
            Latitude: entity.Latitude,
            Longitude: entity.Longitude,
            Province: entity.Province,
            District: entity.District,
            Ward: entity.Ward,
            FullAddress: entity.FullAddress,
            AvailableSlots: entity.AvailableSlots,
            TotalSlots: entity.TotalSlots,
            PricePerHour: entity.PricePerHour,
            IsActive: entity.IsActive,
            Distance: Math.Round(distance, 2) // Round to 2 decimal places
        );
    }
}
