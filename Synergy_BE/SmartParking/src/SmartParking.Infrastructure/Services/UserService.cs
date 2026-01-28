using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Common.Exceptions;
using SmartParking.Application.Common.Models;
using SmartParking.Application.DTOs.User;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Application.Interfaces.Services;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly SmartParkingDBContext _context;
    private readonly IUserRepository _userRepository;

    public UserService(SmartParkingDBContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserResponseDto>> GetAllAsync(UserFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .AsQueryable();

        // Search by name, email, phone
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(searchTerm)) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                (u.Phone != null && u.Phone.Contains(searchTerm)));
        }

        // Filter by role
        if (!string.IsNullOrWhiteSpace(filter.RoleName))
        {
            query = query.Where(u => u.Role.RoleName == filter.RoleName);
        }

        // Filter by IsActive
        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        // Filter by EmailConfirmed
        if (filter.EmailConfirmed.HasValue)
        {
            query = query.Where(u => u.EmailConfirmed == filter.EmailConfirmed.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        // Map to DTOs with additional stats
        var dtos = new List<UserResponseDto>();
        foreach (var user in users)
        {
            var totalBookings = await _context.Bookings
                .Where(b => b.UserId == user.UserId && !b.IsDeleted)
                .CountAsync(ct);

            var totalParkingLots = await _context.ParkingLots
                .Where(p => p.OwnerId == user.UserId && !p.IsDeleted)
                .CountAsync(ct);

            dtos.Add(new UserResponseDto(
                UserId: user.UserId,
                FullName: user.FullName ?? "Unknown",
                Email: user.Email ?? "",
                Phone: user.Phone,
                RoleName: user.Role?.RoleName ?? "User",
                IsActive: user.IsActive,
                EmailConfirmed: user.EmailConfirmed,
                CreatedAt: user.CreatedAt,
                TotalBookings: totalBookings,
                TotalParkingLots: totalParkingLots
            ));
        }

        return new PagedResult<UserResponseDto>(dtos, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<UserResponseDto> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var totalBookings = await _context.Bookings
            .Where(b => b.UserId == user.UserId && !b.IsDeleted)
            .CountAsync(ct);

        var totalParkingLots = await _context.ParkingLots
            .Where(p => p.OwnerId == user.UserId && !p.IsDeleted)
            .CountAsync(ct);

        return new UserResponseDto(
            UserId: user.UserId,
            FullName: user.FullName ?? "Unknown",
            Email: user.Email ?? "",
            Phone: user.Phone,
            RoleName: user.Role?.RoleName ?? "User",
            IsActive: user.IsActive,
            EmailConfirmed: user.EmailConfirmed,
            CreatedAt: user.CreatedAt,
            TotalBookings: totalBookings,
            TotalParkingLots: totalParkingLots
        );
    }

    public async Task<UserResponseDto> UpdateAsync(Guid userId, UpdateUserDto request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName;
        }

        if (request.Phone != null)
        {
            user.Phone = request.Phone;
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        if (request.RoleId.HasValue)
        {
            user.RoleId = request.RoleId.Value;
        }

        await _userRepository.UpdateAsync(user, ct);
        return await GetByIdAsync(userId, ct);
    }

    public async Task<UserResponseDto> ToggleActiveAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.IsActive = !(user.IsActive ?? true);
        await _userRepository.UpdateAsync(user, ct);
        return await GetByIdAsync(userId, ct);
    }

    #region User Profile Operations

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var totalBookings = await _context.Bookings
            .Where(b => b.UserId == userId && !b.IsDeleted)
            .CountAsync(ct);

        var completedBookings = await _context.Bookings
            .Where(b => b.UserId == userId && !b.IsDeleted && b.Status == "Completed")
            .CountAsync(ct);

        var totalVehicles = await _context.Vehicles
            .Where(v => v.UserId == userId && v.IsActive == true)
            .CountAsync(ct);

        var totalParkingLots = await _context.ParkingLots
            .Where(p => p.OwnerId == userId && !p.IsDeleted)
            .CountAsync(ct);

        return new UserProfileDto(
            UserId: user.UserId,
            FullName: user.FullName ?? "Unknown",
            Email: user.Email ?? "",
            Phone: user.Phone,
            AvatarUrl: null, // Can be extended if avatar field is added
            RoleName: user.Role?.RoleName ?? "User",
            EmailConfirmed: user.EmailConfirmed,
            CreatedAt: user.CreatedAt,
            TotalBookings: totalBookings,
            CompletedBookings: completedBookings,
            TotalVehicles: totalVehicles,
            TotalParkingLots: totalParkingLots
        );
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        // Update only allowed fields for user self-update
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if (request.Phone != null)
        {
            user.Phone = request.Phone.Trim();
        }

        // AvatarUrl can be stored if we add the field to User entity
        // For now, we skip it

        await _userRepository.UpdateAsync(user, ct);
        return await GetProfileAsync(userId, ct);
    }

    #endregion
}
