using System;
using System.Linq.Expressions;

namespace SimplyTyped.Core.Query
{
    /// <summary>
    /// Represents a SimpleDB Select query and can be used to generate a valid query string.
    /// </summary>
    public interface IQuery<T>
    {
        /// <summary>
        /// Sets a set of properties as excluded, meaning that their values will not be fetched from SimpleDB when querying.
        /// </summary>
        /// <param name="members">A set of expressions that will be used to extract the property names</param>
        /// <returns>Itself</returns>        
        IQuery<T> Exclude(params Expression<Func<T, object>>[] members);

        /// <summary>
        /// Sets a set of properties as excluded, meaning that their values will not be fetched from SimpleDB when querying.
        /// </summary>
        /// <param name="members">The names of the properties to exclude</param>
        /// <returns>Itself</returns>        
        IQuery<T> Exclude(params string[] members);
       
        /// <summary>
        /// Sets a set of properties as include, meaning that only their values will be fetched from SimpleDB when querying.
        /// </summary>
        /// <param name="members">A set of expressions that will be used to extract the property names</param>
        /// <returns>Itself</returns>        
        IQuery<T> Include(params Expression<Func<T, object>>[] members);
        
        /// <summary>
        /// Sets a set of properties as include, meaning that only their values will be fetched from SimpleDB when querying.
        /// </summary>
        /// <param name="members">The names of the properties to include</param>
        /// <returns>Itself</returns>    
        IQuery<T> Include(params string[] members);
        
        /// <summary>
        /// Sets the maximum number of results to return when querying.
        /// </summary>
        /// <param name="limit">The limit value</param>
        /// <returns>Itself</returns>  
        IQuery<T> Limit(int limit);

        /// <summary>
        /// Sets the sort order of the query results.
        /// </summary>
        /// <param name="member">A member expression to define the property the order by</param>
        /// <param name="direction">The ordering direction</param>
        /// <returns>Itself</returns>  
        IQuery<T> OrderBy<TMember>(Expression<Func<T, TMember>> member, Direction direction);
        
        //TODO: Eliminate the need for this
        ICondition Condition { get; }

        /// <summary>
        /// Sets the sort order of the query results.
        /// </summary>
        /// <param name="domainName">The name of the domain to query</param>
        /// <param name="isCount">A value indicating whether the query should be structured as a count query or not</param>
        /// <returns>A query string that can be used to query a SimpleDB domain</returns>  
        string BuildQuery(string domainName, bool isCount);

        string ToString();
    }
}