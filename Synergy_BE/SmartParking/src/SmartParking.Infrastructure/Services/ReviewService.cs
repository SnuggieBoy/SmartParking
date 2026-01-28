using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Review;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Services;

public sealed class ReviewService : IReviewService
{
    private readonly SmartParkingDBContext _context;

    public ReviewService(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ReviewDto>> GetParkingLotReviewsAsync(
        Guid parkingLotId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.ParkingLot)
            .Where(r => r.ParkingLotId == parkingLotId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<ReviewDto>(dtos, page, pageSize, totalCount);
    }

    public async Task<ParkingLotReviewsSummaryDto> GetParkingLotReviewsSummaryAsync(
        Guid parkingLotId,
        CancellationToken ct = default)
    {
        var parkingLot = await _context.ParkingLots
            .FirstOrDefaultAsync(p => p.ParkingLotId == parkingLotId && !p.IsDeleted, ct);

        if (parkingLot == null)
        {
            throw new NotFoundException("Parking lot not found");
        }

        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ParkingLotId == parkingLotId && !r.IsDeleted)
            .ToListAsync(ct);

        var avgRating = reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;

        return new ParkingLotReviewsSummaryDto(
            ParkingLotId: parkingLotId,
            ParkingLotName: parkingLot.Name,
            AverageRating: Math.Round(avgRating, 1),
            TotalReviews: reviews.Count,
            FiveStarCount: reviews.Count(r => r.Rating == 5),
            FourStarCount: reviews.Count(r => r.Rating == 4),
            ThreeStarCount: reviews.Count(r => r.Rating == 3),
            TwoStarCount: reviews.Count(r => r.Rating == 2),
            OneStarCount: reviews.Count(r => r.Rating == 1),
            RecentReviews: reviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(MapToDto)
                .ToList()
        );
    }

    public async Task<ReviewDto> CreateReviewAsync(
        Guid userId,
        CreateReviewDto request,
        CancellationToken ct = default)
    {
        // Get the booking
        var booking = await _context.Bookings
            .Include(b => b.ParkingLot)
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId && !b.IsDeleted, ct);

        if (booking == null)
        {
            throw new NotFoundException("Booking not found");
        }

        // Verify ownership
        if (booking.UserId != userId)
        {
            throw new ForbiddenException();
        }

        // Only completed bookings can be reviewed
        if (booking.Status != "Completed")
        {
            throw new BadRequestException("Can only review completed bookings");
        }

        // Check if already reviewed
        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.BookingId == request.BookingId && !r.IsDeleted, ct);

        if (existingReview != null)
        {
            throw new BadRequestException("This booking has already been reviewed");
        }

        var review = new Review
        {
            ReviewId = Guid.NewGuid(),
            UserId = userId,
            ParkingLotId = booking.ParkingLotId,
            BookingId = request.BookingId,
            Rating = request.Rating,
            Comment = request.Comment?.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(ct);

        // Reload with navigation properties
        review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.ParkingLot)
            .FirstOrDefaultAsync(r => r.ReviewId == review.ReviewId, ct);

        return MapToDto(review!);
    }

    public async Task<ReviewDto> UpdateReviewAsync(
        Guid reviewId,
        Guid userId,
        UpdateReviewDto request,
        CancellationToken ct = default)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.ParkingLot)
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && !r.IsDeleted, ct);

        if (review == null)
        {
            throw new NotFoundException("Review not found");
        }

        if (review.UserId != userId)
        {
            throw new ForbiddenException();
        }

        if (request.Rating.HasValue)
        {
            review.Rating = request.Rating.Value;
        }

        if (request.Comment != null)
        {
            review.Comment = request.Comment.Trim();
        }

        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return MapToDto(review);
    }

    public async Task DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken ct = default)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && !r.IsDeleted, ct);

        if (review == null)
        {
            throw new NotFoundException("Review not found");
        }

        if (review.UserId != userId)
        {
            throw new ForbiddenException();
        }

        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;
        review.DeletedBy = userId;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<ReviewDto>> GetUserReviewsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.ParkingLot)
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<ReviewDto>(dtos, page, pageSize, totalCount);
    }

    #region Admin Operations

    public async Task<PagedResult<ReviewDto>> GetAllReviewsAsync(
        Guid? parkingLotId,
        int? rating,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.ParkingLot)
            .Where(r => !r.IsDeleted)
            .AsQueryable();

        if (parkingLotId.HasValue)
        {
            query = query.Where(r => r.ParkingLotId == parkingLotId.Value);
        }

        if (rating.HasValue)
        {
            query = query.Where(r => r.Rating == rating.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<ReviewDto>(dtos, page, pageSize, totalCount);
    }

    public async Task DeleteReviewByAdminAsync(Guid reviewId, Guid adminId, CancellationToken ct = default)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && !r.IsDeleted, ct);

        if (review == null)
        {
            throw new NotFoundException("Review not found");
        }

        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;
        review.DeletedBy = adminId;
        await _context.SaveChangesAsync(ct);
    }

    #endregion

    private static ReviewDto MapToDto(Review r)
    {
        return new ReviewDto(
            ReviewId: r.ReviewId,
            UserId: r.UserId,
            UserName: r.User?.FullName ?? "Anonymous",
            ParkingLotId: r.ParkingLotId,
            ParkingLotName: r.ParkingLot?.Name ?? "",
            BookingId: r.BookingId,
            Rating: r.Rating,
            Comment: r.Comment,
            CreatedAt: r.CreatedAt,
            UpdatedAt: r.UpdatedAt
        );
    }
}
