using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Helpers;
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

    public ParkingLotService(
        IParkingLotRepository parkingLotRepository,
        IUserRepository userRepository)
    {
        _parkingLotRepository = parkingLotRepository;
        _userRepository = userRepository;
    }

    public async Task<ParkingLotDto> GetByIdAsync(Guid parkingLotId, CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotRepository.GetByIdAsync(parkingLotId, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        return MapToDto(parkingLot);
    }

    public async Task<IEnumerable<ParkingLotDto>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var parkingLots = await _parkingLotRepository.GetAllAsync(activeOnly, ct);
        return parkingLots.Select(MapToDto);
    }

    public async Task<IEnumerable<ParkingLotDto>> GetMyParkingLotsAsync(Guid ownerId, CancellationToken ct = default)
    {
        var parkingLots = await _parkingLotRepository.GetByOwnerIdAsync(ownerId, ct);
        return parkingLots.Select(MapToDto);
    }

    public async Task<ParkingLotDto> CreateAsync(CreateParkingLotDto request, Guid ownerId, CancellationToken ct = default)
    {
        var owner = await _userRepository.GetByIdAsync(ownerId, ct);
        if (owner == null)
        {
            throw new NotFoundException(Messages.Auth.UserNotFound);
        }

        var parkingLot = new ParkingLot
        {
            ParkingLotId = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = request.Name,
            Address = request.Address,
            TotalCapacity = request.TotalCapacity,
            CurrentOccupancy = 0,
            PricePerHour = request.PricePerHour,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _parkingLotRepository.CreateAsync(parkingLot, ct);
        return MapToDto(created);
    }

    public async Task<ParkingLotDto> UpdateAsync(
        Guid parkingLotId, 
        UpdateParkingLotDto request, 
        Guid ownerId, 
        bool isAdmin,
        CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotRepository.GetByIdAsync(parkingLotId, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(parkingLot.OwnerId, ownerId, isAdmin);

        parkingLot.Name = request.Name;
        parkingLot.Address = request.Address;
        parkingLot.TotalCapacity = request.TotalCapacity;
        parkingLot.PricePerHour = request.PricePerHour;
        parkingLot.Status = request.Status;

        await _parkingLotRepository.UpdateAsync(parkingLot, ct);
        return MapToDto(parkingLot);
    }

    public async Task DeleteAsync(Guid parkingLotId, Guid ownerId, bool isAdmin, CancellationToken ct = default)
    {
        var parkingLot = await _parkingLotRepository.GetByIdAsync(parkingLotId, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(parkingLot.OwnerId, ownerId, isAdmin);

        await _parkingLotRepository.DeleteAsync(parkingLotId, ct);
    }

    private static ParkingLotDto MapToDto(ParkingLot parkingLot)
    {
        var availableCapacity = parkingLot.TotalCapacity - (parkingLot.CurrentOccupancy ?? 0);
        
        return new ParkingLotDto(
            parkingLot.ParkingLotId,
            parkingLot.Name,
            parkingLot.Address,
            parkingLot.TotalCapacity,
            availableCapacity,
            parkingLot.CurrentOccupancy ?? 0,
            parkingLot.PricePerHour ?? 0,
            parkingLot.Status ?? "Active",
            parkingLot.CreatedAt
        );
    }
}
