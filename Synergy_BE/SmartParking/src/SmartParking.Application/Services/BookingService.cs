using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Helpers;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Booking;
using SmartParking.Application.DTOs.User;
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

    public async Task<BookingDto> GetByIdAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(booking.UserId, userId, isAdmin);

        return MapToDto(booking);
    }

    public async Task<PagedResult<BookingListDto>> GetMyBookingsAsync(
        Guid userId,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var pagedResult = await _bookingRepository.GetByUserIdAsync(userId, status, page, pageSize, ct);
        var dtos = pagedResult.Items.Select(MapToListDto).ToList();
        return new PagedResult<BookingListDto>(dtos, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto request, Guid userId, CancellationToken ct = default)
    {
        // Validate parking lot exists and is active
        var parkingLot = await _parkingLotRepository.GetByIdAsync(request.ParkingLotId, includeDeleted: false, ct);
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
        var totalAmount = CalculateAmount(duration, parkingLot.PricePerHour);

        var now = DateTime.UtcNow;
        var booking = new Booking
        {
            BookingId = Guid.NewGuid(),
            UserId = userId,
            ParkingLotId = request.ParkingLotId,
            VehicleId = request.VehicleId,
            BookingTime = now,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = nameof(BookingStatus.Pending),
            TotalAmount = totalAmount,
            CreatedAt = now,
            CreatedBy = userId,
            IsDeleted = false
        };

        var created = await _bookingRepository.CreateAsync(booking, ct);
        
        // Update occupancy
        await _parkingLotRepository.UpdateOccupancyAsync(request.ParkingLotId, 1, ct);

        // Reload with navigation properties
        created = await _bookingRepository.GetByIdAsync(created.BookingId, includeDeleted: false, ct);
        return MapToDto(created!);
    }

    public async Task<BookingDto> UpdateAsync(Guid bookingId, UpdateBookingDto request, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(booking.UserId, userId, isAdmin);

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
        var parkingLot = await _parkingLotRepository.GetByIdAsync(booking.ParkingLotId, includeDeleted: false, ct);
        var duration = request.EndTime - request.StartTime;
        var totalAmount = CalculateAmount(duration, parkingLot!.PricePerHour);

        booking.StartTime = request.StartTime;
        booking.EndTime = request.EndTime;
        booking.TotalAmount = totalAmount;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, ct);

        // Reload with navigation properties
        booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        return MapToDto(booking!);
    }

    public async Task CancelAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(booking.UserId, userId, isAdmin);

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

    public async Task<BookingCheckInResponseDto> BookingCheckInAsync(
        Guid bookingId,
        Guid userId,
        bool isAdmin,
        CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        if (!isAdmin && booking.UserId != userId)
        {
            throw new ForbiddenException();
        }

        if (!string.Equals(booking.Status, nameof(BookingStatus.Confirmed), StringComparison.Ordinal))
        {
            throw new BadRequestException(Messages.Booking.InvalidStatusForCheckIn);
        }

        var now = DateTime.UtcNow;

        booking.CheckInTime = now;
        booking.Status = nameof(BookingStatus.InProgress);
        booking.UpdatedAt = now;

        await _bookingRepository.UpdateAsync(booking, ct);

        return new BookingCheckInResponseDto(
            booking.BookingId,
            booking.Status ?? nameof(BookingStatus.InProgress),
            now
        );
    }

    public async Task<BookingCheckOutResponseDto> BookingCheckOutAsync(
        Guid bookingId,
        Guid userId,
        bool isAdmin,
        CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        if (!isAdmin && booking.UserId != userId)
        {
            throw new ForbiddenException();
        }

        if (!string.Equals(booking.Status, nameof(BookingStatus.InProgress), StringComparison.Ordinal))
        {
            throw new BadRequestException(Messages.Booking.InvalidStatusForCheckOut);
        }

        var parkingLot = await _parkingLotRepository.GetByIdAsync(booking.ParkingLotId, includeDeleted: false, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        var now = DateTime.UtcNow;
        var actualEnd = now;
        
        // Use CheckInTime if available, otherwise fallback to StartTime
        var actualStart = booking.CheckInTime ?? booking.StartTime;
        
        if (actualEnd < actualStart)
        {
            actualEnd = actualStart;
        }

        var duration = actualEnd - actualStart;
        var totalAmount = CalculateAmount(duration, parkingLot.PricePerHour);

        booking.TotalAmount = totalAmount;
        booking.CheckOutTime = now;
        booking.Status = nameof(BookingStatus.Completed);
        booking.UpdatedAt = now;

        await _bookingRepository.UpdateAsync(booking, ct);

        // Update occupancy (free one slot)
        await _parkingLotRepository.UpdateOccupancyAsync(booking.ParkingLotId, -1, ct);

        return new BookingCheckOutResponseDto(
            booking.BookingId,
            booking.Status,
            now,
            totalAmount
        );
    }

    public async Task<PagedResult<ParkingLotBookingDto>> GetBookingsByParkingLotAsync(
        Guid parkingLotId,
        Guid userId,
        bool isAdmin,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var parkingLot = await _parkingLotRepository.GetByIdAsync(parkingLotId, includeDeleted: false, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        if (!isAdmin && parkingLot.OwnerId != userId)
        {
            throw new ForbiddenException();
        }

        var pagedResult = await _bookingRepository.GetByParkingLotIdPagedAsync(parkingLotId, null, page, pageSize, ct);

        var dtos = pagedResult.Items
            .Select(b => new ParkingLotBookingDto(
                b.BookingId,
                b.User?.FullName ?? string.Empty,
                b.Vehicle?.LicensePlate,
                b.Status,
                b.StartTime,
                b.EndTime,
                b.CheckInTime,
                b.CheckOutTime,
                b.TotalAmount
            ))
            .ToList();

        return new PagedResult<ParkingLotBookingDto>(dtos, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);
    }

    public async Task<PagedResult<BookingListDto>> GetAllBookingsAsync(
        string? status,
        Guid? userId,
        Guid? parkingLotId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var pagedResult = await _bookingRepository.GetAllAsync(status, userId, parkingLotId, page, pageSize, ct);
        var dtos = pagedResult.Items.Select(MapToListDto).ToList();
        return new PagedResult<BookingListDto>(dtos, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);
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
            booking.Status,
            booking.TotalAmount,
            booking.CreatedAt
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
            booking.Status,
            booking.TotalAmount,
            booking.CheckInTime,
            booking.CheckOutTime
        );
    }

    public async Task<PagedResult<BookingHistoryDto>> GetBookingHistoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        // Get only completed bookings
        var pagedResult = await _bookingRepository.GetByUserIdAsync(
            userId,
            nameof(BookingStatus.Completed),
            page,
            pageSize,
            ct);

        var dtos = pagedResult.Items.Select(b => new BookingHistoryDto(
            BookingId: b.BookingId,
            ParkingLotName: b.ParkingLot?.Name ?? string.Empty,
            ParkingLotAddress: b.ParkingLot?.Address ?? string.Empty,
            VehiclePlate: b.Vehicle?.LicensePlate,
            StartTime: b.StartTime,
            EndTime: b.EndTime,
            CheckInTime: b.CheckInTime,
            CheckOutTime: b.CheckOutTime,
            TotalAmount: b.TotalAmount,
            PaymentStatus: b.PaymentTransactions?.FirstOrDefault()?.PaymentStatus ?? "Unknown",
            CompletedAt: b.CheckOutTime ?? b.UpdatedAt ?? b.CreatedAt
        )).ToList();

        return new PagedResult<BookingHistoryDto>(dtos, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);
    }

    public async Task<InvoiceDto> GetInvoiceAsync(Guid bookingId, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // SECURITY: Validate ownership or Admin access
        SecurityHelper.ValidateOwnership(booking.UserId, userId, isAdmin);

        // Booking must be completed to generate invoice
        if (booking.Status != nameof(BookingStatus.Completed))
        {
            throw new BadRequestException("Invoice can only be generated for completed bookings");
        }

        var parkingLot = await _parkingLotRepository.GetByIdAsync(booking.ParkingLotId, includeDeleted: false, ct);
        if (parkingLot == null)
        {
            throw new NotFoundException(Messages.ParkingLot.NotFound);
        }

        // Calculate duration
        var actualStart = booking.CheckInTime ?? booking.StartTime;
        var actualEnd = booking.CheckOutTime ?? booking.EndTime;
        var durationMinutes = (int)(actualEnd - actualStart).TotalMinutes;

        // Get payment info
        var payment = booking.PaymentTransactions?.FirstOrDefault(p => !p.IsDeleted);

        // Generate invoice number
        var invoiceNumber = $"INV-{booking.BookingId.ToString()[..8].ToUpper()}-{booking.CreatedAt:yyyyMMdd}";

        return new InvoiceDto(
            InvoiceNumber: invoiceNumber,
            InvoiceDate: DateTime.UtcNow,
            UserId: booking.UserId,
            CustomerName: booking.User?.FullName ?? "Unknown",
            CustomerEmail: booking.User?.Email ?? "",
            CustomerPhone: booking.User?.Phone,
            BookingId: booking.BookingId,
            ParkingLotName: parkingLot.Name ?? "",
            ParkingLotAddress: parkingLot.Address ?? "",
            VehiclePlate: booking.Vehicle?.LicensePlate,
            StartTime: booking.StartTime,
            EndTime: booking.EndTime,
            CheckInTime: booking.CheckInTime,
            CheckOutTime: booking.CheckOutTime,
            DurationMinutes: durationMinutes,
            PricePerHour: parkingLot.PricePerHour,
            SubTotal: booking.TotalAmount,
            Commission: 0, // Commission is internal, not shown on invoice
            TotalAmount: booking.TotalAmount,
            PaymentMethod: payment?.PaymentMethod ?? "N/A",
            PaymentStatus: payment?.PaymentStatus ?? "N/A",
            TransactionRef: payment?.VnpTxnRef ?? payment?.SePayOrderId ?? "N/A",
            PaidAt: payment?.UpdatedAt,
            OwnerName: parkingLot.Owner?.FullName ?? "N/A",
            OwnerBankAccount: null, // Not exposed on invoice for privacy
            OwnerBankName: null
        );
    }

    public async Task<BookingDto> ExtendBookingAsync(Guid bookingId, DateTime newEndTime, Guid userId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // Only owner can extend
        if (booking.UserId != userId)
        {
            throw new ForbiddenException();
        }

        // Can only extend InProgress or Confirmed bookings
        if (booking.Status != nameof(BookingStatus.InProgress) && booking.Status != nameof(BookingStatus.Confirmed))
        {
            throw new BadRequestException("Can only extend confirmed or in-progress bookings");
        }

        // New end time must be after current end time
        if (newEndTime <= booking.EndTime)
        {
            throw new BadRequestException("New end time must be after current end time");
        }

        // Recalculate amount
        var parkingLot = await _parkingLotRepository.GetByIdAsync(booking.ParkingLotId, includeDeleted: false, ct);
        var duration = newEndTime - booking.StartTime;
        var totalAmount = CalculateAmount(duration, parkingLot!.PricePerHour);

        booking.EndTime = newEndTime;
        booking.TotalAmount = totalAmount;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, ct);

        // Reload with navigation properties
        booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        return MapToDto(booking!);
    }
    public async Task<PagedResult<ParkingLotBookingDto>> GetOwnerBookingsAsync(
        Guid ownerId,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var pagedResult = await _bookingRepository.GetByOwnerIdAsync(ownerId, status, page, pageSize, ct);

        var dtos = pagedResult.Items
            .Select(b => new ParkingLotBookingDto(
                b.BookingId,
                b.User?.FullName ?? string.Empty,
                b.Vehicle?.LicensePlate,
                b.Status,
                b.StartTime,
                b.EndTime,
                b.CheckInTime,
                b.CheckOutTime,
                b.TotalAmount
            ))
            .ToList();

        return new PagedResult<ParkingLotBookingDto>(dtos, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);
    }

    public async Task<BookingDto> ApproveBookingAsync(Guid bookingId, Guid ownerId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // Validate Owner
        if (booking.ParkingLot?.OwnerId != ownerId)
        {
            throw new ForbiddenException();
        }

        if (booking.Status != nameof(BookingStatus.Pending))
        {
            throw new BadRequestException("Only pending bookings can be approved.");
        }

        booking.Status = nameof(BookingStatus.Confirmed);
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, ct);

        // Reload to ensure updated data
        return MapToDto(booking);
    }

    public async Task RejectBookingAsync(Guid bookingId, Guid ownerId, string reason, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // Validate Owner
        if (booking.ParkingLot?.OwnerId != ownerId)
        {
            throw new ForbiddenException();
        }

        if (booking.Status != nameof(BookingStatus.Pending) && booking.Status != nameof(BookingStatus.Confirmed))
        {
             throw new BadRequestException("Cannot reject completed or cancelled bookings.");
        }

        booking.Status = nameof(BookingStatus.Cancelled);
        booking.UpdatedAt = DateTime.UtcNow;
        // Note: Booking entity doesn't have RejectReason, so we just Cancel.

        await _bookingRepository.UpdateAsync(booking, ct);
        
        // Update occupancy (if it was confirmed/active, need to free up checking logic? Pending bookings reserved a slot?)
        // CreateAsync updates occupancy +1 (Line 116).
        // CheckOut/Cancel updates occupancy -1.
        // So yes, we MUST decrease occupancy.
        await _parkingLotRepository.UpdateOccupancyAsync(booking.ParkingLotId, -1, ct);
    }
}
