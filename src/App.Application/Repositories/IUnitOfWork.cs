using App.Domain.Models;

namespace App.Application.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<Tag> TagRepository { get; }

        IRepository<UserTag> UserTagRepository { get; }

        IRepository<Notification> NotificationRepository { get; }

        IRepository<Transaction> TransactionRepository { get; }

        Task SaveChangesAsync();
    }
}
