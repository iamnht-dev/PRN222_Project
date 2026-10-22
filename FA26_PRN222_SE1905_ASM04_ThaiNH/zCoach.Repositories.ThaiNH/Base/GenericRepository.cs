using Microsoft.EntityFrameworkCore;
using zCoach.Repositories.ThaiNH.DBContext;
using zCoach.Entities.ThaiNH.Models;

namespace zCoach.Repositories.ThaiNH.Base;

/// <summary>
/// Repository dùng chung cho mọi entity. Luôn dùng Async và nhận DbContext qua DI.
/// </summary>
public class GenericRepository<T> where T : class
{
    protected readonly PRN222Context _context;

    public GenericRepository(PRN222Context context)
    {
        _context = context;
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        // Context mặc định NoTracking; xoá tracker để tránh xung đột khi một entity
        // cùng khoá đã được theo dõi từ lần lưu trước (Blazor Server circuit dài).
        _context.ChangeTracker.Clear();
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> DeleteAsync(T entity)
    {
        _context.ChangeTracker.Clear();
        _context.Set<T>().Remove(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        _context.Set<T>().UpdateRange(entities);
        await _context.SaveChangesAsync();
    }
}
