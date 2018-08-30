using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleDB.Core.Query;

namespace SimpleDB.Core
{
    public interface IDomain<T> where T : new()
    {
        Task BatchDeleteAsync<TMember>(IEnumerable<TMember> ids);
        Task BatchPutAsync(IEnumerable<T> items);
        Task DeleteOneAsync<TMember>(TMember id);
        Task<T> GetByIdAsyc<TMember>(TMember id);
        ISelectQueryBuilder<T> GetQueryBuilder();
        Task PutAsync(T item);
        Task PutAsync(T item, bool throwIfExists);
        Task<IEnumerable<T>> SelectAsync(ISelectQuery<T> query, bool consistantRead);
        Task<long> SelectCountAsync(ISelectQuery<T> query, bool consistantRead);
    }
}