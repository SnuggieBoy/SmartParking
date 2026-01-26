using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Helpers;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.ParkingLot;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Services;

public sealed class ParkingLotService : IParkingLotService
{
    private readonly IParkingLotRepository _parkingLotRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public ParkingLotService(
        IParkingLotRepository parkingLotRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _parkingLotRepository = parkingLotRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<ParkingLotResponseDto> GetByIdAsync(Guid parkingLotId, CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotRepository.GetByIdAsync(parkingLotId, includeDeleted: false, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        return MapToResponseDto(parkingLot);
    }

    public async Task<PagedResult<ParkingLotResponseDto>> GetAllAsync(ParkingLotFilterDto filter, CancellationToken ct = default)
    {
        var pagedResult = await _parkingLotRepository.GetAllAsync(
            filter.SearchTerm,
            filter.IsActive,
            filter.Status,
            filter.Page,
            filter.PageSize,
            ct);

        var dtos = pagedResult.Items.Select(MapToResponseDto).ToList();
        
        return new PagedResult<ParkingLotResponseDto>(
            dtos,
            pagedResult.Page,
            pagedResult.PageSize,
            pagedResult.TotalCount);
    }

    public async Task<IEnumerable<ParkingLotResponseDto>> GetMyParkingLotsAsync(Guid ownerId, CancellationToken ct = default)
    {
        var parkingLots = await _parkingLotRepository.GetByOwnerIdAsync(ownerId, includeDeleted: false, ct);
        return parkingLots.Select(MapToResponseDto);
    }

    public async Task<ParkingLotResponseDto> CreateAsync(CreateParkingLotDto request, Guid ownerId, CancellationToken ct = default)
    {
        // Verify owner exists and has Owner or Admin role
        var owner = await _userRepository.GetByIdAsync(ownerId, ct);
        if (owner == null)
        {
            throw new NotFoundException(Messages.Auth.UserNotFound);
        }

        // Verify user is Owner or Admin
        if (owner.Role.RoleName != AuthConstants.Roles.Owner && 
            owner.Role.RoleName != AuthConstants.Roles.Admin)
        {
            throw new ForbiddenException("Only Owners and Admins can create parking lots");
        }

        var parkingLot = new ParkingLot
        {
            ParkingLotId = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            TotalCapacity = request.TotalCapacity,
            CurrentOccupancy = 0,
            PricePerHour = request.PricePerHour,
            Status = "Active",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = ownerId,
            IsDeleted = false
        };

        var created = await _parkingLotRepository.CreateAsync(parkingLot, ct);
        created.Owner = owner; // Set for DTO mapping
        return MapToResponseDto(created);
    }

    public async Task<ParkingLotResponseDto> UpdateAsync(
        Guid parkingLotId, 
        UpdateParkingLotDto request, 
        Guid userId, 
        bool isAdmin,
        CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotRepository.GetByIdAsync(parkingLotId, includeDeleted: false, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(parkingLot.OwnerId, userId, isAdmin);

        // Validate capacity cannot be less than current occupancy
        if (request.TotalCapacity < parkingLot.CurrentOccupancy)
        {
            throw new BadRequestException(
                $"Total capacity cannot be less than current occupancy ({parkingLot.CurrentOccupancy})");
        }

        // Update fields
        parkingLot.Name = request.Name.Trim();
        parkingLot.Address = request.Address.Trim();
        parkingLot.TotalCapacity = request.TotalCapacity;
        parkingLot.PricePerHour = request.PricePerHour;
        parkingLot.IsActive = request.IsActive;
        parkingLot.UpdatedAt = DateTime.UtcNow;
        parkingLot.UpdatedBy = userId;

        await _parkingLotRepository.UpdateAsync(parkingLot, ct);
        return MapToResponseDto(parkingLot);
    }

    public async Task DeleteAsync(Guid parkingLotId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotRepository.GetByIdAsync(parkingLotId, includeDeleted: false, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(parkingLot.OwnerId, userId, isAdmin);

        // Soft delete
        await _parkingLotRepository.SoftDeleteAsync(parkingLotId, userId, ct);
    }

    public async Task<ParkingLotResponseDto> ToggleActiveAsync(Guid parkingLotId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotRepository.GetByIdAsync(parkingLotId, includeDeleted: false, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(parkingLot.OwnerId, userId, isAdmin);

        // Toggle IsActive
        parkingLot.IsActive = !parkingLot.IsActive;
        parkingLot.UpdatedAt = DateTime.UtcNow;
        parkingLot.UpdatedBy = userId;

        await _parkingLotRepository.UpdateAsync(parkingLot, ct);
        return MapToResponseDto(parkingLot);
    }

    private static ParkingLotResponseDto MapToResponseDto(ParkingLot parkingLot)
    {
        var availableCapacity = parkingLot.TotalCapacity - parkingLot.CurrentOccupancy;
        
        return new ParkingLotResponseDto(
            ParkingLotId: parkingLot.ParkingLotId,
            OwnerId: parkingLot.OwnerId,
            OwnerName: parkingLot.Owner?.FullName ?? "Unknown",
            Name: parkingLot.Name,
            Address: parkingLot.Address,
            TotalCapacity: parkingLot.TotalCapacity,
            AvailableCapacity: availableCapacity > 0 ? availableCapacity : 0,
            CurrentOccupancy: parkingLot.CurrentOccupancy,
            PricePerHour: parkingLot.PricePerHour,
            Status: parkingLot.Status,
            IsActive: parkingLot.IsActive,
            CreatedAt: parkingLot.CreatedAt,
            UpdatedAt: parkingLot.UpdatedAt
        );
    }
}
