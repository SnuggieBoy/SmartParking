using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Review;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers;

/// <summary>
/// Parking lot reviews endpoints.
/// </summary>
[Route("api/parking-lots")]
public sealed class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Get reviews for a parking lot (public)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{parkingLotId:guid}/reviews")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReviewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetParkingLotReviews(
        Guid parkingLotId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetParkingLotReviewsAsync(parkingLotId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ReviewDto>>.SuccessResponse(result, "Reviews retrieved successfully"));
    }

    /// <summary>
    /// Get reviews summary for a parking lot (public)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{parkingLotId:guid}/reviews/summary")]
    [ProducesResponseType(typeof(ApiResponse<ParkingLotReviewsSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ParkingLotReviewsSummaryDto>>> GetParkingLotReviewsSummary(
        Guid parkingLotId,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetParkingLotReviewsSummaryAsync(parkingLotId, ct);
        return Ok(ApiResponse<ParkingLotReviewsSummaryDto>.SuccessResponse(result, "Reviews summary retrieved successfully"));
    }

    /// <summary>
    /// Create a review for a completed booking
    /// </summary>
    [Authorize]
    [HttpPost("{parkingLotId:guid}/reviews")]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateReview(
        Guid parkingLotId,
        [FromBody] CreateReviewDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var review = await _reviewService.CreateReviewAsync(userId, request, ct);
        return CreatedAtAction(
            nameof(GetParkingLotReviews),
            new { parkingLotId },
            ApiResponse<ReviewDto>.SuccessResponse(review, "Review created successfully"));
    }

    /// <summary>
    /// Update user's own review
    /// </summary>
    [Authorize]
    [HttpPut("reviews/{reviewId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ReviewDto>>> UpdateReview(
        Guid reviewId,
        [FromBody] UpdateReviewDto request,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var review = await _reviewService.UpdateReviewAsync(reviewId, userId, request, ct);
        return Ok(ApiResponse<ReviewDto>.SuccessResponse(review, "Review updated successfully"));
    }

    /// <summary>
    /// Delete user's own review
    /// </summary>
    [Authorize]
    [HttpDelete("reviews/{reviewId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteReview(
        Guid reviewId,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        await _reviewService.DeleteReviewAsync(reviewId, userId, ct);
        return Ok(ApiResponse.SuccessResponse("Review deleted successfully"));
    }

    /// <summary>
    /// Get current user's reviews
    /// </summary>
    [Authorize]
    [HttpGet("~/api/users/reviews")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReviewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetMyReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _reviewService.GetUserReviewsAsync(userId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ReviewDto>>.SuccessResponse(result, "Reviews retrieved successfully"));
    }
}
