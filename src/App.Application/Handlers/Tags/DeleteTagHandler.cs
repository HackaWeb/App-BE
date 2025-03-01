using App.Application.Repositories;
using App.RestContracts.Shared;
using MediatR;

namespace App.Application.Handlers.Tags;

public record DeleteTagCommand(Guid tagId) : IRequest<ResultResponse>;

public class DeleteTagHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteTagCommand, ResultResponse>
{
    public async Task<ResultResponse> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await unitOfWork.TagRepository.FindSingle(x => x.Id == request.tagId);

        await unitOfWork.TagRepository.Delete(tag);
        await unitOfWork.SaveChangesAsync();

        return new ResultResponse() { IsSuccess = true, StatusCode = 200 };
    }
}