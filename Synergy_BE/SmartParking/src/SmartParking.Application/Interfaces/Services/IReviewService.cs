using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Review;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for managing parking lot reviews
/// </summary>
public interface IReviewService
{
    /// <summary>
    /// Get reviews for a parking lot
    /// </summary>
    Task<PagedResult<ReviewDto>> GetParkingLotReviewsAsync(Guid parkingLotId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Get reviews summary for a parking lot
    /// </summary>
    Task<ParkingLotReviewsSummaryDto> GetParkingLotReviewsSummaryAsync(Guid parkingLotId, CancellationToken ct = default);

    /// <summary>
    /// Create a review for a completed booking
    /// </summary>
    Task<ReviewDto> CreateReviewAsync(Guid userId, CreateReviewDto request, CancellationToken ct = default);

    /// <summary>
    /// Update user's own review
    /// </summary>
    Task<ReviewDto> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewDto request, CancellationToken ct = default);

    /// <summary>
    /// Delete user's own review
    /// </summary>
    Task DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Get user's reviews
    /// </summary>
    Task<PagedResult<ReviewDto>> GetUserReviewsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    #region Admin Operations

    /// <summary>
    /// Get all reviews (admin)
    /// </summary>
    Task<PagedResult<ReviewDto>> GetAllReviewsAsync(Guid? parkingLotId, int? rating, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Delete review (admin moderation)
    /// </summary>
    Task DeleteReviewByAdminAsync(Guid reviewId, Guid adminId, CancellationToken ct = default);

    #endregion
}
