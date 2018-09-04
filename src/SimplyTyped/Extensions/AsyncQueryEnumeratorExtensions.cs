using System.Collections.Generic;
using System.Threading.Tasks;
using SimplyTyped.Core.Query;

namespace SimplyTyped.Query
{
    public static class IAsyncQueryEnumeratorExtensions
    {
        public static async Task<IEnumerable<T>> AllAsync<T>(this IAsyncQueryEnumerator<T> asyncEnumerator)
        {
            var result = new List<T>();
            while (await asyncEnumerator.MoveNextAsync())
                result.Add(await asyncEnumerator.Current);
            return result;
        }
    }
}