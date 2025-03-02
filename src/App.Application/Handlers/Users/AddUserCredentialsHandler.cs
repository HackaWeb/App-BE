using App.Application.Repositories;
using App.Domain.Enums;
using App.Domain.Models;
using App.RestContracts.Shared;
using MediatR;

namespace App.Application.Handlers.Users
{
    public record AddUserCredentialsCommand(UserCredentialType type, string value, Guid userId) : IRequest<ResultResponse>;
    class AddUserCredentialsHandler(IUnitOfWork unitOfWork) : IRequestHandler<AddUserCredentialsCommand, ResultResponse>
    {
        public async Task<ResultResponse> Handle(AddUserCredentialsCommand request, CancellationToken cancellationToken)
        {
            var creds = new Credential()
            {
                UserCredentialType = request.type, Value = request.value, UserId = request.userId,
            };


            await unitOfWork.CredentialsRepository.Add(creds);
            await unitOfWork.SaveChangesAsync();

            return new ResultResponse() { IsSuccess = true, StatusCode = 200, };
        }
    }
}
