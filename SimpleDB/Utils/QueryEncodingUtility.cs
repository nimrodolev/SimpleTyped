using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDB.Utils
{
    internal static class QueryEncodingUtility
    {
        internal static string EncodeValue(string value)
        {
            return value.Replace("'", "''");
        }
        internal static string EncodeLikePattern(string pattern)
        {
            return EncodeValue(pattern).Replace("%", "%%");
        }
    }
}
