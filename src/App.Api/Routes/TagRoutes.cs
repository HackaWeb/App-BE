using App.Application.Handlers.Tags;
using App.Application.Handlers.Users;
using App.RestContracts.Tags;
using MediatR;

namespace App.Api.Routes;

public static class TagRoutes
{
    public static void MapTagRoutes(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/tags")
            .WithName("Tags")
            .WithTags("Tags");

        group.MapPost("/", async (CreateTagRequest request, IMediator mediator) =>
            {
                return await mediator.Send(new CreateTagCommand(request.Name, request.UserIds));
            })
            .WithName("CreateTag");

        group.MapGet("/{tagId:guid}", async (Guid tagId, IMediator mediator) =>
            {
                return await mediator.Send(new GetTagByIdCommand(tagId));
            })
            .WithName("GetTag");

        group.MapDelete("/{tagId:guid}", async (Guid tagId, IMediator mediator) =>
            {
                return await mediator.Send(new DeleteTagCommand(tagId));

            })
            .WithName("DeleteTag");

        group.MapDelete("/{tagId:guid}/{userId:guid}", async (Guid tagId, Guid userId, IMediator mediator) =>
            {
                return await mediator.Send(new RemoveUserTag(tagId, userId));
            })
            .WithName("DeleteUserTag");
    }
}
