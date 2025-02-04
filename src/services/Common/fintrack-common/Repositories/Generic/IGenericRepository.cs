using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.Repositories.Generic
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        void Delete(TEntity entityToDelete);
        Task DeleteAsync(TEntity entityToDelete, CancellationToken token);
        void DeleteRange(ICollection<TEntity> entitiesToDelete);
        Task DeleteRangeAsync(ICollection<TEntity> entitiesToDelete, CancellationToken token);
        void Dispose();
        ValueTask<TEntity?> FindAsync(uint primaryKey, CancellationToken token);
        IQueryable<TEntity> FullTextSearch(string cols, string text, string table);
        void Insert(TEntity entity);
        void Save();
        Task SaveAsync(CancellationToken cancellationToken);
        void Update(TEntity entityToUpdate);
    }
}
