using System.Collections.Generic;

namespace Common.Interfaces.DataAccess.Repositories
{
    public interface IRepository<TEntity> where TEntity: class
    {
        TEntity Get(string id);
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Remove(IEnumerable<TEntity> entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
