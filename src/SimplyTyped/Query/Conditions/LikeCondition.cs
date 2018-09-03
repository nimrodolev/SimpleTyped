using System.Collections.Generic;
using System.Linq;
using SimplyTyped.Core.Query;
using SimplyTyped.Utils;

namespace SimplyTyped.Query
{
    public class LikeCondition : ICondition
    {
        private const string LIKE_TEMPLATE = "`{0}` LIKE '{1}'";
        private string _memberName;
        private string _pattern;

        public LikeCondition(string memberName, string pattern)
        {
            _memberName = memberName;
            _pattern = pattern;
        }

        public string Condition => 
            string.Format(LIKE_TEMPLATE, _memberName, QueryEncodingUtility.EncodeLikePattern(_pattern)); 
    }
}