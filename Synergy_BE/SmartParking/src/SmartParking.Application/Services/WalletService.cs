using Microsoft.Extensions.Options;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Models;
using SmartParking.Application.Common.Settings;
using SmartParking.Application.DTOs.Notification;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Constants;
using SmartParking.Domain.Entities;
using SmartParking.Domain.Enums;

namespace SmartParking.Application.Services;

public sealed class WalletService : IWalletService
{
    private readonly IUserWalletRepository _walletRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IOwnerUpgradeRequestRepository _ownerUpgradeRequestRepository;
    private readonly INotificationService _notificationService;
    private readonly CommissionSettings _commissionSettings;

    public WalletService(
        IUserWalletRepository walletRepository,
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository,
        IOwnerUpgradeRequestRepository ownerUpgradeRequestRepository,
        INotificationService notificationService,
        IOptions<CommissionSettings> commissionSettings)
    {
        _walletRepository = walletRepository;
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
        _ownerUpgradeRequestRepository = ownerUpgradeRequestRepository;
        _notificationService = notificationService;
        _commissionSettings = commissionSettings.Value;
    }

    public async Task<decimal> GetBalanceAsync(Guid userId, CancellationToken ct = default)
    {
        var wallet = await _walletRepository.GetOrCreateAsync(userId, ct);
        return wallet.Balance;
    }

    public async Task<object> TopUpAsync(Guid userId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0)
            throw new BadRequestException("Số tiền nạp phải lớn hơn 0");

        var wallet = await _walletRepository.GetOrCreateAsync(userId, ct);
        wallet.Balance += amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Type = "TopUp",
            BalanceAfter = wallet.Balance,
            Description = "Nạp tiền ví (Giả lập)",
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(wallet, ct);
        await _walletRepository.AddTransactionAsync(transaction, ct);

        // Thông báo nạp ví thành công
        await _notificationService.SendNotificationAsync(
            new SendNotificationDto(UserId: userId, Title: "Nạp ví thành công", Message: $"Bạn đã nạp {amount:N0}đ vào ví. Số dư hiện tại: {wallet.Balance:N0}đ.", Type: "Success"),
            null, ct);

        return new { balance = wallet.Balance, transaction = new { id = transaction.WalletTransactionId, amount, balanceAfter = wallet.Balance, type = "TopUp", createdAt = transaction.CreatedAt } };
    }

    public async Task CreditWalletFromPaymentAsync(Guid userId, decimal amount, string description, CancellationToken ct = default)
    {
        if (amount <= 0) return;
        var wallet = await _walletRepository.GetOrCreateAsync(userId, ct);
        wallet.Balance += amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Type = "TopUp",
            BalanceAfter = wallet.Balance,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(wallet, ct);
        await _walletRepository.AddTransactionAsync(transaction, ct);

        // Thông báo nạp ví thành công (qua SePay/ngân hàng)
        await _notificationService.SendNotificationAsync(
            new SendNotificationDto(UserId: userId, Title: "Nạp ví thành công", Message: $"Bạn đã nạp {amount:N0}đ vào ví qua chuyển khoản. Số dư hiện tại: {wallet.Balance:N0}đ.", Type: "Success"),
            null, ct);
    }

    public async Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var paged = await _walletRepository.GetTransactionsAsync(userId, page, pageSize, ct);
        var dtos = paged.Items.Select(t => new WalletTransactionDto(
            t.WalletTransactionId.ToString(),
            t.Type,
            t.Amount,
            t.BalanceAfter,
            t.Description,
            t.CreatedAt
        )).ToList();

        return new PagedResult<WalletTransactionDto>(dtos, paged.Page, paged.PageSize, paged.TotalCount);
    }

    public async Task<PayWithWalletResultDto> PayWithWalletAsync(Guid userId, Guid bookingId, decimal amount, CancellationToken ct = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
            return new PayWithWalletResultDto(false, "Booking không tồn tại", null);

        if (booking.UserId != userId)
            return new PayWithWalletResultDto(false, "Bạn không có quyền thanh toán booking này", null);

        var existingPayment = await _paymentRepository.GetLatestByBookingIdAsync(bookingId, ct);
        if (existingPayment?.PaymentStatus == nameof(PaymentStatus.Success))
            return new PayWithWalletResultDto(false, "Booking đã được thanh toán", null);

        var wallet = await _walletRepository.GetOrCreateAsync(userId, ct);
        if (wallet.Balance < amount)
            return new PayWithWalletResultDto(false, "Số dư không đủ", null);

        wallet.Balance -= amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var payment = new PaymentTransaction
        {
            PaymentId = Guid.NewGuid(),
            BookingId = bookingId,
            UserId = userId,
            Amount = amount,
            PaymentMethod = PaymentConstants.WalletProvider,
            PaymentStatus = nameof(PaymentStatus.Success),
            PaymentType = "Booking",
            VnpTxnRef = $"WALLET{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
            CreatedAt = DateTime.UtcNow
        };

        var walletTrans = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = userId,
            Amount = -amount,
            Type = "BookingPayment",
            BalanceAfter = wallet.Balance,
            BookingId = bookingId,
            PaymentTransactionId = payment.PaymentId,
            Description = $"Thanh toán booking #{bookingId:N}",
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(wallet, ct);
        await _walletRepository.AddTransactionAsync(walletTrans, ct);
        await _paymentRepository.CreateAsync(payment, ct);

        // Chuyển tiền sang ví owner ngay khi thanh toán thành công
        var ownerId = booking.ParkingLot?.OwnerId ?? Guid.Empty;
        if (ownerId != Guid.Empty && amount > 0)
        {
            await TransferBookingToOwnerAsync(bookingId, amount, ownerId, ct);
        }

        // Thông báo thanh toán thành công
        await _notificationService.CreatePaymentNotificationAsync(
            userId, "Thanh toán thành công", $"Bạn đã thanh toán {amount:N0}đ cho đặt chỗ tại bãi xe {booking.ParkingLot?.Name ?? "bãi xe"}.", payment.PaymentId, ct);

        return new PayWithWalletResultDto(true, "Thanh toán thành công", wallet.Balance);
    }

    public async Task<PayWithWalletResultDto> PayExtensionWithWalletAsync(Guid userId, Guid bookingId, decimal extensionAmount, CancellationToken ct = default)
    {
        if (extensionAmount <= 0)
            return new PayWithWalletResultDto(false, "Số tiền gia hạn không hợp lệ", null);

        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        if (booking == null)
            return new PayWithWalletResultDto(false, "Booking không tồn tại", null);

        if (booking.UserId != userId)
            return new PayWithWalletResultDto(false, "Bạn không có quyền thanh toán gia hạn này", null);

        var wallet = await _walletRepository.GetOrCreateAsync(userId, ct);
        if (wallet.Balance < extensionAmount)
            return new PayWithWalletResultDto(false, "Số dư không đủ để thanh toán gia hạn", null);

        wallet.Balance -= extensionAmount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var payment = new PaymentTransaction
        {
            PaymentId = Guid.NewGuid(),
            BookingId = bookingId,
            UserId = userId,
            Amount = extensionAmount,
            PaymentMethod = PaymentConstants.WalletProvider,
            PaymentStatus = nameof(PaymentStatus.Success),
            PaymentType = "Extension",
            VnpTxnRef = $"EXT{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
            CreatedAt = DateTime.UtcNow
        };

        var walletTrans = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = userId,
            Amount = -extensionAmount,
            Type = "ExtensionPayment",
            BalanceAfter = wallet.Balance,
            BookingId = bookingId,
            PaymentTransactionId = payment.PaymentId,
            Description = $"Thanh toán gia hạn #{bookingId:N}",
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(wallet, ct);
        await _walletRepository.AddTransactionAsync(walletTrans, ct);
        await _paymentRepository.CreateAsync(payment, ct);

        var ownerId = booking.ParkingLot?.OwnerId ?? Guid.Empty;
        if (ownerId != Guid.Empty && extensionAmount > 0)
        {
            var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
            var commissionAmount = Math.Round(extensionAmount * commissionRate, 0);
            var ownerAmount = extensionAmount - commissionAmount;

            var ownerWallet = await _walletRepository.GetOrCreateAsync(ownerId, ct);
            ownerWallet.Balance += ownerAmount;
            ownerWallet.UpdatedAt = DateTime.UtcNow;

            var ownerTrans = new WalletTransaction
            {
                WalletTransactionId = Guid.NewGuid(),
                UserId = ownerId,
                Amount = ownerAmount,
                Type = "BookingIncome",
                BalanceAfter = ownerWallet.Balance,
                BookingId = bookingId,
                Description = $"Thu tiền gia hạn #{bookingId:N} (sau phí {_commissionSettings.CommissionRatePercent}%)",
                CreatedAt = DateTime.UtcNow
            };

            await _walletRepository.UpdateAsync(ownerWallet, ct);
            await _walletRepository.AddTransactionAsync(ownerTrans, ct);
            await CreditAdminCommissionAsync(commissionAmount, bookingId, $"Hoa hồng {_commissionSettings.CommissionRatePercent}% gia hạn #{bookingId:N}", ct);
        }

        await _notificationService.CreatePaymentNotificationAsync(
            userId, "Thanh toán gia hạn thành công", $"Bạn đã thanh toán {extensionAmount:N0}đ cho gia hạn đặt chỗ.", payment.PaymentId, ct);

        return new PayWithWalletResultDto(true, "Thanh toán gia hạn thành công", wallet.Balance);
    }

    public async Task<PayWithWalletResultDto> PayOwnerUpgradeFromWalletAsync(Guid userId, Guid ownerUpgradeRequestId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0)
            return new PayWithWalletResultDto(false, "Số tiền không hợp lệ", null);

        var upgradeRequest = await _ownerUpgradeRequestRepository.GetByIdAsync(ownerUpgradeRequestId, ct);
        if (upgradeRequest == null)
            return new PayWithWalletResultDto(false, "Yêu cầu đăng ký owner không tồn tại", null);

        if (upgradeRequest.UserId != userId)
            return new PayWithWalletResultDto(false, "Bạn không có quyền thanh toán yêu cầu này", null);

        if (upgradeRequest.PaymentTransactionId.HasValue)
            return new PayWithWalletResultDto(false, "Yêu cầu đã được thanh toán", null);

        var wallet = await _walletRepository.GetOrCreateAsync(userId, ct);
        if (wallet.Balance < amount)
            return new PayWithWalletResultDto(false, "Số dư không đủ", null);

        wallet.Balance -= amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var payment = new PaymentTransaction
        {
            PaymentId = Guid.NewGuid(),
            OwnerUpgradeRequestId = ownerUpgradeRequestId,
            UserId = userId,
            Amount = amount,
            PaymentMethod = PaymentConstants.WalletProvider,
            PaymentStatus = nameof(PaymentStatus.Success),
            PaymentType = "Subscription",
            VnpTxnRef = $"SUB{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
            CreatedAt = DateTime.UtcNow
        };

        var walletTrans = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = userId,
            Amount = -amount,
            Type = "OwnerUpgradePayment",
            BalanceAfter = wallet.Balance,
            PaymentTransactionId = payment.PaymentId,
            Description = $"Thanh toán phí đăng ký owner #{ownerUpgradeRequestId:N}",
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(wallet, ct);
        await _walletRepository.AddTransactionAsync(walletTrans, ct);
        await _paymentRepository.CreateAsync(payment, ct);
        await _ownerUpgradeRequestRepository.UpdatePaymentAsync(ownerUpgradeRequestId, payment.PaymentId, ct);

        await CreditAdminCommissionAsync(amount, null, $"Phí đăng ký owner #{ownerUpgradeRequestId:N}", ct);

        await _notificationService.SendNotificationAsync(
            new SendNotificationDto(UserId: userId, Title: "Thanh toán phí đăng ký thành công", Message: $"Bạn đã thanh toán {amount:N0}đ cho phí đăng ký làm chủ bãi xe. Yêu cầu đang chờ Admin duyệt.", Type: "Success"),
            null, ct);

        return new PayWithWalletResultDto(true, "Thanh toán phí đăng ký thành công", wallet.Balance);
    }

    /// <summary>Chuyển tiền booking sang ví owner (trừ commission) và ví admin (commission).</summary>
    public async Task<bool> TransferBookingToOwnerAsync(Guid bookingId, decimal amount, Guid ownerId, CancellationToken ct = default)
    {
        if (amount <= 0) return false;

        if (await _walletRepository.HasBookingIncomeForBookingAsync(bookingId, ct))
            return true;

        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
        var commissionAmount = Math.Round(amount * commissionRate, 0);
        var ownerAmount = amount - commissionAmount;

        var ownerWallet = await _walletRepository.GetOrCreateAsync(ownerId, ct);
        ownerWallet.Balance += ownerAmount;
        ownerWallet.UpdatedAt = DateTime.UtcNow;

        var ownerTrans = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = ownerId,
            Amount = ownerAmount,
            Type = "BookingIncome",
            BalanceAfter = ownerWallet.Balance,
            BookingId = bookingId,
            Description = $"Thu tiền booking #{bookingId:N} (sau phí {_commissionSettings.CommissionRatePercent}%)",
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(ownerWallet, ct);
        await _walletRepository.AddTransactionAsync(ownerTrans, ct);

        await CreditAdminCommissionAsync(commissionAmount, bookingId, $"Hoa hồng {_commissionSettings.CommissionRatePercent}% booking #{bookingId:N}", ct);

        return true;
    }

    /// <summary>Cộng tiền hoa hồng/phí vào ví Admin.</summary>
    private async Task CreditAdminCommissionAsync(decimal amount, Guid? bookingId, string description, CancellationToken ct)
    {
        if (amount <= 0) return;
        var adminId = await _walletRepository.GetFirstAdminUserIdAsync(ct);
        if (adminId == null) return;

        var adminWallet = await _walletRepository.GetOrCreateAsync(adminId.Value, ct);
        adminWallet.Balance += amount;
        adminWallet.UpdatedAt = DateTime.UtcNow;

        var trans = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = adminId.Value,
            Amount = amount,
            Type = "PlatformCommission",
            BalanceAfter = adminWallet.Balance,
            BookingId = bookingId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(adminWallet, ct);
        await _walletRepository.AddTransactionAsync(trans, ct);
    }

    /// <summary>Hoàn 70% thời gian chưa dùng khi checkout sớm: trừ Owner (phần 90%) + Admin (phần 10%), cộng User.</summary>
    public async Task<bool> RefundEarlyCheckoutAsync(Guid bookingId, decimal refundAmount, Guid userId, Guid ownerId, CancellationToken ct = default)
    {
        if (refundAmount <= 0) return false;

        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
        var adminShare = Math.Round(refundAmount * commissionRate, 0);
        var ownerShare = refundAmount - adminShare;

        var ownerWallet = await _walletRepository.GetOrCreateAsync(ownerId, ct);
        ownerWallet.Balance -= ownerShare;
        ownerWallet.UpdatedAt = DateTime.UtcNow;

        var ownerTrans = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = ownerId,
            Amount = -ownerShare,
            Type = "EarlyCheckoutRefund",
            BalanceAfter = ownerWallet.Balance,
            BookingId = bookingId,
            Description = $"Hoàn tiền checkout sớm #{bookingId:N} (phần owner)",
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(ownerWallet, ct);
        await _walletRepository.AddTransactionAsync(ownerTrans, ct);

        if (adminShare > 0)
        {
            var adminId = await _walletRepository.GetFirstAdminUserIdAsync(ct);
            if (adminId != null)
            {
                var adminWallet = await _walletRepository.GetOrCreateAsync(adminId.Value, ct);
                adminWallet.Balance -= adminShare;
                adminWallet.UpdatedAt = DateTime.UtcNow;

                var adminTrans = new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    UserId = adminId.Value,
                    Amount = -adminShare,
                    Type = "CommissionRefund",
                    BalanceAfter = adminWallet.Balance,
                    BookingId = bookingId,
                    Description = $"Hoàn hoa hồng checkout sớm #{bookingId:N}",
                    CreatedAt = DateTime.UtcNow
                };

                await _walletRepository.UpdateAsync(adminWallet, ct);
                await _walletRepository.AddTransactionAsync(adminTrans, ct);
            }
        }

        var userWallet = await _walletRepository.GetOrCreateAsync(userId, ct);
        userWallet.Balance += refundAmount;
        userWallet.UpdatedAt = DateTime.UtcNow;

        var userTrans = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = userId,
            Amount = refundAmount,
            Type = "Refund",
            BalanceAfter = userWallet.Balance,
            BookingId = bookingId,
            Description = $"Hoàn 70% thời gian chưa dùng #{bookingId:N}",
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(userWallet, ct);
        await _walletRepository.AddTransactionAsync(userTrans, ct);

        await _notificationService.SendNotificationAsync(
            new SendNotificationDto(UserId: userId, Title: "Hoàn tiền checkout sớm", Message: $"Bạn được hoàn {refundAmount:N0}đ (70% thời gian chưa dùng).", Type: "Success"),
            null, ct);

        return true;
    }

    /// <summary>
    /// Hoàn tiền booking đầy đủ: reverse Owner income + Admin commission → User.
    /// Dùng khi User hủy, Owner reject, hoặc Admin refund.
    /// </summary>
    public async Task<bool> RefundBookingFullAsync(Guid bookingId, Guid userId, decimal refundAmount, string reason, CancellationToken ct = default)
    {
        if (refundAmount <= 0) return false;

        var commissionRate = _commissionSettings.CommissionRatePercent / 100m;
        var adminShare = Math.Round(refundAmount * commissionRate, 0);
        var ownerShare = refundAmount - adminShare;

        var booking = await _bookingRepository.GetByIdAsync(bookingId, includeDeleted: false, ct);
        var ownerId = booking?.ParkingLot?.OwnerId ?? Guid.Empty;

        if (ownerId != Guid.Empty)
        {
            var hasIncome = await _walletRepository.HasBookingIncomeForBookingAsync(bookingId, ct);
            if (hasIncome)
            {
                var ownerWallet = await _walletRepository.GetOrCreateAsync(ownerId, ct);
                ownerWallet.Balance -= ownerShare;
                ownerWallet.UpdatedAt = DateTime.UtcNow;

                var ownerTrans = new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    UserId = ownerId,
                    Amount = -ownerShare,
                    Type = "BookingRefund",
                    BalanceAfter = ownerWallet.Balance,
                    BookingId = bookingId,
                    Description = $"Hoàn tiền booking #{bookingId:N}: {reason}",
                    CreatedAt = DateTime.UtcNow
                };

                await _walletRepository.UpdateAsync(ownerWallet, ct);
                await _walletRepository.AddTransactionAsync(ownerTrans, ct);
            }
        }

        if (adminShare > 0)
        {
            var adminId = await _walletRepository.GetFirstAdminUserIdAsync(ct);
            if (adminId != null)
            {
                var adminWallet = await _walletRepository.GetOrCreateAsync(adminId.Value, ct);
                adminWallet.Balance -= adminShare;
                adminWallet.UpdatedAt = DateTime.UtcNow;

                var adminTrans = new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    UserId = adminId.Value,
                    Amount = -adminShare,
                    Type = "CommissionRefund",
                    BalanceAfter = adminWallet.Balance,
                    BookingId = bookingId,
                    Description = $"Hoàn hoa hồng booking #{bookingId:N}: {reason}",
                    CreatedAt = DateTime.UtcNow
                };

                await _walletRepository.UpdateAsync(adminWallet, ct);
                await _walletRepository.AddTransactionAsync(adminTrans, ct);
            }
        }

        var userWallet = await _walletRepository.GetOrCreateAsync(userId, ct);
        userWallet.Balance += refundAmount;
        userWallet.UpdatedAt = DateTime.UtcNow;

        var userTrans = new WalletTransaction
        {
            WalletTransactionId = Guid.NewGuid(),
            UserId = userId,
            Amount = refundAmount,
            Type = "Refund",
            BalanceAfter = userWallet.Balance,
            BookingId = bookingId,
            Description = $"Hoàn tiền: {reason}",
            CreatedAt = DateTime.UtcNow
        };

        await _walletRepository.UpdateAsync(userWallet, ct);
        await _walletRepository.AddTransactionAsync(userTrans, ct);

        await _notificationService.SendNotificationAsync(
            new SendNotificationDto(UserId: userId, Title: "Hoàn tiền booking", Message: $"Bạn được hoàn {refundAmount:N0}đ. Lý do: {reason}", Type: "Success"),
            null, ct);

        return true;
    }
}
