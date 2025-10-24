using System.Linq.Expressions;
using AAyjet.Evaluation.Center.Persistence.Context;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AAyjet.Evaluation.Center.Persistence.Repositories;

// Bu sınıf, IRepository arayüzünü genel (generic) olarak uygular.
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().AnyAsync(predicate, cancellationToken);
    }

    // Artık SaveChangesAsync burada çağrılmıyor!
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().ToListAsync(cancellationToken);
    }
    
    public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync([id], cancellationToken: cancellationToken);
    }
    
    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
    
    
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().CountAsync(cancellationToken);
    }
    
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddRangeAsync(entities, cancellationToken);
    }
    
    public void DeleteRange(IEnumerable<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
    }
    
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);
    }
    
    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }
    
    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().Where(predicate).ToListAsync(cancellationToken);
    }
}