using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Helpers;
using SmartParking.Application.Common.Models;
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

    public async Task<PagedResult<ParkingLocationResponseDto>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        // Validate and enforce pagination limits
        if (page < PaginationConstants.MinPage)
            page = PaginationConstants.DefaultPage;
        
        if (pageSize < PaginationConstants.MinPageSize)
            pageSize = PaginationConstants.DefaultPageSize;
        
        if (pageSize > PaginationConstants.MaxPageSize)
            pageSize = PaginationConstants.MaxPageSize;

        var allLocations = await _repository.GetAllAsync(ct);
        var totalCount = allLocations.Count();
        
        var pagedLocations = allLocations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(loc => MapToDto(loc))
            .ToList();

        return new PagedResult<ParkingLocationResponseDto>(
            pagedLocations,
            page,
            pageSize,
            totalCount);
    }

    public async Task<PagedResult<ParkingLocationResponseDto>> GetNearbyAsync(
        double latitude,
        double longitude,
        double radiusInMeters = 3000,
        int page = 1,
        int pageSize = 10,
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

        // Validate and enforce pagination limits
        if (page < PaginationConstants.MinPage)
            page = PaginationConstants.DefaultPage;
        
        if (pageSize < PaginationConstants.MinPageSize)
            pageSize = PaginationConstants.DefaultPageSize;
        
        if (pageSize > PaginationConstants.MaxPageSize)
            pageSize = PaginationConstants.MaxPageSize;

        // PERFORMANCE OPTIMIZATION: Calculate bounding box to pre-filter candidates
        var (minLat, maxLat, minLon, maxLon) = GeoDistanceHelper.GetBoundingBox(
            latitude, longitude, radiusInMeters);

        // Get active locations within bounding box (reduces candidate set significantly)
        var locations = await _repository.GetActiveWithinBoundsAsync(
            minLat, maxLat, minLon, maxLon, ct);

        // Calculate precise distance for remaining candidates and filter by exact radius
        var nearbyLocationsWithDistance = locations
            .Select(loc => new
            {
                Location = loc,
                Distance = GeoDistanceHelper.CalculateDistanceInMeters(
                    latitude, longitude,
                    loc.Latitude, loc.Longitude)
            })
            .Where(x => x.Distance <= radiusInMeters)
            .OrderBy(x => x.Distance)
            .ToList();

        var totalCount = nearbyLocationsWithDistance.Count;

        var pagedLocations = nearbyLocationsWithDistance
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapToDto(x.Location, x.Distance))
            .ToList();

        return new PagedResult<ParkingLocationResponseDto>(
            pagedLocations,
            page,
            pageSize,
            totalCount);
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
