using System;
using System.Linq.Expressions;

namespace SimpleDB.Core.Query
{
    public interface ISelectQuery<T>
    {
        ISelectQuery<T> Exclude(params Expression<Func<T, object>>[] members);
        ISelectQuery<T> Exclude(params string[] members);
        ISelectQuery<T> Include(params Expression<Func<T, object>>[] members);
        ISelectQuery<T> Include(params string[] members);
        ISelectQuery<T> Limit(int limit);
        ISelectQuery<T> OrderBy<TMember>(Expression<Func<T, TMember>> member, Direction direction);
        string Selector { get; }
        string Assemble(string domainName, bool isCount);
        string ToString();
    }
}