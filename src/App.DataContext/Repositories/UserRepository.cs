using App.Application.Services;
using App.Domain.Exceptions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace App.DataContext.Repositories;

internal class UserRepository(
    UserManager<DataContext.Models.User> _userManager,
    IMapper _mapper) : IUserService
{
    public async Task<Domain.Models.User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        return appUser == null ? null : _mapper.Map<Domain.Models.User>(appUser);
    }

    public async Task<Domain.Models.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByEmailAsync(email);
        return appUser == null ? null : _mapper.Map<Domain.Models.User>(appUser);
    }

    public async Task<List<Domain.Models.User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        return _mapper.Map<List<Domain.Models.User>>(users);
    }

    public async Task<List<Domain.Models.User>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

        return _mapper.Map<List<Domain.Models.User>>(users);
    }

    public async Task<Domain.Models.User?> GetByRefreshToken(string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
        return _mapper.Map<Domain.Models.User>(user);
    }

    public async Task<bool> CheckPasswordAsync(Domain.Models.User user, string password)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null) return false;
        return await _userManager.CheckPasswordAsync(appUser, password);
    }

    public async Task CreateAsync(Domain.Models.User user, string password = null)
    {
        var appUser = _mapper.Map<DataContext.Models.User>(user);
        IdentityResult result;

        if (!string.IsNullOrEmpty(password))
        {
            result = await _userManager.CreateAsync(appUser, password);
        }
        else
        {
            result = await _userManager.CreateAsync(appUser);
        }

        if (!result.Succeeded)
            throw new DomainException("User creation failed.", (int)HttpStatusCode.BadRequest);

        user.Id = appUser.Id;
    }

    public async Task UpdateAsync(Domain.Models.User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null) throw new DomainException("User not found", (int)HttpStatusCode.Unauthorized);

        _mapper.Map(user, appUser);

        var result = await _userManager.UpdateAsync(appUser);
        if (!result.Succeeded)
            throw new DomainException("User update failed.", (int)HttpStatusCode.BadRequest);
    }

    public async Task DeleteAsync(Domain.Models.User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null) return;

        var result = await _userManager.DeleteAsync(appUser);
        if (!result.Succeeded)
            throw new DomainException("User delete failed.", (int)HttpStatusCode.BadRequest);
    }

    public async Task<bool> IsInRoleAsync(Domain.Models.User user, string roleName)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return appUser != null && await _userManager.IsInRoleAsync(appUser, roleName);
    }

    public async Task<bool> IsUserNameExists(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user != null;
    }

    public async Task<IList<string>> GetRolesAsync(Domain.Models.User user)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null) return new List<string>();
        return await _userManager.GetRolesAsync(appUser);
    }

    public async Task AddToRoleAsync(Domain.Models.User user, string roleName)
    {
        var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (appUser == null) return;
        await _userManager.AddToRoleAsync(appUser, roleName);
    }
}