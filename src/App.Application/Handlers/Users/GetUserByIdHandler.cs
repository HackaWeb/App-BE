using App.Application.Repositories;
using App.Application.Services;
using App.Domain.Enums;
using App.Domain.Exceptions;
using App.RestContracts.Models;
using MediatR;
using System.Net;

namespace App.Application.Handlers.Users;

public record GetUserByIdCommand(string userId) : IRequest<UserModel>;

public class GetUserByIdHandler(
    IUserService userService,
    IUnitOfWork unitOfWork) : IRequestHandler<GetUserByIdCommand, UserModel>
{
    public async Task<UserModel> Handle(GetUserByIdCommand request, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(request.userId, cancellationToken);
        if (user is null)
        {
            throw new DomainException("User was not found", (int)HttpStatusCode.Unauthorized);
        }

        var isAdmin = await userService.IsInRoleAsync(user, nameof(UserRoles.ADMIN));
        var userTransaction = await unitOfWork.TransactionRepository.Find(x => x.UserId == user.Id);
        var lastTransaction = userTransaction.OrderByDescending(x => x.TransactionDate).FirstOrDefault();

        var userRest = new UserModel
        {
            Id = user.Id,
            IsAdmin = isAdmin,
            Balance = lastTransaction?.Balance ?? 0,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
        };

        return userRest;
    }
}