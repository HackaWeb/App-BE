using App.Application.Repositories;
using App.Domain.Exceptions;
using App.Domain.Models;
using App.RestContracts.Models;
using MediatR;
using System.Net;

namespace App.Application.Handlers.Tags
{
    public record GetTagByIdCommand(Guid tagId) : IRequest<TagModel>;

    public class GetTagByIdHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTagByIdCommand, TagModel>
    {
        public async Task<TagModel> Handle(GetTagByIdCommand request, CancellationToken cancellationToken)
        {
            var tag = await unitOfWork.TagRepository.GetById(request.tagId, "UserTags.User");
            if (tag is null)
            {
                throw new DomainException($"Tag with {request.tagId} doesn't exist.", (int)HttpStatusCode.BadRequest);
            }

            return new TagModel
            {
                Id = tag.Id,
                Name = tag.Name,
                Users = tag.UserTags.Select(ut => new UserModel()
                {
                    Id = ut.UserId,
                    AvatarUrl = ut.User.AvatarUrl,
                    CreatedAt = ut.User.CreatedAt,
                    Email = ut.User.Email,
                    FirstName = ut.User.FirstName,
                    LastName = ut.User.LastName,
                }).ToList()
            };

        }
    }
}
