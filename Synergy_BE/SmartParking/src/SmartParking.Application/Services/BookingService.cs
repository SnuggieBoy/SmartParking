using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;
using SmartParking.Domain.Enums;

namespace SmartParking.Application.Services;

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IParkingLotRepository _parkingLotRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IParkingLotRepository parkingLotRepository,
        IVehicleRepository vehicleRepository)
    {
        _bookingRepository = bookingRepository;
        _parkingLotRepository = parkingLotRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<BookingDto> GetByIdAsync(Guid bookingId, Guid userId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        if (booking.UserId != userId)
        {
            throw new UnauthorizedException(Messages.Common.Forbidden);
        }

        return MapToDto(booking);
    }

    public async Task<IEnumerable<BookingListDto>> GetMyBookingsAsync(Guid userId, CancellationToken ct = default)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(userId, ct);
        return bookings.Select(MapToListDto);
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto request, Guid userId, CancellationToken ct = default)
    {
        // Validate parking lot exists and is active
        var parkingLot = await _parkingLotRepository.GetByIdAsync(request.ParkingLotId, ct);
        if (parkingLot == null || parkingLot.Status != "Active")
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        // Check available slots
        if (!await _parkingLotRepository.HasAvailableSlotsAsync(request.ParkingLotId, ct))
        {
            throw new BadRequestException(Messages.Booking.NoAvailableSlots);
        }

        // Validate vehicle if provided
        if (request.VehicleId.HasValue)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId.Value, ct);
            if (vehicle == null || vehicle.UserId != userId)
            {
                throw new NotFoundException(Messages.Vehicle.NotFound);
            }
        }

        // Validate time range
        if (request.StartTime >= request.EndTime)
        {
            throw new BadRequestException("End time must be after start time");
        }

        if (request.StartTime < DateTime.UtcNow)
        {
            throw new BadRequestException("Start time cannot be in the past");
        }

        // Calculate total amount
        var duration = request.EndTime - request.StartTime;
        var totalAmount = CalculateAmount(duration, parkingLot.PricePerHour ?? 0);

        var booking = new Booking
        {
            BookingId = Guid.NewGuid(),
            UserId = userId,
            ParkingLotId = request.ParkingLotId,
            VehicleId = request.VehicleId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = nameof(BookingStatus.Pending),
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _bookingRepository.CreateAsync(booking, ct);
        
        // Update occupancy
        await _parkingLotRepository.UpdateOccupancyAsync(request.ParkingLotId, 1, ct);

        // Reload with navigation properties
        created = await _bookingRepository.GetByIdAsync(created.BookingId, ct);
        return MapToDto(created!);
    }

    public async Task<BookingDto> UpdateAsync(Guid bookingId, UpdateBookingDto request, Guid userId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        if (booking.UserId != userId)
        {
            throw new UnauthorizedException(Messages.Common.Forbidden);
        }

        if (booking.Status == nameof(BookingStatus.Cancelled) || booking.Status == nameof(BookingStatus.Completed))
        {
            throw new BadRequestException(Messages.Booking.CannotCancel);
        }

        // Validate time range
        if (request.StartTime >= request.EndTime)
        {
            throw new BadRequestException("End time must be after start time");
        }

        // Recalculate amount
        var parkingLot = await _parkingLotRepository.GetByIdAsync(booking.ParkingLotId, ct);
        var duration = request.EndTime - request.StartTime;
        var totalAmount = CalculateAmount(duration, parkingLot!.PricePerHour ?? 0);

        booking.StartTime = request.StartTime;
        booking.EndTime = request.EndTime;
        booking.TotalAmount = totalAmount;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, ct);

        // Reload with navigation properties
        booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
        return MapToDto(booking!);
    }

    public async Task CancelAsync(Guid bookingId, Guid userId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        if (booking.UserId != userId)
        {
            throw new UnauthorizedException(Messages.Common.Forbidden);
        }

        if (booking.Status == nameof(BookingStatus.Cancelled))
        {
            throw new BadRequestException(Messages.Booking.AlreadyCancelled);
        }

        if (booking.Status == nameof(BookingStatus.Completed))
        {
            throw new BadRequestException(Messages.Booking.CannotCancel);
        }

        booking.Status = nameof(BookingStatus.Cancelled);
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, ct);

        // Update occupancy
        await _parkingLotRepository.UpdateOccupancyAsync(booking.ParkingLotId, -1, ct);
    }

    private static decimal CalculateAmount(TimeSpan duration, decimal pricePerHour)
    {
        var totalHours = Math.Ceiling(duration.TotalHours);
        var amount = (decimal)totalHours * pricePerHour;
        return Math.Round(amount, 2);
    }

    private static BookingDto MapToDto(Booking booking)
    {
        return new BookingDto(
            booking.BookingId,
            booking.UserId,
            booking.ParkingLotId,
            booking.VehicleId,
            booking.ParkingLot?.Name ?? string.Empty,
            booking.Vehicle?.LicensePlate,
            booking.StartTime,
            booking.EndTime,
            booking.Status ?? nameof(BookingStatus.Pending),
            booking.TotalAmount ?? 0,
            booking.CreatedAt ?? DateTime.UtcNow
        );
    }

    private static BookingListDto MapToListDto(Booking booking)
    {
        return new BookingListDto(
            booking.BookingId,
            booking.ParkingLot?.Name ?? string.Empty,
            booking.Vehicle?.LicensePlate,
            booking.StartTime,
            booking.EndTime,
            booking.Status ?? nameof(BookingStatus.Pending),
            booking.TotalAmount ?? 0
        );
    }
}
