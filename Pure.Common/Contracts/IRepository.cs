using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pure.Common.Contracts
{
    public interface IRepository<T> where T : IMongoCommon
    {
        T Create();
        Task<IEnumerable<T>> Get(Expression<Func<T, bool>> predicate);
        Task<T> GetByIdAsync(object id);
        Task<List<T>> FindAllAsync();
        Task<List<T>> FindAllAsyncReversed();
        Task Add(T entity);
        Task Update(T entity);
        Task UpdateMany(List<T> entities);
        Task Delete(T entity);
        IQueryable<T> Collection { get; }
        IQueryable<T> GetQueryable(bool includedDeleted = false);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
