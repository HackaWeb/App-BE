using App.Application.Repositories;
using App.Application.Services;
using App.Domain.Enums;
using App.RestContracts.Models;
using MediatR;

namespace App.Application.Handlers.Users
{
    public record GetAllUsersCommand() : IRequest<List<UserModel>>;

    public class GetAllUsersHandler(IUserService userService, IUnitOfWork unitOfWork) : IRequestHandler<GetAllUsersCommand, List<UserModel>>
    {
        public async Task<List<UserModel>> Handle(GetAllUsersCommand request, CancellationToken cancellationToken)
        {
            var users = await userService.GetAllAsync(cancellationToken);
            var userModels = new List<UserModel>();


            foreach (var user in users)
            {
                var userTransaction = await unitOfWork.TransactionRepository.Find(x => x.UserId == user.Id);
                var lastTransaction = userTransaction.OrderByDescending(x => x.TransactionDate).FirstOrDefault();
                var isAdmin = await userService.IsInRoleAsync(user, nameof(UserRoles.ADMIN));

                userModels.Add(new UserModel
                {
                    Id = user.Id,
                    Balance = lastTransaction?.Balance ?? 0,
                    IsAdmin = isAdmin,
                    AvatarUrl = user.AvatarUrl,
                    CreatedAt = user.CreatedAt,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                });
            }

            return userModels;
        }
    }
}
