using App.Application.Repositories;
using App.Application.Services;
using App.Domain.Enums;
using App.Domain.Exceptions;
using App.RestContracts.Models;
using MediatR;
using System.Net;

namespace App.Application.Handlers.Users;

public record UpdateUserCommand(string userId, string? firstName, string? lastName, string? email, string? username = null) : IRequest<UserModel>;

public class UpdateUserHandler(IUserService userService, IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, UserModel>
{
    public async Task<UserModel> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(request.userId);
        if (user is null)
        {
            throw new DomainException("User was not found", (int)HttpStatusCode.Unauthorized);
        }

        if (!string.IsNullOrWhiteSpace(request.email))
            user.Email = request.email;
        
        if (!string.IsNullOrWhiteSpace(request.firstName))
            user.FirstName = request.firstName;
        
        if (!string.IsNullOrWhiteSpace(request.lastName))
            user.LastName = request.lastName;
        
        if (!string.IsNullOrWhiteSpace(request.username))
            user.LastName = request.username;

        var isAdmin = await userService.IsInRoleAsync(user, nameof(UserRoles.ADMIN));
        var userTransaction = await unitOfWork.TransactionRepository.Find(x => x.UserId == user.Id);
        var lastTransaction = userTransaction.OrderByDescending(x => x.TransactionDate).FirstOrDefault();
        await userService.UpdateAsync(user);

        return new UserModel{
            Id = user.Id,
            IsAdmin = isAdmin,
            Balance = lastTransaction?.Balance ?? 0,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarUrl = user.AvatarUrl,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
        };
    }
}