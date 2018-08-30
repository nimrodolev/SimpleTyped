using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleDB.Core;

namespace SimpleDB.Core
{
    public interface ISdbConnection
    {
        IDomain<T> GetDomain<T>(string domainName) where T : new();
        IDomain<T> GetDomain<T>(string domainName, DomainConfiguration configuration) where T : new();
        Task<DomainMetadata> GetDomainMetadata(string domainName);
        Task<IEnumerable<string>> GetDomainsAsync();
    }
}