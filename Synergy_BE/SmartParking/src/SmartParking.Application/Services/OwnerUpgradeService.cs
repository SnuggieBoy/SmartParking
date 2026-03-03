using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.DTOs.Notification;
using SmartParking.Application.DTOs.Owner;
using SmartParking.Application.DTOs.ParkingLot;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;

namespace SmartParking.Application.Services;

/// <summary>
/// Handles the flow where a User requests to become an Owner (host).
/// </summary>
public sealed class OwnerUpgradeService : IOwnerUpgradeService
{
    private readonly IOwnerUpgradeRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IParkingLotService _parkingLotService;
    private readonly INotificationService _notificationService;
    private readonly OwnerSubscriptionSettings _settings;

    public OwnerUpgradeService(
        IOwnerUpgradeRequestRepository requestRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IParkingLotService parkingLotService,
        INotificationService notificationService,
        IOptions<OwnerSubscriptionSettings> settings)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _parkingLotService = parkingLotService;
        _notificationService = notificationService;
        _settings = settings.Value;
    }

    public Task<IEnumerable<OwnerPlanDto>> GetPlansAsync(CancellationToken ct = default)
    {
        var plans = new[]
        {
            new OwnerPlanDto(
                PlanType: "Monthly",
                MonthlyFee: 100000m,
                YearlyFee: 0m,
                Description: "Owner subscription billed monthly - 100,000 VND / month"),
            new OwnerPlanDto(
                PlanType: "Yearly",
                MonthlyFee: 0m,
                YearlyFee: 1000000m,
                Description: "Owner subscription billed yearly (recommended) - 1,000,000 VND / year")
        }.AsEnumerable();

        return Task.FromResult(plans);
    }

    public async Task<OwnerUpgradeRequestResponseDto> CreateRequestAsync(
        Guid userId,
        CreateOwnerUpgradeRequestDto request,
        CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException(Messages.Auth.UserNotFound);

        // Only basic User can request upgrade (not already Owner/Admin)
        if (user.Role.RoleName == AuthConstants.Roles.Owner ||
            user.Role.RoleName == AuthConstants.Roles.Admin)
        {
            throw new BadRequestException("You are already an owner or admin.");
        }

        if (string.IsNullOrWhiteSpace(request.ParkingLotName) ||
            string.IsNullOrWhiteSpace(request.ParkingLotAddress))
        {
            throw new BadRequestException("Parking lot name and address are required.");
        }

        if (!request.Latitude.HasValue || !request.Longitude.HasValue)
        {
            throw new BadRequestException("Vui lòng chọn vị trí bãi xe trên bản đồ để người dùng có thể tìm thấy bãi xe.");
        }

        var planType = request.PlanType?.Trim() ?? "Monthly";
        var fee = planType.Equals("Yearly", StringComparison.OrdinalIgnoreCase)
            ? 1000000m
            : 100000m;

        // PaymentTransactionId is optional: user creates request first (PendingPayment), then pays via VNPay/SePay.
        // After payment success, callback sets PaymentTransactionId and status → PendingApproval.
        var status = request.PaymentTransactionId.HasValue ? "Pending" : "PendingPayment";

        var entity = new OwnerUpgradeRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId,
            FullNameSnapshot = user.FullName ?? string.Empty,
            EmailSnapshot = user.Email ?? string.Empty,
            PhoneSnapshot = user.Phone,
            ParkingLotName = request.ParkingLotName.Trim(),
            ParkingLotAddress = request.ParkingLotAddress.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            PlanType = planType,
            FeeAmount = fee,
            Status = status,
            RejectReason = null,
            PaymentTransactionId = request.PaymentTransactionId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _requestRepository.CreateAsync(entity, ct);
        return MapToResponse(created);
    }

    public async Task<OwnerUpgradeRequestResponseDto?> GetMyLatestRequestAsync(Guid userId, CancellationToken ct = default)
    {
        var entity = await _requestRepository.GetLatestByUserIdAsync(userId, ct);
        return entity is null ? null : MapToResponse(entity);
    }

    public async Task<PagedResult<OwnerUpgradeRequestResponseDto>> GetAllAsync(
        OwnerUpgradeRequestFilterDto filter,
        CancellationToken ct = default)
    {
        var paged = await _requestRepository.GetAllAsync(
            filter.Status,
            filter.Page,
            filter.PageSize,
            ct);

        var items = paged.Items.Select(MapToResponse).ToList();
        return new PagedResult<OwnerUpgradeRequestResponseDto>(
            items,
            paged.Page,
            paged.PageSize,
            paged.TotalCount);
    }

    public async Task<OwnerUpgradeRequestResponseDto> ApproveAsync(
        Guid requestId,
        Guid adminId,
        CancellationToken ct = default)
    {
        var entity = await _requestRepository.GetByIdAsync(requestId, ct)
                     ?? throw new NotFoundException("Owner upgrade request not found.");

        var canApprove = string.Equals(entity.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(entity.Status, "PendingApproval", StringComparison.OrdinalIgnoreCase);
        if (!canApprove)
        {
            throw new BadRequestException("Only pending or pending approval requests can be approved.");
        }

        // Promote user to Owner role
        var user = await _userRepository.GetByIdAsync(entity.UserId, ct)
                   ?? throw new NotFoundException(Messages.Auth.UserNotFound);

        var ownerRole = await _roleRepository.GetByNameAsync(AuthConstants.Roles.Owner, ct)
                        ?? throw new NotFoundException("Owner role not configured.");

        user.RoleId = ownerRole.RoleId;
        await _userRepository.UpdateAsync(user, ct);

        // Create ParkingLot from the registration data (name, address, lat/lng, image)
        var createLotDto = new CreateParkingLotDto(
            Name: entity.ParkingLotName,
            Address: entity.ParkingLotAddress,
            TotalCapacity: 10,      // Default, owner can update later
            PricePerHour: 10000m,   // Default 10k VND/hour, owner can update later
            Latitude: entity.Latitude,
            Longitude: entity.Longitude,
            ImageUrl: entity.ImageUrl);
        _ = await _parkingLotService.CreateAsync(createLotDto, entity.UserId, isAdmin: true, ct);

        entity.Status = "Approved";
        entity.ApprovedAt = DateTime.UtcNow;
        entity.ProcessedBy = adminId;
        
        // Prevent EF Core tracking conflict (since we have another tracked User instance)
        entity.User = null!;

        await _requestRepository.UpdateAsync(entity, ct);

        // Gửi thông báo cho user biết yêu cầu đã được duyệt
        await _notificationService.SendNotificationAsync(
            new SendNotificationDto(
                UserId: entity.UserId,
                Title: "Yêu cầu đăng ký làm chủ bãi xe đã được duyệt",
                Message: "Chúc mừng! Yêu cầu đăng ký làm chủ bãi xe của bạn đã được phê duyệt. Bạn có thể đăng nhập và quản lý bãi xe của mình.",
                Type: "Success"),
            adminId,
            ct);

        return MapToResponse(entity);
    }

    public async Task<OwnerUpgradeRequestResponseDto> RejectAsync(
        Guid requestId,
        Guid adminId,
        RejectOwnerUpgradeRequestDto request,
        CancellationToken ct = default)
    {
        var entity = await _requestRepository.GetByIdAsync(requestId, ct)
                     ?? throw new NotFoundException("Owner upgrade request not found.");

        var canReject = string.Equals(entity.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(entity.Status, "PendingApproval", StringComparison.OrdinalIgnoreCase);
        if (!canReject)
        {
            throw new BadRequestException("Only pending or pending approval requests can be rejected.");
        }

        entity.Status = "Rejected";
        entity.RejectReason = string.IsNullOrWhiteSpace(request.Reason)
            ? null
            : request.Reason.Trim();
        entity.RejectedAt = DateTime.UtcNow;
        entity.ProcessedBy = adminId;

        await _requestRepository.UpdateAsync(entity, ct);

        // Gửi thông báo cho user biết yêu cầu đã bị từ chối kèm lý do
        var reasonText = string.IsNullOrWhiteSpace(entity.RejectReason)
            ? "Không có lý do cụ thể."
            : entity.RejectReason;
        await _notificationService.SendNotificationAsync(
            new SendNotificationDto(
                UserId: entity.UserId,
                Title: "Yêu cầu đăng ký làm chủ bãi xe đã bị từ chối",
                Message: $"Yêu cầu đăng ký làm chủ bãi xe của bạn đã bị từ chối. Lý do: {reasonText}",
                Type: "Error"),
            adminId,
            ct);

        return MapToResponse(entity);
    }

    private static OwnerUpgradeRequestResponseDto MapToResponse(OwnerUpgradeRequest entity)
    {
        return new OwnerUpgradeRequestResponseDto(
            RequestId: entity.RequestId,
            UserId: entity.UserId,
            FullName: entity.FullNameSnapshot,
            Email: entity.EmailSnapshot,
            Phone: entity.PhoneSnapshot,
            ParkingLotName: entity.ParkingLotName,
            ParkingLotAddress: entity.ParkingLotAddress,
            Latitude: entity.Latitude,
            Longitude: entity.Longitude,
            PlanType: entity.PlanType,
            FeeAmount: entity.FeeAmount,
            Status: entity.Status,
            RejectReason: entity.RejectReason,
            CreatedAt: entity.CreatedAt,
            ApprovedAt: entity.ApprovedAt,
            RejectedAt: entity.RejectedAt,
            PaymentTransactionId: entity.PaymentTransactionId);
    }
}

