using App.Domain.Models;
using App.RestContracts.Users.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Handlers.Users
{
    public record GetAllUsersCommand() : IRequest<List<UserModel>>;

    public class GetAllUsersHandler(UserManager<User> userManager) : IRequestHandler<GetAllUsersCommand, List<UserModel>>
    {
        public async Task<List<UserModel>> Handle(GetAllUsersCommand request, CancellationToken cancellationToken)
        {
            var users = await userManager.Users.ToListAsync(cancellationToken);

            return users.Select(user => new UserModel
            {
                Id = user.Id,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            }).ToList();
        }
    }
}
