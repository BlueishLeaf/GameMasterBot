using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces.Entities;

namespace Common.Interfaces.DataAccess
{
    public interface IDynamoDbContext: IDisposable
    {
        Task<IEnumerable<IDynamoDbItem>> GetByPkAsync(string pk);
        Task<IDynamoDbItem> GetByPkAndSkAsync(string pk, string sk);
        Task SaveAsync(IDynamoDbItem item);
        Task DeleteByIdAsync(IDynamoDbItem item);
    }

}