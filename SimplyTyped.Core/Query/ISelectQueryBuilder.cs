using System;
using System.Linq.Expressions;

namespace SimplyTyped.Core.Query
{
    public interface ISelectQueryBuilder<T>
    {
        ISelectQuery<T> And(params ISelectQuery<T>[] quries);
        ISelectQuery<T> Between<TMember>(Expression<Func<T, TMember>> member, TMember left, TMember right);
        ISelectQuery<T> Empty();
        ISelectQuery<T> In<TMember>(Expression<Func<T, TMember>> member, params TMember[] values);
        ISelectQuery<T> Intersection(params ISelectQuery<T>[] quries);
        ISelectQuery<T> IsNull<TMember>(Expression<Func<T, TMember>> member);
        ISelectQuery<T> Like<TMember>(Expression<Func<T, TMember>> member, string pattern);
        ISelectQuery<T> Not(ISelectQuery<T> qurey);
        ISelectQuery<T> Or(params ISelectQuery<T>[] quries);
        ISelectQuery<T> Where(Expression<Func<T, bool>> condition);
    }
}