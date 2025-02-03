using fintrack_database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.Repositories.Generic
{
    public class GenericRepository<TEntity> : IDisposable, IGenericRepository<TEntity> where TEntity : class
    {

        protected FinTrackDatabaseContext context;


        protected DbSet<TEntity> dbSet;

        public GenericRepository(FinTrackDatabaseContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context), "Database context is set incorrectly. Value is null: {context}");
            this.dbSet = context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> Get(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string includeProperties = "",
            bool noTracking = false)
        {
            IQueryable<TEntity> query;
            if (noTracking)
            {

                query = dbSet.AsNoTracking();
            }
            else
            {
                query = dbSet;
            }


            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }


        public void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public ValueTask<TEntity?> FindAsync(uint primaryKey, CancellationToken token)
        {
            return dbSet.FindAsync(primaryKey, token);
        }

        public void Delete(TEntity entityToDelete)
        {
            if (entityToDelete != null)
            {
                if (context.Entry(entityToDelete).State == EntityState.Detached)
                {
                    dbSet.Attach(entityToDelete);
                }
                dbSet.Remove(entityToDelete);
                Save();
            }
        }

        public async Task DeleteAsync(TEntity entityToDelete, CancellationToken token)
        {
            if (entityToDelete != null)
            {
                if (context.Entry(entityToDelete).State == EntityState.Detached)
                {
                    dbSet.Attach(entityToDelete);
                }
                dbSet.Remove(entityToDelete);
                await SaveAsync(token);
            }
        }

        public void DeleteRange(ICollection<TEntity> entitiesToDelete)
        {
            if (entitiesToDelete != null)
            {
                dbSet.RemoveRange(entitiesToDelete);
                Save();
            }
        }

        public async Task DeleteRangeAsync(ICollection<TEntity> entitiesToDelete, CancellationToken token)
        {
            if (entitiesToDelete != null)
            {
                dbSet.RemoveRange(entitiesToDelete);
                await SaveAsync(token);
            }
        }

        public void Update(TEntity entityToUpdate)
        {
            if (entityToUpdate != null)
            {
                dbSet.Update(entityToUpdate);
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public async Task SaveAsync(CancellationToken cancellationToken)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        private bool disposed = false;
        ~GenericRepository()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private void Dispose(bool disposing)
        {
            if (disposed == false)
            {

            }
            disposed = true;
        }

        public IQueryable<TEntity> FullTextSearch(string cols, string text, string table)
        {
            return this.dbSet.FromSqlRaw<TEntity>("SELECT * FROM " + table + " WHERE MATCH(" + cols + ") AGAINST ('*" + text + "*' IN BOOLEAN MODE)");
        }
    }
}
