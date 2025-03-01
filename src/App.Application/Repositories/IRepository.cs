using System.Linq.Expressions;

namespace App.Application.Repositories;

public interface IRepository<TDomain>
    where TDomain : class
{
    Task<TDomain?> GetById(Guid id, params string[] includes);

    Task<List<TDomain>> GetAll(bool isReadOnly = true, params string[] includes);

    Task<List<TDomain>> Find(Expression<Func<TDomain, bool>> predicate, bool isReadOnly = true, params string[] includes);

    Task<TDomain> FindSingle(Expression<Func<TDomain, bool>> predicate, bool isReadOnly = true, params string[] includes);

    Task Delete(TDomain entity);

    Task<TDomain> Add(TDomain entity);

    Task<List<TDomain>> AddRange(IEnumerable<TDomain> entities);
}
