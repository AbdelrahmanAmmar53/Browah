using Berwah.DAL.App;
using Berwah.DAL.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace Berwah.DAL.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
            private readonly DbSet<T> _dbSet;
            private readonly ApplicationDbContext _context;

            public Repository(ApplicationDbContext context)
            {
                _dbSet = context.Set<T>();
                _context = context;
            }

            public async Task<T> AddAsync(T entity)
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
            {
                await _dbSet.AddRangeAsync(entities);
                return entities;
            }



            public IEnumerable<T> UpdateRange(IEnumerable<T> entities)
            {
                _dbSet.UpdateRange(entities);
                return entities;
            }



            public IEnumerable<T> DeleteRange(IEnumerable<T> entities)
            {
                _dbSet.RemoveRange(entities);
                return entities;
            }

            public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includeProperties)
            {
                var query = filter != null ? _dbSet.Where(filter) : _dbSet;

                if (includeProperties != null)
                    query = includeProperties.Aggregate(query, (record, property) => record.Include(property));
                return await query.ToListAsync();
            }

            public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includeProperties)
            {
                var query = filter != null ? _dbSet.Where(filter) : _dbSet;

                if (includeProperties != null)
                    query = includeProperties.Aggregate(query, (record, property) => record.Include(property));

                return await query.FirstOrDefaultAsync();
            }

            public async Task<int> CountAsync(Expression<Func<T, bool>> filter = null)
            {
                return await (filter != null ? _dbSet.CountAsync(filter) : _dbSet.CountAsync());
            }

            public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter = null)
            {
                return await (filter != null ? _dbSet.AnyAsync(filter) : _dbSet.AnyAsync());
            }
            public async Task<T> GetByIdAsync(int id)
            {
                return await _dbSet.FindAsync(id);
            }
            public async Task<int> GetDeviceCountByUserIdAsync(int userId)
            {
                return await _dbSet.CountAsync(d => EF.Property<int>(d, "UserId") == userId);
            }
            public async Task<List<T>> GetDevicesByUserIdAsync(int userId)
            {
                return await _dbSet.Where(d => EF.Property<int>(d, "UserId") == userId).ToListAsync();
            }
            public async Task UpdateAsync(T entity)
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
            }
            public async Task DeleteAsync(T entity)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }

    }
}

