using EventService.Application.Abstractions.Repositories;
using EventService.Infrastructure.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly EventDbContext _context;

    protected Repository(EventDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(long id)
        => await _context.Set<T>().FindAsync(id);

    public virtual async Task AddAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(long id)
    {
        T? entity = await _context.Set<T>().FindAsync(id);

        if (entity == null)
            return;

        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        => await _context.Set<T>().AsNoTracking().ToListAsync();
}