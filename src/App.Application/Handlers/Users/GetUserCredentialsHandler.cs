using App.Application.Repositories;
using App.Application.Services;
using App.Domain.Enums;
using App.Domain.Exceptions;
using App.RestContracts.Models;
using MediatR;
using System.Net;

namespace App.Application.Handlers.Users
{
    public record GetUserCredentialsCommand(Guid UserId) : IRequest<UserModel>;
    class GetUserCredentialsHandler(IUnitOfWork unitOfWork, IUserService userService) : IRequestHandler<GetUserCredentialsCommand, UserModel>
    {
        public async Task<UserModel> Handle(GetUserCredentialsCommand request, CancellationToken cancellationToken)
        {
            var keys = await unitOfWork.CredentialsRepository.Find(x => x.UserId == request.UserId, true, "User");
            if (keys is null)
            {
                throw new DomainException("No API keys", (int)HttpStatusCode.BadRequest);
            }

            var user = keys.FirstOrDefault().User;
            var isAdmin = await userService.IsInRoleAsync(user, nameof(UserRoles.ADMIN));
            var userTransaction = await unitOfWork.TransactionRepository.Find(x => x.UserId == user.Id);
            var lastTransaction = userTransaction.OrderByDescending(x => x.TransactionDate).FirstOrDefault();

            var userModel = new UserModel()
            {
                IsAdmin = isAdmin,
                Balance = lastTransaction.Balance,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                Keys = keys.Select(x => new KeyModel() { KeyType = x.UserCredentialType, Value = x.Value, })
                    .ToList(),
            };


            return userModel;
        }
    }
}
