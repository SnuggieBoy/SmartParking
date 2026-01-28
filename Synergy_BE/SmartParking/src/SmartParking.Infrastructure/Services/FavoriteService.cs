using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.Favorite;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Services;

public sealed class FavoriteService : IFavoriteService
{
    private readonly SmartParkingDBContext _context;

    public FavoriteService(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FavoriteDto>> GetUserFavoritesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Favorites
            .Include(f => f.ParkingLot)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = new List<FavoriteDto>();
        foreach (var f in items)
        {
            var reviewStats = await _context.Reviews
                .Where(r => r.ParkingLotId == f.ParkingLotId && !r.IsDeleted)
                .GroupBy(r => r.ParkingLotId)
                .Select(g => new { Avg = g.Average(r => r.Rating), Count = g.Count() })
                .FirstOrDefaultAsync(ct);

            dtos.Add(new FavoriteDto(
                FavoriteId: f.FavoriteId,
                ParkingLotId: f.ParkingLotId,
                ParkingLotName: f.ParkingLot?.Name ?? "",
                ParkingLotAddress: f.ParkingLot?.Address ?? "",
                PricePerHour: f.ParkingLot?.PricePerHour ?? 0,
                TotalCapacity: f.ParkingLot?.TotalCapacity ?? 0,
                CurrentOccupancy: f.ParkingLot?.CurrentOccupancy ?? 0,
                AverageRating: reviewStats != null ? (decimal?)reviewStats.Avg : null,
                ReviewCount: reviewStats?.Count ?? 0,
                FavoritedAt: f.CreatedAt
            ));
        }

        return new PagedResult<FavoriteDto>(dtos, page, pageSize, totalCount);
    }

    public async Task<FavoriteDto> AddFavoriteAsync(Guid userId, Guid parkingLotId, CancellationToken ct = default)
    {
        // Check if parking lot exists
        var parkingLot = await _context.ParkingLots
            .FirstOrDefaultAsync(p => p.ParkingLotId == parkingLotId && !p.IsDeleted, ct);

        if (parkingLot == null)
        {
            throw new NotFoundException("Parking lot not found");
        }

        // Check if already favorited
        var existing = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ParkingLotId == parkingLotId, ct);

        if (existing != null)
        {
            throw new BadRequestException("Parking lot is already in favorites");
        }

        var favorite = new Favorite
        {
            FavoriteId = Guid.NewGuid(),
            UserId = userId,
            ParkingLotId = parkingLotId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync(ct);

        var reviewStats = await _context.Reviews
            .Where(r => r.ParkingLotId == parkingLotId && !r.IsDeleted)
            .GroupBy(r => r.ParkingLotId)
            .Select(g => new { Avg = g.Average(r => r.Rating), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        return new FavoriteDto(
            FavoriteId: favorite.FavoriteId,
            ParkingLotId: parkingLot.ParkingLotId,
            ParkingLotName: parkingLot.Name,
            ParkingLotAddress: parkingLot.Address,
            PricePerHour: parkingLot.PricePerHour,
            TotalCapacity: parkingLot.TotalCapacity,
            CurrentOccupancy: parkingLot.CurrentOccupancy,
            AverageRating: reviewStats != null ? (decimal?)reviewStats.Avg : null,
            ReviewCount: reviewStats?.Count ?? 0,
            FavoritedAt: favorite.CreatedAt
        );
    }

    public async Task RemoveFavoriteAsync(Guid userId, Guid parkingLotId, CancellationToken ct = default)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ParkingLotId == parkingLotId, ct);

        if (favorite == null)
        {
            throw new NotFoundException("Favorite not found");
        }

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid parkingLotId, CancellationToken ct = default)
    {
        return await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.ParkingLotId == parkingLotId, ct);
    }
}
