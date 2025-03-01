using App.Application.Repositories;
using App.Domain.Models;
using App.RestContracts.Shared;
using MediatR;

namespace App.Application.Handlers.Tags
{
    public record RemoveUserTag(Guid tagId, Guid userId) : IRequest<ResultResponse>;
    
    public class RemoveUserTagHandler(IUnitOfWork unitOfWork) : IRequestHandler<RemoveUserTag, ResultResponse>
    {
        public async Task<ResultResponse> Handle(RemoveUserTag request, CancellationToken cancellationToken)
        {
            var userTag = await unitOfWork.UserTagRepository.FindSingle(x => x.UserId == request.userId && x.TagId == request.tagId, isReadOnly: false);

            await unitOfWork.UserTagRepository.Delete(userTag);
            await unitOfWork.SaveChangesAsync();

            return new ResultResponse() { StatusCode = 200, IsSuccess = true, };
        }
    }
}
