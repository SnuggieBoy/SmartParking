using System.ComponentModel.DataAnnotations;

namespace SmartParking.Application.DTOs.Review;

/// <summary>
/// Review response DTO
/// </summary>
public sealed record ReviewDto(
    Guid ReviewId,
    Guid UserId,
    string UserName,
    Guid ParkingLotId,
    string ParkingLotName,
    Guid BookingId,
    int Rating,
    string? Comment,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Create review request
/// </summary>
public sealed record CreateReviewDto(
    [Required(ErrorMessage = "Booking ID is required")]
    Guid BookingId,

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    int Rating,

    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    string? Comment
);

/// <summary>
/// Update review request
/// </summary>
public sealed record UpdateReviewDto(
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    int? Rating,

    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    string? Comment
);

/// <summary>
/// Parking lot reviews summary
/// </summary>
public sealed record ParkingLotReviewsSummaryDto(
    Guid ParkingLotId,
    string ParkingLotName,
    decimal AverageRating,
    int TotalReviews,
    int FiveStarCount,
    int FourStarCount,
    int ThreeStarCount,
    int TwoStarCount,
    int OneStarCount,
    IEnumerable<ReviewDto> RecentReviews
);
