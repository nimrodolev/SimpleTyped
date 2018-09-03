using Amazon.SimpleDB;
using FastMember;
using SimplyTyped.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Amazon.SimpleDB.Model;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using SimplyTyped.Core;
using SimplyTyped.Core.Query;
using SimplyTyped.Query;
using SimplyTyped.Utils;

namespace SimplyTyped
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

        public async Task PutAsync(T item)
        {
            await PutAsync(item, true);
        }

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

            if (!HttpUtility.IsSuccessStatusCode(resp.HttpStatusCode))
                throw new Exception("Something went wrong");
        }


        public async Task<T> GetByIdAsync<TMember>(TMember id)
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

            if (!HttpUtility.IsSuccessStatusCode(resp.HttpStatusCode))
                throw new Exception("Something went wrong");
            if (resp.Attributes.Count == 0)
                throw new KeyNotFoundException($"No item with id {id} was found");

            return Deserialize(resp.Attributes);
        }

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

            if (!HttpUtility.IsSuccessStatusCode(resp.HttpStatusCode))
                throw new Exception("Something went wrong");
        }

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

        public async Task<IEnumerable<T>> SelectAsync(ISelectQuery<T> query, bool consistantRead)
        {
            var queryStr = query.Assemble(_domainName, false);
            var items = await RawSelectAsync(queryStr, consistantRead);
            var results = items.Select(i => Deserialize(i.Attributes)).ToArray();
            return results;
        }

        public async Task<long> SelectCountAsync(ISelectQuery<T> query, bool consistantRead)
        {
            var queryStr = query.Assemble(_domainName, true);
            var items = await RawSelectAsync(queryStr, consistantRead);
            var result = long.Parse(items.First().Attributes.First().Value);
            return result;
        }

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

                if (!HttpUtility.IsSuccessStatusCode(resp.HttpStatusCode))
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

            if (!HttpUtility.IsSuccessStatusCode(resp.HttpStatusCode))
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

            if (!HttpUtility.IsSuccessStatusCode(resp.HttpStatusCode))
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
