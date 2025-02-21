using App.Domain;
using App.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace App.DataContext.Repository;

public class Repository<T> : IRepository<T> where T : BaseModel
{
    private readonly DbSet<T> _dbSet;
    private readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> Add(T entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }

    public async Task<List<T>> Find(Expression<Func<T, bool>> predicate, bool isReadOnly = true, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet.Where(predicate);
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return isReadOnly ? await query.AsNoTracking().ToListAsync() : await query.ToListAsync();
    }

    public async Task<List<T>> GetAll(bool isReadOnly = true, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return isReadOnly ? await query.AsNoTracking().ToListAsync() : await query.ToListAsync();
    }

    public async Task<T?> GetById(Guid id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
