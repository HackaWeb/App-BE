using App.Application.Repositories;
using App.Application.Services;
using App.Domain.Models;
using App.RestContracts.Shared;
using MediatR;

namespace App.Application.Handlers.Tags
{
    public record CreateTagCommand(string Name, List<Guid>? userIds) : IRequest<ResultResponse>;
    public class CreateTagHandler(IUnitOfWork unitOfWork, IUserService userService) : IRequestHandler<CreateTagCommand, ResultResponse>
    {
        public async Task<ResultResponse> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            var tag = new Tag { Name = request.Name, };
            if (request.userIds != null && request.userIds.Any())
            {
                var users = await userService.GetByIdsAsync(request.userIds);
                tag.UserTags = users.Select(user => new UserTag() { UserId = user.Id, Tag = tag, }).ToList();
            }

            await unitOfWork.TagRepository.Add(tag);
            await unitOfWork.SaveChangesAsync();

            return new ResultResponse() { IsSuccess = true, StatusCode = 200, };
        }
    }
}
