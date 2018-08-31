using Amazon.Runtime;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using MessagePack;
using SimplyTyped;
using SimplyTyped.Core;
using SimplyTyped.MessagePack;
using SimplyTyped.Query;
using SimplyTyped.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace tester
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            var sdb = new AmazonSimpleDBClient("AKIAJSRJ44GD3Z3PQSKA", "cR5LxtQMuDF3cUHZJPO1f6cD1xKQuiY72md+jMoj", Amazon.RegionEndpoint.EUWest1);
            ISdbConnection connection = new SdbConnection(sdb);
            ClassMap.RegisterClassMap<Person>(cm =>
            {
                //cm.Member(p => p.Job).SetIgnore();
            });


            var domain = connection.GetDomain<Person>("person");  // Serializer = new SimpleDB.MessagePack.MessagePackSerializer()

            try
            {
                // var lst = new List<Person>();
                // for (int i = 0; i < 20; i++)
                // {
                //    var p = new Person
                //    {
                //        Id = i + 1,
                //        Age = 27 + (i * 2),
                //        Name = $"Person of grop {i % 4}",
                //        Title = i % 2 == 0 ? "Dr" : "Mr",
                //        Job = i % 4 == 0 ? null : new job
                //        {
                //            Name = $"Job number {i % 3}"
                //        }
                //    };
                //    lst.Add(p);
                // }
                // await domain.BatchPutAsync(lst);

                // var lst = new List<int>();
                // for (int i = 0; i < 20; i++)
                //    lst.Add(i + 1);
                // domain.BatchDeleteAsync(lst).Wait();

                var all = await domain.SelectAsync(domain.GetQueryBuilder().Empty(), false);
            }
            catch (Exception)
            {

            }
        }
    }

    [MessagePackObject]
    public class Person
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public string Name { get; set; }
        [Key(2)]
        public int Age { get; set; }
        [Key(3)]
        public string Title { get; set; }
        [Key(4)]
        public job Job { get; set; }

        [IgnoreMember]
        public string FormalName => $"{Title} {Name}";

        public override string ToString()
        {
            return $"[{Id}]: {FormalName}, Age {Age}";
        }
    }

    [MessagePackObject]
    public class job
    {
        [Key(0)]
        public string Name { get; set; }
    }

    class dummyClient : IAmazonSimpleDB
    {
        public IClientConfig Config => throw new NotImplementedException();

        public Task<BatchDeleteAttributesResponse> BatchDeleteAttributesAsync(BatchDeleteAttributesRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<BatchPutAttributesResponse> BatchPutAttributesAsync(BatchPutAttributesRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<CreateDomainResponse> CreateDomainAsync(CreateDomainRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<DeleteAttributesResponse> DeleteAttributesAsync(DeleteAttributesRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<DeleteDomainResponse> DeleteDomainAsync(DeleteDomainRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<DomainMetadataResponse> DomainMetadataAsync(DomainMetadataRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<GetAttributesResponse> GetAttributesAsync(GetAttributesRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<ListDomainsResponse> ListDomainsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<ListDomainsResponse> ListDomainsAsync(ListDomainsRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<PutAttributesResponse> PutAttributesAsync(PutAttributesRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<SelectResponse> SelectAsync(SelectRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
