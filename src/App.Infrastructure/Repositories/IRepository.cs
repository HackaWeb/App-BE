using System.Linq.Expressions;

namespace App.Infrastructure.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetById(Guid id, params Expression<Func<T, object>>[] includes);

    Task<List<T>> GetAll(bool isReadOnly = true, params Expression<Func<T, object>>[] includes);

    Task<List<T>> Find(Expression<Func<T, bool>> predicate, bool isReadOnly = true, params Expression<Func<T, object>>[] includes);

    Task<T> Add(T entity);
}
