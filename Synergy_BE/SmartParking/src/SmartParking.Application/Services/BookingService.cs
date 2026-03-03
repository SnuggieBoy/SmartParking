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
    private readonly IPaymentRepository _paymentRepository;
    private readonly IExtensionRequestRepository _extensionRequestRepository;
    private readonly IWalletService _walletService;
    private readonly INotificationService _notificationService;

    public BookingService(
        IBookingRepository bookingRepository,
        IParkingLotRepository parkingLotRepository,
        IVehicleRepository vehicleRepository,
        IPaymentRepository paymentRepository,
        IExtensionRequestRepository extensionRequestRepository,
        IWalletService walletService,
        INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _parkingLotRepository = parkingLotRepository;
        _vehicleRepository = vehicleRepository;
        _paymentRepository = paymentRepository;
        _extensionRequestRepository = extensionRequestRepository;
        _walletService = walletService;
        _notificationService = notificationService;
    }

    public async Task<BookingDto> GetByIdAsync(Guid bookingId, Guid userId, bool isAdmin, bool isOwner = false, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        var canAccess = isAdmin || booking.UserId == userId || (isOwner && booking.ParkingLot?.OwnerId == userId);
        if (!canAccess)
        {
            throw new ForbiddenException();
        }

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

        // Không tạo PaymentTransaction ở đây - user thanh toán tại PaymentSelection (Wallet/VNPay/SePay)
        // Update occupancy
        await _parkingLotRepository.UpdateOccupancyAsync(request.ParkingLotId, 1, ct);

        // Reload with navigation properties
        created = await _bookingRepository.GetByIdAsync(created.BookingId, includeDeleted: false, ct);

        // Thông báo đặt chỗ thành công
        await _notificationService.CreateBookingNotificationAsync(
            userId,
            "Đặt chỗ thành công",
            $"Bạn đã đặt chỗ tại bãi xe {parkingLot.Name}. Tổng tiền: {totalAmount:N0}đ. Vui lòng thanh toán để hoàn tất.",
            created!.BookingId,
            ct);

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

        // Thông báo hủy booking
        await _notificationService.CreateBookingNotificationAsync(
            booking.UserId,
            "Booking đã bị hủy",
            "Đặt chỗ của bạn đã được hủy.",
            bookingId,
            ct);
    }

    public async Task<BookingCheckInResponseDto> BookingCheckInAsync(
        Guid bookingId,
        Guid userId,
        bool isAdmin,
        bool isOwner = false,
        CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        var canAccess = isAdmin || booking.UserId == userId || (isOwner && booking.ParkingLot?.OwnerId == userId);
        if (!canAccess)
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

    public async Task<BookingCheckOutPreviewDto?> GetCheckoutPreviewAsync(
        Guid bookingId,
        Guid userId,
        bool isAdmin,
        bool isOwner = false,
        CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null) return null;

        var canAccess = isAdmin || booking.UserId == userId || (isOwner && booking.ParkingLot?.OwnerId == userId);
        if (!canAccess) return null;

        if (!string.Equals(booking.Status, nameof(BookingStatus.InProgress), StringComparison.Ordinal))
            return null;

        var parkingLot = await _parkingLotRepository.GetByIdAsync(booking.ParkingLotId, includeDeleted: false, ct);
        if (parkingLot == null) return null;

        var now = DateTime.UtcNow;
        var actualStart = booking.CheckInTime ?? booking.StartTime;
        var actualEnd = now < actualStart ? actualStart : now;
        var duration = actualEnd - actualStart;
        var actualCharge = CalculateAmount(duration, parkingLot.PricePerHour);

        var paidAmount = await _paymentRepository.GetTotalPaidForBookingAsync(bookingId, ct);
        if (paidAmount <= 0) paidAmount = booking.TotalAmount;

        var isEarlyCheckout = actualCharge < paidAmount && paidAmount > 0;
        var unusedAmount = isEarlyCheckout ? paidAmount - actualCharge : 0;
        var refundAmount = Math.Round(unusedAmount * PaymentConstants.EarlyCheckoutRefundRate, 2);

        var message = isEarlyCheckout
            ? $"Bạn đang checkout sớm. Thời gian đậu: {duration.TotalHours:F1}h. Số tiền sẽ hoàn: {refundAmount:N0}đ (70% thời gian chưa dùng)."
            : $"Thời gian đậu: {duration.TotalHours:F1}h. Số tiền thanh toán: {actualCharge:N0}đ.";

        return new BookingCheckOutPreviewDto(
            bookingId,
            paidAmount,
            actualCharge,
            refundAmount,
            isEarlyCheckout,
            duration.TotalHours,
            message
        );
    }

    public async Task<BookingCheckOutResponseDto> BookingCheckOutAsync(
        Guid bookingId,
        Guid userId,
        bool isAdmin,
        bool isOwner = false,
        CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        var canAccess = isAdmin || booking.UserId == userId || (isOwner && booking.ParkingLot?.OwnerId == userId);
        if (!canAccess)
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

        // Thời gian tính từ CheckIn (owner bấm check-in), không phải StartTime
        var actualStart = booking.CheckInTime ?? booking.StartTime;

        if (actualEnd < actualStart)
        {
            actualEnd = actualStart;
        }

        var duration = actualEnd - actualStart;
        var actualCharge = CalculateAmount(duration, parkingLot.PricePerHour);

        var paidAmount = await _paymentRepository.GetTotalPaidForBookingAsync(bookingId, ct);
        if (paidAmount <= 0) paidAmount = booking.TotalAmount;

        decimal refundAmount = 0;
        var ownerId = booking.ParkingLot?.OwnerId ?? Guid.Empty;

        // Checkout sớm: hoàn 70% thời gian chưa dùng
        if (actualCharge < paidAmount && paidAmount > 0)
        {
            var unusedAmount = paidAmount - actualCharge;
            refundAmount = Math.Round(unusedAmount * PaymentConstants.EarlyCheckoutRefundRate, 2);

            if (refundAmount > 0 && ownerId != Guid.Empty)
            {
                await _walletService.RefundEarlyCheckoutAsync(bookingId, refundAmount, booking.UserId, ownerId, ct);
            }
        }

        booking.TotalAmount = actualCharge;
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
            actualCharge,
            refundAmount
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

        var lotName = parkingLot.Name ?? "Bãi xe";
        var dtos = new List<ParkingLotBookingDto>();
        foreach (var b in pagedResult.Items)
        {
            var payment = await _paymentRepository.GetLatestByBookingIdAsync(b.BookingId, ct);
            var paymentStatus = payment?.PaymentStatus ?? nameof(PaymentStatus.Pending);
            dtos.Add(new ParkingLotBookingDto(
                b.BookingId,
                b.UserId,
                lotName,
                b.User?.FullName ?? string.Empty,
                b.Vehicle?.LicensePlate,
                b.Status,
                b.StartTime,
                b.EndTime,
                b.CheckInTime,
                b.CheckOutTime,
                b.TotalAmount,
                paymentStatus
            ));
        }

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
            booking.CreatedAt,
            booking.CheckInTime,
            booking.CheckOutTime
        );
    }

    private static BookingListDto MapToListDto(Booking booking)
    {
        return new BookingListDto(
            booking.BookingId,
            booking.ParkingLotId,
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

    public async Task<ExtensionRequestDto> RequestExtensionAsync(Guid bookingId, DateTime newEndTime, Guid userId, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
            throw new NotFoundException(Messages.Booking.NotFound);

        if (booking.UserId != userId)
            throw new ForbiddenException();

        if (booking.Status != nameof(BookingStatus.InProgress) && booking.Status != nameof(BookingStatus.Confirmed))
            throw new BadRequestException("Can only extend confirmed or in-progress bookings");

        if (newEndTime <= booking.EndTime)
            throw new BadRequestException("New end time must be after current end time");

        var extReq = new ExtensionRequest
        {
            ExtensionRequestId = Guid.NewGuid(),
            BookingId = bookingId,
            RequestedEndTime = newEndTime,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        await _extensionRequestRepository.CreateAsync(extReq, ct);

        var parkingLot = await _parkingLotRepository.GetByIdAsync(booking.ParkingLotId, includeDeleted: false, ct);
        var availableSlots = parkingLot != null ? Math.Max(0, parkingLot.TotalCapacity - parkingLot.CurrentOccupancy) : 0;
        var totalCapacity = parkingLot?.TotalCapacity ?? 0;

        return new ExtensionRequestDto(
            extReq.ExtensionRequestId,
            bookingId,
            booking.ParkingLot?.Name ?? "Bãi xe",
            booking.User?.FullName ?? string.Empty,
            booking.Vehicle?.LicensePlate,
            booking.EndTime,
            newEndTime,
            availableSlots,
            totalCapacity,
            extReq.CreatedAt
        );
    }

    public async Task<IEnumerable<ExtensionRequestDto>> GetPendingExtensionRequestsAsync(Guid ownerId, CancellationToken ct = default)
    {
        var requests = await _extensionRequestRepository.GetPendingByOwnerIdAsync(ownerId, ct);
        var dtos = new List<ExtensionRequestDto>();
        foreach (var r in requests)
        {
            var pl = await _parkingLotRepository.GetByIdAsync(r.Booking!.ParkingLotId, includeDeleted: false, ct);
            var availableSlots = pl != null ? Math.Max(0, pl.TotalCapacity - pl.CurrentOccupancy) : 0;
            var totalCapacity = pl?.TotalCapacity ?? 0;
            dtos.Add(new ExtensionRequestDto(
                r.ExtensionRequestId,
                r.BookingId,
                r.Booking.ParkingLot?.Name ?? "Bãi xe",
                r.Booking.User?.FullName ?? string.Empty,
                r.Booking.Vehicle?.LicensePlate,
                r.Booking.EndTime,
                r.RequestedEndTime,
                availableSlots,
                totalCapacity,
                r.CreatedAt
            ));
        }
        return dtos;
    }

    public async Task<BookingDto> ApproveExtensionAsync(Guid extensionRequestId, Guid ownerId, bool isAdmin = false, CancellationToken ct = default)
    {
        var extReq = await _extensionRequestRepository.GetByIdAsync(extensionRequestId, ct);
        if (extReq == null)
            throw new NotFoundException("Extension request not found");

        if (extReq.Status != "Pending")
            throw new BadRequestException("Extension request is no longer pending");

        var booking = extReq.Booking;
        if (booking == null)
            throw new NotFoundException(Messages.Booking.NotFound);

        if (!isAdmin && booking.ParkingLot?.OwnerId != ownerId)
            throw new ForbiddenException("Bạn không phải chủ bãi xe của booking này.");

        if (booking.Status != nameof(BookingStatus.InProgress) && booking.Status != nameof(BookingStatus.Confirmed))
            throw new BadRequestException("Can only extend confirmed or in-progress bookings");

        var parkingLot = await _parkingLotRepository.GetByIdAsync(booking.ParkingLotId, includeDeleted: false, ct);
        var duration = extReq.RequestedEndTime - booking.StartTime;
        var newTotalAmount = CalculateAmount(duration, parkingLot!.PricePerHour);
        var extensionAmount = newTotalAmount - booking.TotalAmount;

        if (extensionAmount > 0)
        {
            var payResult = await _walletService.PayExtensionWithWalletAsync(booking.UserId, booking.BookingId, extensionAmount, ct);
            if (!payResult.Success)
                throw new BadRequestException(payResult.Message ?? "Số dư ví không đủ để thanh toán gia hạn. User cần nạp tiền.");
        }

        booking.EndTime = extReq.RequestedEndTime;
        booking.TotalAmount = newTotalAmount;
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking, ct);

        extReq.Status = "Approved";
        extReq.ProcessedAt = DateTime.UtcNow;
        extReq.ProcessedBy = ownerId;
        await _extensionRequestRepository.UpdateAsync(extReq, ct);

        booking = await _bookingRepository.GetByIdAsync(booking.BookingId, includeDeleted: false, ct)!;
        return MapToDto(booking!);
    }

    public async Task RejectExtensionAsync(Guid extensionRequestId, Guid ownerId, string reason, bool isAdmin = false, CancellationToken ct = default)
    {
        var extReq = await _extensionRequestRepository.GetByIdAsync(extensionRequestId, ct);
        if (extReq == null)
            throw new NotFoundException("Extension request not found");

        if (extReq.Status != "Pending")
            throw new BadRequestException("Extension request is no longer pending");

        var booking = extReq.Booking;
        if (booking == null)
            throw new NotFoundException(Messages.Booking.NotFound);

        if (!isAdmin && booking.ParkingLot?.OwnerId != ownerId)
            throw new ForbiddenException("Bạn không phải chủ bãi xe của booking này.");

        extReq.Status = "Rejected";
        extReq.RejectReason = reason;
        extReq.ProcessedAt = DateTime.UtcNow;
        extReq.ProcessedBy = ownerId;
        await _extensionRequestRepository.UpdateAsync(extReq, ct);
    }

    public async Task<PagedResult<ParkingLotBookingDto>> GetOwnerBookingsAsync(
        Guid ownerId,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var pagedResult = await _bookingRepository.GetByOwnerIdAsync(ownerId, status, page, pageSize, ct);

        var dtos = new List<ParkingLotBookingDto>();
        foreach (var b in pagedResult.Items)
        {
            var payment = await _paymentRepository.GetLatestByBookingIdAsync(b.BookingId, ct);
            var paymentStatus = payment?.PaymentStatus ?? nameof(PaymentStatus.Pending);
            dtos.Add(new ParkingLotBookingDto(
                b.BookingId,
                b.UserId,
                b.ParkingLot?.Name ?? "Bãi xe",
                b.User?.FullName ?? string.Empty,
                b.Vehicle?.LicensePlate,
                b.Status,
                b.StartTime,
                b.EndTime,
                b.CheckInTime,
                b.CheckOutTime,
                b.TotalAmount,
                paymentStatus
            ));
        }

        return new PagedResult<ParkingLotBookingDto>(dtos, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount);
    }

    public async Task<BookingDto> ApproveBookingAsync(Guid bookingId, Guid ownerId, bool isAdmin = false, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // Validate Owner (Admin bypasses)
        if (!isAdmin && booking.ParkingLot?.OwnerId != ownerId)
        {
            throw new ForbiddenException("Bạn không phải chủ bãi xe của booking này.");
        }

        if (booking.Status != nameof(BookingStatus.Pending))
        {
            throw new BadRequestException("Only pending bookings can be approved.");
        }

        var payment = await _paymentRepository.GetLatestByBookingIdAsync(bookingId, ct);
        if (payment == null || payment.PaymentStatus != nameof(PaymentStatus.Success))
        {
            throw new BadRequestException("Booking must be paid before owner can approve. User needs to complete payment first.");
        }

        booking.Status = nameof(BookingStatus.Confirmed);
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking, ct);

        // Chuyển tiền sang ví owner (tiền đã trừ từ user lúc thanh toán, pending cho tới khi duyệt)
        var parkingLotOwnerId = booking.ParkingLot?.OwnerId ?? Guid.Empty;
        var totalPaid = await _paymentRepository.GetTotalPaidForBookingAsync(bookingId, ct);
        if (parkingLotOwnerId != Guid.Empty && totalPaid > 0)
        {
            await _walletService.TransferBookingToOwnerAsync(bookingId, totalPaid, parkingLotOwnerId, ct);
        }

        // Thông báo booking đã được duyệt
        await _notificationService.CreateBookingNotificationAsync(
            booking.UserId,
            "Booking đã được duyệt",
            $"Đặt chỗ tại bãi xe {booking.ParkingLot?.Name ?? "bãi xe"} đã được chủ bãi duyệt. Bạn có thể Check-in khi đến.",
            bookingId,
            ct);

        // Reload to ensure updated data
        return MapToDto(booking);
    }

    public async Task RejectBookingAsync(Guid bookingId, Guid ownerId, string reason, bool isAdmin = false, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
        {
            throw new NotFoundException(Messages.Booking.NotFound);
        }

        // Validate Owner (Admin bypasses)
        if (!isAdmin && booking.ParkingLot?.OwnerId != ownerId)
        {
            throw new ForbiddenException("Bạn không phải chủ bãi xe của booking này.");
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

        // Thông báo booking bị từ chối
        await _notificationService.CreateBookingNotificationAsync(
            booking.UserId,
            "Booking đã bị từ chối",
            $"Đặt chỗ tại bãi xe {booking.ParkingLot?.Name ?? "bãi xe"} đã bị chủ bãi từ chối.{(string.IsNullOrWhiteSpace(reason) ? "" : $" Lý do: {reason}")}",
            bookingId,
            ct);
    }
}
