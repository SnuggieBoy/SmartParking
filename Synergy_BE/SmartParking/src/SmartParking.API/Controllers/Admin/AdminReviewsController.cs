using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Review;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers.Admin;

/// <summary>
/// Admin endpoints for review moderation.
/// SECURITY: Only Admin can access these endpoints.
/// </summary>
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/admin/reviews")]
public sealed class AdminReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;

    public AdminReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Get all reviews with filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReviewDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> GetAllReviews(
        [FromQuery] Guid? parkingLotId,
        [FromQuery] int? rating,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _reviewService.GetAllReviewsAsync(parkingLotId, rating, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<ReviewDto>>.SuccessResponse(result, "Reviews retrieved successfully"));
    }

    /// <summary>
    /// Delete review (moderation)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteReview(Guid id, CancellationToken ct = default)
    {
        var adminId = GetUserIdFromToken();
        await _reviewService.DeleteReviewByAdminAsync(id, adminId, ct);
        return Ok(ApiResponse.SuccessResponse("Review deleted successfully"));
    }
}
