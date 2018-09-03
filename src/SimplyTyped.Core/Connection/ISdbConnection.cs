using System.Collections.Generic;
using System.Threading.Tasks;
using SimplyTyped.Core;

namespace SimplyTyped.Core
{
    /// <summary>
    /// Allows instantiating IDomain instances, as well performing higher level operation such as listing all domains.
    /// </summary>
    public interface ISdbConnection
    {
        /// <summary>
        /// Instantiates a typed IDomain instance that can be used to perform operations against a given domain
        /// </summary>
        /// <param name="domainName">The name of the domain to work against</param>
        /// <returns>A typed domain object with the default configurations</returns>
        IDomain<T> GetDomain<T>(string domainName) where T : new();
        
        /// <summary>
        /// Instantiates a typed IDomain instance that can be used to perform operations against a given domain
        /// </summary>
        /// <param name="domainName">The name of the domain to work against</param>
        /// <param name="configuration">A DomainConfiguration object to be used when configuring the IDomain instance</param>
        /// <returns>A typed domain object with the given configurations</returns>
        IDomain<T> GetDomain<T>(string domainName, DomainConfiguration configuration) where T : new();
        
        /// <summary>
        /// Fetches the metadata from the requested SimpleDB domain. 
        /// </summary>
        /// <param name="domainName">The name of the domain for which metadata should be fetched</param>
        /// <returns>An object containing domain's metadata</returns>     
        Task<DomainMetadata> GetDomainMetadata(string domainName);
        
        /// <summary>
        /// Fetches the names of all existing SimpleDB domains
        /// </summary>
        /// <returns>An IEnumerable&lt;string&gt; with all of the domain names</returns>        
        Task<IEnumerable<string>> ListDomainsAsync();
    }
}