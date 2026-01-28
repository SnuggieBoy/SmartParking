using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.API.Authorization.Policies;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Favorite;
using SmartParking.Application.Interfaces.Services;

namespace SmartParking.API.Controllers;

/// <summary>
/// User favorites management endpoints.
/// SECURITY: Only authenticated users can access.
/// </summary>
[Authorize]
[Route("api/parking-lots")]
public sealed class FavoritesController : BaseApiController
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    /// <summary>
    /// Get user's favorite parking lots
    /// </summary>
    [HttpGet("favorites")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FavoriteDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<FavoriteDto>>>> GetFavorites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var result = await _favoriteService.GetUserFavoritesAsync(userId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<FavoriteDto>>.SuccessResponse(result, "Favorites retrieved successfully"));
    }

    /// <summary>
    /// Add parking lot to favorites
    /// </summary>
    [HttpPost("{parkingLotId:guid}/favorite")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FavoriteDto>>> AddFavorite(
        Guid parkingLotId,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var favorite = await _favoriteService.AddFavoriteAsync(userId, parkingLotId, ct);
        return CreatedAtAction(
            nameof(GetFavorites),
            ApiResponse<FavoriteDto>.SuccessResponse(favorite, "Added to favorites"));
    }

    /// <summary>
    /// Remove parking lot from favorites
    /// </summary>
    [HttpDelete("{parkingLotId:guid}/favorite")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> RemoveFavorite(
        Guid parkingLotId,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        await _favoriteService.RemoveFavoriteAsync(userId, parkingLotId, ct);
        return Ok(ApiResponse.SuccessResponse("Removed from favorites"));
    }

    /// <summary>
    /// Check if parking lot is in favorites
    /// </summary>
    [HttpGet("{parkingLotId:guid}/favorite/check")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckFavorite(
        Guid parkingLotId,
        CancellationToken ct = default)
    {
        var userId = GetUserIdFromToken();
        var isFavorite = await _favoriteService.IsFavoriteAsync(userId, parkingLotId, ct);
        return Ok(ApiResponse<bool>.SuccessResponse(isFavorite, "Favorite status retrieved"));
    }
}
