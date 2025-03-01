using App.Application.Repositories;
using App.DataContext.Models;
using App.Domain;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace App.DataContext.Repositories;

internal class Repository<TDomain, TEntity> : IRepository<TDomain> 
    where TDomain : BaseModel
    where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _dbSet;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public Repository(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _mapper = mapper;
    }

    public Task Delete(TDomain domainEntity)
    {
        var entity = _mapper.Map<TEntity>(domainEntity);
        _dbSet.Remove(entity);

        return Task.CompletedTask;
    }

    public async Task<TDomain> Add(TDomain domainEntity)
    {
        var entity = _mapper.Map<TEntity>(domainEntity);
        var entry = await _dbSet.AddAsync(entity);

        return _mapper.Map<TDomain>(entry.Entity);
    }

    public async Task<List<TDomain>> AddRange(IEnumerable<TDomain> domainEntities)
    {
        var entities = _mapper.Map<List<TEntity>>(domainEntities);
        await _dbSet.AddRangeAsync(entities);

        return _mapper.Map<List<TDomain>>(entities);
    }

    public async Task<List<TDomain>> Find(Expression<Func<TDomain, bool>> predicate, bool isReadOnly = true, params string[] includes)
    {
        var entityPredicate = _mapper.MapExpression<Expression<Func<TEntity, bool>>>(predicate);
        IQueryable<TEntity> query = _dbSet.Where(entityPredicate);
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (isReadOnly)
        {
            query = query.AsNoTracking();
        }

        var entities = await query.ToListAsync();
        return _mapper.Map<List<TDomain>>(entities);
    }

    public async Task<TDomain?> FindSingle(Expression<Func<TDomain, bool>> predicate, bool isReadOnly = true, params string[] includes)
    {
        var entityPredicate = _mapper.MapExpression<Expression<Func<TEntity, bool>>>(predicate);
        IQueryable<TEntity> query = _dbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (isReadOnly)
        {
            query = query.AsNoTracking();
        }

        var entity = await query.SingleOrDefaultAsync(entityPredicate);
        return _mapper.Map<TDomain>(entity);
    }

    public async Task<List<TDomain>> GetAll(bool isReadOnly = true, params string[] includes)
    {
        IQueryable<TEntity> query = _dbSet;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (isReadOnly)
        {
            query = query.AsNoTracking();
        }

        var entities = await query.ToListAsync();
        return _mapper.Map<List<TDomain>>(entities);
    }

    public async Task<TDomain?> GetById(Guid id, params string[] includes)
    {
        IQueryable<TEntity> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        var entity = await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        return _mapper.Map<TDomain>(entity);
    }
}
