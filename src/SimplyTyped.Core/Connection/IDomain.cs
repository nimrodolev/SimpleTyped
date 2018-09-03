using System.Collections.Generic;
using System.Threading.Tasks;
using SimplyTyped.Core.Query;

namespace SimplyTyped.Core
{
    /// <summary>
    /// Allows performing operations against a SimpleDB domain is a strongly typed manner.
    /// </summary>
    public interface IDomain<T> where T : new()
    {
        /// <summary>
        /// Deletes a set of items from the domain.
        /// </summary>
        /// <remarks>
        /// A SimpleDB "BatchDeleteAttributes" operation is limited to 25 items per call. If you pass more than 25 items
        /// they will get sent in batches of 25. In the even of an error, this splitting of the operation can cause inconsistency, and so it is recommended
        /// to limit the your batch size to 25, and calls BatchDeleteAsync multiple times if needed.
        /// </remarks>
        /// <param name="items">The Ids of the items to delete</param>
        /// <returns></returns>
        Task BatchDeleteAsync<TMember>(IEnumerable<TMember> ids);
        
        /// <summary>
        /// Adds a set of items to the domain. 
        /// </summary>
        /// <remarks>
        /// A SimpleDB "BatchPutAttributes" operation is limited to 25 items per call. If you pass more than 25 items
        /// they will get sent in batches of 25. In the even of an error, this splitting of the operation can cause inconsistency, and so it is recommended
        /// to limit the your batch size to 25, and calls BatchPutAsync multiple times if needed.
        /// </remarks>
        /// <param name="items">The items to be added</param>
        /// <returns></returns>
        Task BatchPutAsync(IEnumerable<T> items);
        
        /// <summary>
        /// Deletes one item by its Id
        /// </summary>
        /// <param name="id">The id of the item to delete. Must be of the item's Id member type.</param>
        /// <returns></returns>
        Task DeleteOneAsync<TMember>(TMember id);
        
        /// <summary>
        /// Retrives one item by its Id. Will throw an exception if the items is not found.
        /// </summary>
        /// <param name="id">The id of the item to fetch. Must be of the item's Id member type.</param>
        /// <returns>The item request, of type T</returns>
        Task<T> GetByIdAsync<TMember>(TMember id);
        
        /// <summary>
        /// Instantiates and returns a SelectQueryBuilder&lt;T&gt; that can be used to comprise a Select query to fetch data using SelectAsync and SelectCountAsync
        /// </summary>
        /// <returns>An instance of SelectQueryBuilder&lt;T&gt;</returns>
        ISelectQueryBuilder<T> GetQueryBuilder();
        
        /// <summary>
        /// Adds an item in the domain. Throws an exception if the item already exists.
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <returns></returns>
        Task PutAsync(T item);

        /// <summary>
        /// Adds or replaces an item in the domain.
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <param name="throwIfExists">A flag indicating whether an exception should be raised in case the item already exists. If false, existing values will be overridden</param>
        /// <returns></returns>
        Task PutAsync(T item, bool throwIfExists);

        /// <summary>
        /// Issues a Select request for the given query. All results are fetched, even if they are paged.
        /// </summary>
        /// <param name="query">The query object</param>
        /// <param name="consistantRead">Sets the underlaying consistency option for the issued Select request, 
        /// as described in the AWS SimpleDB documentation found at https://docs.aws.amazon.com/AmazonSimpleDB/latest/DeveloperGuide/ConsistencySummary.html</param>
        /// <returns>The Select request's results, deserialized into instances of T</returns>
        Task<IEnumerable<T>> SelectAsync(ISelectQuery<T> query, bool consistantRead);
        
        /// <summary>
        /// Issues a given Select request as a count query.
        /// </summary>
        /// <param name="query">The query object</param>
        /// <param name="consistantRead">Sets the underlaying consistency option for the issued Select request, 
        /// as described in the AWS SimpleDB documentation found at https://docs.aws.amazon.com/AmazonSimpleDB/latest/DeveloperGuide/ConsistencySummary.html</param>
        /// <returns>A long value representing the amount of results for the given Select query</returns>
        Task<long> SelectCountAsync(ISelectQuery<T> query, bool consistantRead);
    }
}