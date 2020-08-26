using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GameMasterBot.Extensions
{
    public static class DbSetExtensions
    {
        public static async Task<T> AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate) where T : class, new() => 
            await dbSet.SingleOrDefaultAsync(predicate) ?? (await dbSet.AddAsync(entity)).Entity;
    }
}