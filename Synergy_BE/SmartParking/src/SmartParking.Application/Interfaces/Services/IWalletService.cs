using SmartParking.Application.Common.Models;

namespace SmartParking.Application.Interfaces.Services;

public interface IWalletService
{
    Task<decimal> GetBalanceAsync(Guid userId, CancellationToken ct = default);
    Task<object> TopUpAsync(Guid userId, decimal amount, CancellationToken ct = default);
    Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<PayWithWalletResultDto> PayWithWalletAsync(Guid userId, Guid bookingId, decimal amount, CancellationToken ct = default);
    Task<PayWithWalletResultDto> PayExtensionWithWalletAsync(Guid userId, Guid bookingId, decimal extensionAmount, CancellationToken ct = default);
    Task<PayWithWalletResultDto> PayOwnerUpgradeFromWalletAsync(Guid userId, Guid ownerUpgradeRequestId, decimal amount, CancellationToken ct = default);

    /// <summary>Chuyển tiền booking sang ví owner khi owner duyệt.</summary>
    Task<bool> TransferBookingToOwnerAsync(Guid bookingId, decimal amount, Guid ownerId, CancellationToken ct = default);

    /// <summary>Hoàn tiền 70% thời gian chưa dùng khi checkout sớm.</summary>
    Task<bool> RefundEarlyCheckoutAsync(Guid bookingId, decimal refundAmount, Guid userId, Guid ownerId, CancellationToken ct = default);
}

public record WalletTransactionDto(
    string Id,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string? Description,
    DateTime CreatedAt
);

public record PayWithWalletResultDto(bool Success, string? Message, decimal? NewBalance);
