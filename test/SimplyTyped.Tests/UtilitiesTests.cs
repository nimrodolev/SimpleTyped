using System.Collections.Generic;
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
        
        [Theory]
        [InlineData("")]
        [InlineData("The lazy fox")]
        public void Check_QueryEncodingUtility_EncodeValue_Single_Quote_Value_Should_Not_Change(string source)
        {
            var shouldBeEscapedCorrectly = QueryEncodingUtility.EncodeValue(source);        
            // assert
            Assert.Equal(source, shouldBeEscapedCorrectly);
        }
        [Theory]
        [InlineData("")]
        [InlineData("The lazy fox")]
        public void Check_QueryEncodingUtility_EncodeLikePattern_Percent_Sign_Value_Should_Not_Change(string source)
        {
            var shouldBeEscapedCorrectly = QueryEncodingUtility.EncodeLikePattern(source);        
            // assert
            Assert.Equal(source, shouldBeEscapedCorrectly);
        }

        [Theory]
        [InlineData("The 'lazy' fox")]
        [InlineData("'''''''")]
        public void Check_QueryEncodingUtility_EncodeValue_Single_Quote_Value_Should_Change(string source)
        {
            var shouldBeEscapedCorrectly = QueryEncodingUtility.EncodeValue(source);
            Check_All_Values_Encoded_Correctly(source, shouldBeEscapedCorrectly, "'", "''");
        }

        [Theory]
        [InlineData("The %lazy% fox")]
        [InlineData("%The lazy fox")]
        [InlineData("The lazy fox%")]
        [InlineData("%The lazy fox%")]
        [InlineData("%%%%")]
        public void Check_QueryEncodingUtility_EncodeLikePattern_Percent_Sign_Value_Should_Change(string source)
        {
            var shouldBeEscapedCorrectly = QueryEncodingUtility.EncodeLikePattern(source);
            Check_All_Values_Encoded_Correctly(source, shouldBeEscapedCorrectly, "%", "\\%");
        }

        private void Check_All_Values_Encoded_Correctly(string source, string escaped, string unEncodedValue, string encodedValue)
        {
            var added = 0;
            foreach (var idx in GetAllIndices(source, unEncodedValue))
            {
                var p = idx + added;
                Assert.True(escaped.Length >= p + encodedValue.Length);
                Assert.Equal(escaped.Substring(p, encodedValue.Length), encodedValue);
                added += encodedValue.Length - unEncodedValue.Length;
            }
        }

        private IEnumerable<int> GetAllIndices(string source, string lookup)
        {
            if (string.IsNullOrEmpty(source))
                yield break;
            for (int idx = 0; true; idx += lookup.Length)
            {
                idx = source.IndexOf(lookup, idx);
                if (idx == -1)
                    yield break;
                yield return idx;
            }
        }
        #endregion
    }
}