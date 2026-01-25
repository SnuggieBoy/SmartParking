using SmartParking.Domain.Entities;

namespace SmartParking.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int roleId, CancellationToken ct = default);
    Task<Role?> GetByNameAsync(string roleName, CancellationToken ct = default);
}
