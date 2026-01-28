using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Favorite;

namespace SmartParking.Application.Interfaces.Services;

/// <summary>
/// Service for managing user's favorite parking lots
/// </summary>
public interface IFavoriteService
{
    /// <summary>
    /// Get user's favorite parking lots
    /// </summary>
    Task<PagedResult<FavoriteDto>> GetUserFavoritesAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Add parking lot to favorites
    /// </summary>
    Task<FavoriteDto> AddFavoriteAsync(Guid userId, Guid parkingLotId, CancellationToken ct = default);

    /// <summary>
    /// Remove parking lot from favorites
    /// </summary>
    Task RemoveFavoriteAsync(Guid userId, Guid parkingLotId, CancellationToken ct = default);

    /// <summary>
    /// Check if parking lot is in user's favorites
    /// </summary>
    Task<bool> IsFavoriteAsync(Guid userId, Guid parkingLotId, CancellationToken ct = default);
}
