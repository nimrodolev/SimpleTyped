using Amazon.SimpleDB;
using FastMember;
using SimpleDB.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Amazon.SimpleDB.Model;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using SimpleDB.Core;
using SimpleDB.Core.Query;
using SimpleDB.Query;

namespace SimpleDB
{
    public class Domain<T> : IDomain<T> where T : new()
    {
        private static readonly int MAX_BATCH_SIZE = 25;

        private IAmazonSimpleDB _client;
        private TypeAccessor _accessor = TypeAccessor.Create(typeof(T));
        private PrimitiveAttributeSerializer _primitiveSerializer = new PrimitiveAttributeSerializer();
        private bool _domainExists = false;
        private SemaphoreSlim _sync = new SemaphoreSlim(1, 1);

        private ISerializer _serializer;
        private string _domainName;
        private bool _shouldCreateDomain;
        private bool _ignoreExtraAttributes;
        private ClassMap<T> _classMap;

        internal Domain(IAmazonSimpleDB client, string domainName, DomainConfiguration config)
        {
            _client = client;
            _serializer = config.Serializer;
            _domainName = domainName;
            _shouldCreateDomain = config.CreateDomainIfNotExists;
            _ignoreExtraAttributes = config.IgnoreUnknownAttributes;
            _classMap = ClassMap.Get<T>();
        }

        /// <summary>
        /// Adds an item in the domain. Throws an exception if the item already exists.
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <returns></returns>
        public async Task PutAsync(T item)
        {
            await PutAsync(item, true);
        }

        /// <summary>
        /// Adds or replaces an item in the domain.
        /// </summary>
        /// <param name="item">The item to be added</param>
        /// <param name="throwIfExists">A flag indicating whether an exception should be raised in case the item already exists. If false, existing values will be overridden</param>
        /// <returns></returns>
        public async Task PutAsync(T item, bool throwIfExists)
        {
            await EnsureDomainCreated();
            string id = ValidateAndExtractId(item);
            var idDesc = _classMap.GetIdMemberDescriptor();
            var resp = await _client.PutAttributesAsync(new PutAttributesRequest
            {
                DomainName = _domainName,
                ItemName = id, //ensured to be a primitive earlier
                Attributes = Serialize(item).Select(a => new ReplaceableAttribute() { Name = a.Name, Value = a.Value }).ToList(),
                Expected = throwIfExists ? new UpdateCondition { Name = idDesc.AttributeName, Exists = false } : null
            });

            if (!IsSucessStatusCode(resp.HttpStatusCode))
                throw new Exception("Something went wrong");
        }

        /// <summary>
        /// Retrives one item by its Id. Will throw an exception if the items is not found.
        /// </summary>
        /// <param name="id">The id of the item to fetch. Must be of the item's Id member type.</param>
        /// <returns>The item request, of type T</returns>
        public async Task<T> GetByIdAsyc<TMember>(TMember id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            ValidateIdObject(id);

            await EnsureDomainCreated();
            var resp = await _client.GetAttributesAsync(new GetAttributesRequest
            {
                DomainName = _domainName,
                ItemName = _primitiveSerializer.Serialize(id)
            });

            if (!IsSucessStatusCode(resp.HttpStatusCode))
                throw new Exception("Something went wrong");
            if (resp.Attributes.Count == 0)
                throw new KeyNotFoundException($"No item with id {id} was found");

            return Deserialize(resp.Attributes);
        }

        /// <summary>
        /// Deletes one item by its Id
        /// </summary>
        /// <param name="id">The id of the item to delete. Must be of the item's Id member type.</param>
        /// <returns></returns>
        public async Task DeleteOneAsync<TMember>(TMember id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            ValidateIdObject(id);

            await EnsureDomainCreated();
            var resp = await _client.DeleteAttributesAsync(new DeleteAttributesRequest
            {
                DomainName = _domainName,
                ItemName = _primitiveSerializer.Serialize(id)
            });

            if (!IsSucessStatusCode(resp.HttpStatusCode))
                throw new Exception("Something went wrong");
        }

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
        public async Task BatchPutAsync(IEnumerable<T> items)
        {
            //start by serializing everything to make sure there are not errors
            List<ReplaceableItem> serialized = items.Select(i =>
            {
                string id = ValidateAndExtractId(i);
                return new ReplaceableItem
                {
                    Name = id,
                    Attributes = Serialize(i).Select(a => new ReplaceableAttribute { Name = a.Name, Value = a.Value }).ToList()
                };
            }).ToList();
            var batch = new List<ReplaceableItem>(MAX_BATCH_SIZE);
            foreach (var item in serialized)
            {
                batch.Add(item);
                if (batch.Count < MAX_BATCH_SIZE)
                    continue;

                await InternalBatchPutAsync(batch);
                batch = new List<ReplaceableItem>(MAX_BATCH_SIZE);
            }
            if (batch.Count > 0)
                await InternalBatchPutAsync(batch);
        }

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
        public async Task BatchDeleteAsync<TMember>(IEnumerable<TMember> ids)
        {
            var serialized = new List<DeletableItem>();
            foreach (var id in ids)
            {
                ValidateIdObject(id);
                string idStr = _primitiveSerializer.Serialize(id);
                serialized.Add(new DeletableItem { Name = idStr });
            }

            var batch = new List<DeletableItem>();
            foreach (var item in serialized)
            {
                batch.Add(item);
                if (batch.Count < MAX_BATCH_SIZE)
                    continue;

                await InternalBatchDeleteAsync(batch);
                batch = new List<DeletableItem>(MAX_BATCH_SIZE);
            }
            if (batch.Count > 0)
                await InternalBatchDeleteAsync(batch);
        }

        /// <summary>
        /// Issues a Select request for the given query. All results are fetched, even if they are paged.
        /// </summary>
        /// <param name="query">The query object</param>
        /// <param name="consistantRead">Sets the underlaying consistency option for the issued Select request, 
        /// as described in the AWS SimpleDB documentation found at https://docs.aws.amazon.com/AmazonSimpleDB/latest/DeveloperGuide/ConsistencySummary.html</param>
        /// <returns>The Select request's results, deserialized into instances of T</returns>
        public async Task<IEnumerable<T>> SelectAsync(ISelectQuery<T> query, bool consistantRead)
        {
            var queryStr = query.Assemble(_domainName, false);
            var items = await RawSelectAsync(queryStr, consistantRead);
            var results = items.Select(i => Deserialize(i.Attributes)).ToArray();
            return results;
        }

        /// <summary>
        /// Issues a gived Select request as a count query.
        /// </summary>
        /// <param name="query">The query object</param>
        /// <param name="consistantRead">Sets the underlaying consistency option for the issued Select request, 
        /// as described in the AWS SimpleDB documentation found at https://docs.aws.amazon.com/AmazonSimpleDB/latest/DeveloperGuide/ConsistencySummary.html</param>
        /// <returns>A long value representing the amount of results for the given Select query</returns>
        public async Task<long> SelectCountAsync(ISelectQuery<T> query, bool consistantRead)
        {
            var queryStr = query.Assemble(_domainName, true);
            var items = await RawSelectAsync(queryStr, consistantRead);
            var result = long.Parse(items.First().Attributes.First().Value);
            return result;
        }

        /// <summary>
        /// Instansiates and returns an SelectQueryBuilder<T> that can be used to comprise a Select query to fetch data using SelectAsync and SelectCountAsync
        /// </summary>
        /// <returns>An instance of SelectQueryBuilder<T></returns>
        public ISelectQueryBuilder<T> GetQueryBuilder()
        {
            return new SelectQueryBuilder<T>(_domainName);
        }


        private async Task<IEnumerable<Item>> RawSelectAsync(string query, bool consistantRead)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentNullException(nameof(query));
            var results = new List<Item>();
            SelectResponse resp = null;
            do
            {
                resp = await _client.SelectAsync(new SelectRequest
                {
                    ConsistentRead = consistantRead,
                    SelectExpression = query,
                    NextToken = resp?.NextToken
                });

                if (!IsSucessStatusCode(resp.HttpStatusCode))
                    throw new Exception("Something went wrong");

                results.AddRange(resp.Items);

            } while (!string.IsNullOrEmpty(resp.NextToken));

            return results;
        }
        private async Task InternalBatchDeleteAsync(List<DeletableItem> itemIds)
        {
            if (itemIds.Count > MAX_BATCH_SIZE)
                throw new Exception("Batch size may not exceed 25");

            await EnsureDomainCreated();

            var resp = await _client.BatchDeleteAttributesAsync(new BatchDeleteAttributesRequest
            {
                DomainName = _domainName,
                Items = itemIds
            });

            if (!IsSucessStatusCode(resp.HttpStatusCode))
                throw new Exception("Something went wrong");
        }
        private async Task InternalBatchPutAsync(List<ReplaceableItem> items)
        {
            if (items.Count > MAX_BATCH_SIZE)
                throw new Exception("Batch size may not exceed 25");

            await EnsureDomainCreated();

            var resp = await _client.BatchPutAttributesAsync(new BatchPutAttributesRequest
            {
                DomainName = _domainName,
                Items = items.ToList()
            });

            if (!IsSucessStatusCode(resp.HttpStatusCode))
                throw new Exception("Something went wrong");
        }
        private T Deserialize(IEnumerable<Amazon.SimpleDB.Model.Attribute> attrs)
        {
            var empty = Activator.CreateInstance(typeof(T));
            foreach (var attr in attrs)
            {
                var key = attr.Name;
                var val = attr.Value;

                MemberDescriptor memberDescriptor = null;
                if (!_classMap.TryGetMemberByAtributeName(key, out memberDescriptor))
                {
                    if (_ignoreExtraAttributes)
                        throw new Exception($"Attribute with key {key} can not be mapped to any of the members on class {typeof(T).Name}.");
                    continue;
                }
                
                var useSer = SelectSerializer(memberDescriptor.MemberType);
                var obj = useSer.Deserialize(val, memberDescriptor.MemberType);
                _accessor[empty, memberDescriptor.MemberName] = obj;
            }

            return (T)empty;
        }
        private IEnumerable<Amazon.SimpleDB.Model.Attribute> Serialize(T item)
        {
            var res = new List<Amazon.SimpleDB.Model.Attribute>();
            foreach (var member in _classMap.GetSerializationDescriptors())
            {
                var memberValue = _accessor[item, member.MemberName];
                if (memberValue == null)
                    continue;

                var useSer = SelectSerializer(member.MemberType);
                res.Add(new Amazon.SimpleDB.Model.Attribute
                {
                    Name = member.AttributeName,
                    Value = useSer.Serialize(memberValue)
                });
            }
            return res;
        }
        private ISerializer SelectSerializer(Type type)
        {
            if (PrimitiveAttributeSerializer.IsPrimitive(type))
                return _primitiveSerializer;
            return _serializer;
        }
        private bool IsSucessStatusCode(HttpStatusCode code)
        {
            var n = (int)code;
            return n >= 200 && n < 300;
        }
        private async Task EnsureDomainCreated()
        {
            if (_domainExists)
                return;
            try
            {
                await _sync.WaitAsync();
                if (_domainExists)
                    return;

                ListDomainsResponse domains = null;
                do
                {
                    domains = await _client.ListDomainsAsync(new ListDomainsRequest { MaxNumberOfDomains = 100, NextToken = domains?.NextToken });
                    foreach (var domain in domains.DomainNames)
                    {
                        if (domain == _domainName)
                            return;
                    }
                } while (!string.IsNullOrEmpty(domains.NextToken));

                if (!_shouldCreateDomain)
                    throw new Exception($"Domain {_domainName} does not exist, and the configuration provided instructs not to create one");

                await _client.CreateDomainAsync(new CreateDomainRequest
                {
                    DomainName = _domainName
                });
            }
            finally
            {
                _sync.Release();
            }
        }
        private void ValidateIdObject<TMember>(TMember id)
        {
            var idDesc = _classMap.GetIdMemberDescriptor();
            if (typeof(TMember) != idDesc.MemberType)
                throw new Exception($"Parameter {nameof(id)} of type {id.GetType().Name} does not match Id member's type ({idDesc.MemberType.Name})");
        }
        private string ValidateAndExtractId(T item)
        {
            var idDesc = _classMap.GetIdMemberDescriptor();
            var idVal = _accessor[item, idDesc.MemberName];
            if (idVal == null)
                throw new Exception("Id member's value can not be null");

            return  _primitiveSerializer.Serialize(idVal);
        }
    }
}
