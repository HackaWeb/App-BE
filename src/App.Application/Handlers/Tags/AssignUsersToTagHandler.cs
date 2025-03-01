using App.RestContracts.Shared;
using MediatR;

namespace App.Application.Handlers.Tags
{
    public record AssignUsersToTagCommand(Guid tagId, List<Guid> userIds) : IRequest<ResultResponse>;

    class AssignUsersToTagHandler : IRequestHandler<AssignUsersToTagCommand, ResultResponse>
    {
        public Task<ResultResponse> Handle(AssignUsersToTagCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
