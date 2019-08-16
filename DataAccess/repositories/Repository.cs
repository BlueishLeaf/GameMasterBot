using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces.DataAccess.Repositories;

namespace DataAccess.Repositories
{
    public class Repository<TEntity>: IRepository<TEntity> where TEntity: class
    {
        protected DynamoDBContext Context;

        public Repository(DynamoDBContext context) => Context = context;

        public TEntity Get(string id)
        {
            throw new NotImplementedException();
        }

        public void Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        public void Remove(IEnumerable<TEntity> entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
