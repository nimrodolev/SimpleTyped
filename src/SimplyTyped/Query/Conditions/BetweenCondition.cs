using System.Collections.Generic;
using System.Linq;
using SimplyTyped.Core.Query;
using SimplyTyped.Utils;

namespace SimplyTyped.Query
{
    public class BetweenCondition : ICondition
    {
        private const string BETWEEN_TEMPLATE = "`{0}` BETWEEN '{1}' AND '{2}'";
        private string _memberName;
        private string _left;
        private string _right;

        public BetweenCondition(string memberName, string left, string right)
        {
            _memberName = memberName;
            _left = left;
            _right = right;
        }

        public string Condition => 
            string.Format(BETWEEN_TEMPLATE, _memberName, QueryEncodingUtility.EncodeValue(_left), QueryEncodingUtility.EncodeValue(_right)); 
    }
}