using System;
using System.Linq.Expressions;

namespace SimplyTyped.Core.Query
{
    /// <summary>
    /// Used to create and compile ISelectQuery instances
    /// </summary>
    public interface ISelectQueryBuilder<T>
    {
        /// <summary>
        /// Constructs a new query buy joining several queries into a singal one with an 'AND' operator.
        /// </summary>
        /// <param name="queries">The queries to join</param>
        /// <returns>A new query</returns>
        ISelectQuery<T> And(params ISelectQuery<T>[] queries);

        /// <summary>
        /// Constructs a new 'BETWEEN' condition, in the form of 'X BETWEEN left AND right'.
        /// </summary>
        /// <param name="member">A member expression to define the property the query over</param>
        /// <param name="left">The left (first) operand</param>
        /// <param name="right">The right (second) operand</param>
        /// <returns>A new query</returns>
        ISelectQuery<T> Between<TMember>(Expression<Func<T, TMember>> member, TMember left, TMember right);
        
        /// <summary>
        /// Constructs a new, empty query. At this query will return all items without filtering.
        /// </summary>
        /// <returns>A new, empty query</returns>
        ISelectQuery<T> Empty();

        /// <summary>
        /// Constructs a new 'IN' query to filter for values in a given set.
        /// </summary>
        /// <param name="member">A member expression to define the property the query over</param>
        /// <param name="values">Set the of values to use for the query</param>
        /// <returns>A new query</returns>
        ISelectQuery<T> In<TMember>(Expression<Func<T, TMember>> member, params TMember[] values);
        
        /// <summary>
        /// Constructs a new query that filters for the intersection of all given queries.
        /// </summary>
        /// <param name="queries">The queries to intersect</param>
        /// <returns>A new query</returns>
        ISelectQuery<T> Intersection(params ISelectQuery<T>[] queries);
        
        
        /// <summary>
        /// Constructs a new query that filters for items where the given property is NULL
        /// </summary>
        /// <param name="member">A member expression to define the property the query over</param>
        /// <returns>A new query</returns>
        ISelectQuery<T> IsNull<TMember>(Expression<Func<T, TMember>> member);
        
        /// <summary>
        /// Constructs a new 'LIKE' query to filter on string values.
        /// </summary>
        /// <param name="member">A member expression to define the property the query over</param>
        /// <param name="pattern">The search pattern</param>
        /// <returns>A new query</returns>
        ISelectQuery<T> Like(Expression<Func<T, string>> member, string pattern);
        
        /// <summary>
        /// Constructs a new query by applying a 'NOT' operator on an existing query.
        /// </summary>
        /// <param name="query">The original query</param>
        /// <returns>A new query</returns>
        ISelectQuery<T> Not(ISelectQuery<T> query);

        /// <summary>
        /// Constructs a new query buy joining several queries into a singal one with an 'OR' operator.
        /// </summary>
        /// <param name="queries">The queries to join</param>
        /// <returns>A new query</returns>
        ISelectQuery<T> Or(params ISelectQuery<T>[] queries);

        /// <summary>
        /// Constructs a new query buy processing a given expression.
        /// </summary>
        /// <param name="queries">The queries to join</param>
        /// <returns>A new query</returns>
        /// <remarks>
        /// This is the only method that creates a filtering query without performing any validation 
        /// to make sure the properties that are being queried are actually queryable. Also, it reads the expression 
        /// and converts it into SQL-like code - meaning it will probably fail if used with a complicated expression. 
        /// That said - simple expressions will probably work without a problem.
        /// </remarks>
        ISelectQuery<T> Where(Expression<Func<T, bool>> condition);
    }
}