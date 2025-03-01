using App.Application.Services;
using App.Domain.Exceptions;
using MediatR;
using System.Net;

namespace App.Application.Handlers.Users;

public record DeleteUserCommand(string userId) : IRequest;

public class DeleteUserHandler(IUserService userService) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userService.GetByEmailAsync(request.userId);
        if (user == null)
        {
            throw new DomainException("User was not found", (int)HttpStatusCode.Unauthorized);
        }

        await userService.DeleteAsync(user);
    }
}