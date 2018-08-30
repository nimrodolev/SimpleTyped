using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using SimpleDB.Core;
using SimpleDB.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDB
{
    public class SdbConnection : ISdbConnection
    {
        private IAmazonSimpleDB _client;
        public SdbConnection(IAmazonSimpleDB client)
        {
            _client = client;
        }
        public async Task<IEnumerable<string>> GetDomainsAsync()
        {
            var result = new List<string>();
            ListDomainsResponse domains = null;
            do
            {
                domains = await _client.ListDomainsAsync(new ListDomainsRequest { MaxNumberOfDomains = 100, NextToken = domains?.NextToken });
                result.AddRange(domains.DomainNames);

            } while (!string.IsNullOrEmpty(domains.NextToken));
            return result;
        }
        public async Task<DomainMetadata> GetDomainMetadata(string domainName)
        {
            var resp = await _client.DomainMetadataAsync(new DomainMetadataRequest
            {
                DomainName = domainName
            });
            return new DomainMetadata
            {
                AttributeNameCount = resp.AttributeNameCount,
                AttributeNamesSizeBytes = resp.AttributeNamesSizeBytes,
                AttributeValueCount = resp.AttributeValueCount,
                AttributeValuesSizeBytes = resp.AttributeValuesSizeBytes,
                ItemCount = resp.ItemCount,
                ItemNamesSizeBytes = resp.ItemNamesSizeBytes
            };
        }
        public IDomain<T> GetDomain<T>(string domainName) where T : new()
        {
            return GetDomain<T>(domainName, new DomainConfiguration { Serializer = new DefaultAttributeSerializer() });
        }

        public IDomain<T> GetDomain<T>(string domainName, DomainConfiguration configuration) where T : new()
        {
            return new Domain<T>(_client, domainName, configuration);
        }
    }
}
