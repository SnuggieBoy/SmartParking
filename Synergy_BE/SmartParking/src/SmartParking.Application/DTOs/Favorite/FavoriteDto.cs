using System.ComponentModel.DataAnnotations;

namespace SmartParking.Application.DTOs.Favorite;

/// <summary>
/// Favorite parking lot response
/// </summary>
public sealed record FavoriteDto(
    Guid FavoriteId,
    Guid ParkingLotId,
    string ParkingLotName,
    string ParkingLotAddress,
    decimal PricePerHour,
    int TotalCapacity,
    int CurrentOccupancy,
    decimal? AverageRating,
    int ReviewCount,
    DateTime FavoritedAt
);

/// <summary>
/// Add favorite request
/// </summary>
public sealed record AddFavoriteDto(
    [Required(ErrorMessage = "Parking lot ID is required")]
    Guid ParkingLotId
);
