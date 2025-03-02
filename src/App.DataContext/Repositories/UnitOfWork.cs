using App.Application.Repositories;
using App.Domain.Models;
using AutoMapper;

namespace App.DataContext.Repositories
{
    internal class UnitOfWork(AppDbContext context, IMapper mapper) : IUnitOfWork
    {
        public IRepository<Tag> TagRepository => new Repository<Tag, Models.Tag>(context, mapper);
        public IRepository<UserTag> UserTagRepository => new Repository<UserTag, Models.UserTag>(context, mapper);
        public IRepository<Notification> NotificationRepository => new Repository<Notification, Models.Notification>(context, mapper);
        public IRepository<Transaction> TransactionRepository => new Repository<Transaction, Models.Transaction>(context, mapper);

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
