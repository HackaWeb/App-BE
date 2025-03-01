using App.Domain.Models;

namespace App.Application.Services;

public interface IUserService
{
    Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<User>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshToken(string refreshTokenCommand, CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task CreateAsync(User user, string password = null);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<bool> IsInRoleAsync(User user, string roleName);
    Task<bool> IsUserNameExists(string username);
    Task<IList<string>> GetRolesAsync(User user);
    Task AddToRoleAsync(User user, string roleName);
}