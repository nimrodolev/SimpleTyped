using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Moq;
using Xunit;

namespace SimplyTyped.Tests
{
    public class ConnectionTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(15)]
        public async Task Connection_ListDomainsAsync_AllPagesRead(int pages)
        {
            //arrange
            int numOfPagesReturned = 1;
            var mock = GetSimpleDbClientMock();
            mock.Setup(x => x.ListDomainsAsync(It.IsAny<ListDomainsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => {
                    if (numOfPagesReturned == pages)
                        return CreateDummyListDomainsResponse(null);    
                    return CreateDummyListDomainsResponse("p" + ++numOfPagesReturned);
                });

            //act
            var connection = new Connection(mock.Object);
            var domains = await connection.ListDomainsAsync();

            //asset
            mock.Verify(a => a.ListDomainsAsync(It.IsAny<ListDomainsRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(pages));
        }

        [Fact]
        public async Task Connection_DomainMetadataAsync_ClientCalledOnce()
        {
            //arrange
            var mock = GetSimpleDbClientMock();
            mock.Setup(x => x.DomainMetadataAsync(It.IsAny<DomainMetadataRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DomainMetadataResponse(){});

            //act
            var connection = new Connection(mock.Object);
            var domains = await connection.GetDomainMetadata("test");

            //asset
            mock.Verify(x => x.DomainMetadataAsync(It.IsAny<DomainMetadataRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task Connection_DomainMetadataAsync_AllPropertiesEqual()
        {
            //arrange
            var now = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
            var mock = GetSimpleDbClientMock();
            mock.Setup(x => x.DomainMetadataAsync(It.IsAny<DomainMetadataRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DomainMetadataResponse(){
                    AttributeNameCount = 1,
                    AttributeNamesSizeBytes = 2,
                    AttributeValueCount = 3,
                    AttributeValuesSizeBytes = 4,
                    ItemCount = 5,
                    ItemNamesSizeBytes = 6,
                    Timestamp = (int)(now - new DateTime(1970, 1, 1, 0, 0 , 0, DateTimeKind.Utc)).TotalSeconds
                });

            //act
            var connection = new Connection(mock.Object);
            var metadata = await connection.GetDomainMetadata("test");

            //asset
            Assert.Equal(1, metadata.AttributeNameCount);
            Assert.Equal(2, metadata.AttributeNamesSizeBytes);
            Assert.Equal(3, metadata.AttributeValueCount);
            Assert.Equal(4, metadata.AttributeValuesSizeBytes);
            Assert.Equal(5, metadata.ItemCount);
            Assert.Equal(6, metadata.ItemNamesSizeBytes);
            Assert.Equal(now, metadata.Timestamp);
        }



        #region private setups
        private Mock<IAmazonSimpleDB> GetSimpleDbClientMock()
        {
            var mock =  Mock.Get<IAmazonSimpleDB>(Mock.Of<IAmazonSimpleDB>());
            return mock;
        }
        private ListDomainsResponse CreateDummyListDomainsResponse(string nextPageToken)
        {
            return new ListDomainsResponse()
            {
                DomainNames = new List<string> { "domain1", "domain2", "domain3" },
                HttpStatusCode = HttpStatusCode.OK,
                NextToken = nextPageToken
            };
        }
        #endregion

    }
    class TestEntry
    {
        public string Id { get; set; }
        public int MyProperty { get; set; }
    }
}