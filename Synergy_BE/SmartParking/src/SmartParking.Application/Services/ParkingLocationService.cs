using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Helpers;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Location;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Services;

public sealed class ParkingLocationService : IParkingLocationService
{
    private readonly IParkingLocationRepository _locationRepository;
    private readonly IParkingLotRepository _parkingLotRepository;

    public ParkingLocationService(
        IParkingLocationRepository locationRepository,
        IParkingLotRepository parkingLotRepository)
    {
        _locationRepository = locationRepository;
        _parkingLotRepository = parkingLotRepository;
    }

    public async Task<ParkingLocationResponseDto> GetByIdAsync(Guid locationId, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, includeDeleted: false, ct);
        if (location == null)
        {
            throw new NotFoundException("Parking location not found");
        }

        return MapToResponseDto(location);
    }

    public async Task<ParkingLocationResponseDto?> GetByParkingLotIdAsync(Guid parkingLotId, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByParkingLotIdAsync(parkingLotId, includeDeleted: false, ct);
        if (location == null)
        {
            return null;
        }

        return MapToResponseDto(location);
    }

    public async Task<IEnumerable<ParkingLocationResponseDto>> GetNearbyAsync(
        NearbyLocationRequestDto request,
        CancellationToken ct = default)
    {
        // Validate radius
        if (request.RadiusInMeters < 100 || request.RadiusInMeters > 50000)
        {
            throw new BadRequestException("Radius must be between 100 and 50000 meters");
        }

        var locations = await _locationRepository.GetNearbyAsync(
            request.Latitude,
            request.Longitude,
            request.RadiusInMeters,
            ct);

        return locations.Select(loc =>
        {
            var distance = GeoDistanceHelper.CalculateDistanceInMeters(
                request.Latitude,
                request.Longitude,
                loc.Latitude,
                loc.Longitude);

            return MapToResponseDto(loc, distance);
        }).ToList();
    }

    public async Task<PagedResult<ParkingLocationResponseDto>> SearchAsync(
        SearchLocationRequestDto request,
        CancellationToken ct = default)
    {
        var pagedResult = await _locationRepository.SearchAsync(
            request.Province,
            request.District,
            request.Ward,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            ct);

        var dtos = pagedResult.Items.Select(loc => MapToResponseDto(loc)).ToList();

        return new PagedResult<ParkingLocationResponseDto>(
            dtos,
            pagedResult.Page,
            pagedResult.PageSize,
            pagedResult.TotalCount);
    }

    public async Task<ParkingLocationResponseDto> CreateAsync(
        CreateLocationDto request,
        Guid userId,
        CancellationToken ct = default)
    {
        // Verify parking lot exists
        var parkingLot = await _parkingLotRepository.GetByIdAsync(request.ParkingLotId, includeDeleted: false, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        // Check if location already exists for this parking lot
        var existing = await _locationRepository.GetByParkingLotIdAsync(request.ParkingLotId, includeDeleted: false, ct);
        if (existing != null)
        {
            throw new BadRequestException("Location already exists for this parking lot");
        }

        var location = new ParkingLocation
        {
            LocationId = Guid.NewGuid(),
            ParkingLotId = request.ParkingLotId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Province = request.Province,
            District = request.District,
            Ward = request.Ward,
            Street = request.Street,
            Area = request.Area,
            FullAddress = request.FullAddress,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsDeleted = false
        };

        var created = await _locationRepository.CreateAsync(location, ct);
        created.ParkingLot = parkingLot; // Set for DTO mapping
        return MapToResponseDto(created);
    }

    public async Task<ParkingLocationResponseDto> UpdateAsync(
        Guid locationId,
        UpdateLocationDto request,
        Guid userId,
        bool isAdmin,
        CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, includeDeleted: false, ct);
        if (location == null)
        {
            throw new NotFoundException("Parking location not found");
        }

        // SECURITY: Only Admin can update locations (or parking lot owner - can be added)
        if (!isAdmin)
        {
            throw new ForbiddenException("Only administrators can update parking locations");
        }

        if (request.Latitude.HasValue)
        {
            location.Latitude = request.Latitude.Value;
        }

        if (request.Longitude.HasValue)
        {
            location.Longitude = request.Longitude.Value;
        }

        if (request.Province != null)
        {
            location.Province = request.Province;
        }

        if (request.District != null)
        {
            location.District = request.District;
        }

        if (request.Ward != null)
        {
            location.Ward = request.Ward;
        }

        if (request.Street != null)
        {
            location.Street = request.Street;
        }

        if (request.Area != null)
        {
            location.Area = request.Area;
        }

        if (request.FullAddress != null)
        {
            location.FullAddress = request.FullAddress;
        }

        location.UpdatedAt = DateTime.UtcNow;
        location.UpdatedBy = userId;

        await _locationRepository.UpdateAsync(location, ct);
        return MapToResponseDto(location);
    }

    public async Task DeleteAsync(Guid locationId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var location = await _locationRepository.GetByIdAsync(locationId, includeDeleted: false, ct);
        if (location == null)
        {
            throw new NotFoundException("Parking location not found");
        }

        // SECURITY: Only Admin can delete locations
        if (!isAdmin)
        {
            throw new ForbiddenException("Only administrators can delete parking locations");
        }

        await _locationRepository.SoftDeleteAsync(locationId, userId, ct);
    }

    private static ParkingLocationResponseDto MapToResponseDto(ParkingLocation location, double? distanceInMeters = null)
    {
        return new ParkingLocationResponseDto(
            LocationId: location.LocationId,
            ParkingLotId: location.ParkingLotId,
            ParkingLotName: location.ParkingLot?.Name ?? "Unknown",
            Latitude: location.Latitude,
            Longitude: location.Longitude,
            Province: location.Province,
            District: location.District,
            Ward: location.Ward,
            Street: location.Street,
            FullAddress: location.FullAddress,
            DistanceInMeters: distanceInMeters
        );
    }
}
