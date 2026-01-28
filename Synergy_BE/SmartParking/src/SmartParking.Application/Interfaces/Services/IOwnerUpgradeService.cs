using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Owner;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for handling User -> Owner upgrade flow (P2P subscription).
/// </summary>
public interface IOwnerUpgradeService
{
    /// <summary>
    /// Get available owner subscription plans (monthly / yearly).
    /// </summary>
    Task<IEnumerable<OwnerPlanDto>> GetPlansAsync(CancellationToken ct = default);

    /// <summary>
    /// Create a new upgrade request for the current user.
    /// </summary>
    Task<OwnerUpgradeRequestResponseDto> CreateRequestAsync(
        Guid userId,
        CreateOwnerUpgradeRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Get the current user's latest upgrade request (if any).
    /// </summary>
    Task<OwnerUpgradeRequestResponseDto?> GetMyLatestRequestAsync(
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Admin: get paged list of owner upgrade requests.
    /// </summary>
    Task<PagedResult<OwnerUpgradeRequestResponseDto>> GetAllAsync(
        OwnerUpgradeRequestFilterDto filter,
        CancellationToken ct = default);

    /// <summary>
    /// Admin: approve an upgrade request and promote user to Owner.
    /// (Implementation will later ensure payment is valid.)
    /// </summary>
    Task<OwnerUpgradeRequestResponseDto> ApproveAsync(
        Guid requestId,
        Guid adminId,
        CancellationToken ct = default);

    /// <summary>
    /// Admin: reject an upgrade request with a reason.
    /// </summary>
    Task<OwnerUpgradeRequestResponseDto> RejectAsync(
        Guid requestId,
        Guid adminId,
        RejectOwnerUpgradeRequestDto request,
        CancellationToken ct = default);
}

