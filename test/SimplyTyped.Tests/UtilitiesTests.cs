using SimplyTyped.Utils;
using Xunit;

namespace SimplyTyped.Tests
{
    public class UtilitiesTests
    {
        #region HttpUtility
        [Theory]
        [InlineData(System.Net.HttpStatusCode.Created)]
        [InlineData(System.Net.HttpStatusCode.OK)]
        [InlineData(System.Net.HttpStatusCode.NoContent)]
        [InlineData(System.Net.HttpStatusCode.Accepted)]
        public void Check_Http_Status_Code_Validation_Should_Return_True(System.Net.HttpStatusCode statusCode)
        {
            var success = HttpUtility.IsSuccessStatusCode(statusCode);
            Assert.True(success);
        }

        [Theory]
        [InlineData(System.Net.HttpStatusCode.Unauthorized)]
        [InlineData(System.Net.HttpStatusCode.TooManyRequests)]
        [InlineData(System.Net.HttpStatusCode.InternalServerError)]
        [InlineData(System.Net.HttpStatusCode.UnprocessableEntity)]
        [InlineData(System.Net.HttpStatusCode.ServiceUnavailable)]
        [InlineData(System.Net.HttpStatusCode.BadGateway)]
        [InlineData(System.Net.HttpStatusCode.GatewayTimeout)]
        public void Check_Http_Status_Code_Validation_Should_Return_False(System.Net.HttpStatusCode statusCode)
        {
            var success = HttpUtility.IsSuccessStatusCode(statusCode);
            Assert.False(success);
        }
        #endregion

        #region QueryEncodingUtility
        
        // [Theory]
        // [InlineData("He said, \"That's the ticket!")]
        // public void Check_Value_Encoding_Escape(string value)
        // {
        //     var escaped = QueryEncodingUtility.EncodeValue(value);
        //     Assert.Equal(value,escaped);
        // }
        #endregion
    }
}