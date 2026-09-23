using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Berwah.DAL.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
            Task<T> AddAsync(T entity);

            Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

            IEnumerable<T> UpdateRange(IEnumerable<T> entities);

            public IEnumerable<T> DeleteRange(IEnumerable<T> entities);

            Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includeProperties);
            Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] includeProperties);

            Task<int> CountAsync(Expression<Func<T, bool>> filter = null);

            Task<bool> AnyAsync(Expression<Func<T, bool>> filter = null);
            Task<T> GetByIdAsync(int id);
            Task<int> GetDeviceCountByUserIdAsync(int userId);
            Task<List<T>> GetDevicesByUserIdAsync(int userId);
            Task UpdateAsync(T entity);
            Task DeleteAsync(T entity);
    }
}
