using System.Linq;
using System.Threading.Tasks;

namespace SimplyTyped.Core.Query
{
    public interface IAsyncQueryEnumerator<T>
    {
        Task<bool> MoveNextAsync();
        Task<T> Current { get; }
    }
}