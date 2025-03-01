using App.Application.Services;
using App.RestContracts.Models;
using MediatR;

namespace App.Application.Handlers.Users
{
    public record GetAllUsersCommand() : IRequest<List<UserModel>>;

    public class GetAllUsersHandler(IUserService userService) : IRequestHandler<GetAllUsersCommand, List<UserModel>>
    {
        public async Task<List<UserModel>> Handle(GetAllUsersCommand request, CancellationToken cancellationToken)
        {
            var users = await userService.GetAllAsync(cancellationToken);

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
