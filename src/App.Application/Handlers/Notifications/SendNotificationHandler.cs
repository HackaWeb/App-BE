using App.Application.Repositories;
using App.Domain.Models;
using App.RestContracts.Shared;
using MediatR;

namespace App.Application.Handlers.Notifications
{
    public record SendNotificationCommand(Guid senderId, Guid usersTagId, string Title, string message) : IRequest<ResultResponse>;

    public class SendNotificationHandler(IUnitOfWork unitOfWork) : IRequestHandler<SendNotificationCommand, ResultResponse>
    {
        public async Task<ResultResponse> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        {
            var users = await unitOfWork.UserTagRepository.Find(x => x.TagId == request.usersTagId);
            var notifications = users.Select(x => new Notification()
            {
                Message = request.message, SenderId = request.senderId, Title = request.Title, UserId = x.UserId,
            });

            await unitOfWork.NotificationRepository.AddRange(notifications);

            await unitOfWork.SaveChangesAsync();
            return new ResultResponse() { IsSuccess = true, StatusCode = 200, };
        }
    }
}
