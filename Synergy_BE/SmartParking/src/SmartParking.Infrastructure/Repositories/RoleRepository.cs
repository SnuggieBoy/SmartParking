using Microsoft.EntityFrameworkCore;
using SmartParking.Application.Interfaces.Repositories;
using SmartParking.Domain.Entities;
using SmartParking.Infrastructure.Data;

namespace SmartParking.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly SmartParkingDBContext _context;

    public RoleRepository(SmartParkingDBContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(int roleId, CancellationToken ct = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleId == roleId, ct);
    }

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken ct = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleName == roleName, ct);
    }
}
