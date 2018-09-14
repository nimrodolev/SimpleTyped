using System.Collections.Generic;
using System.Linq;
using SimplyTyped.Core.Query;
using SimplyTyped.Utils;

namespace SimplyTyped.Query
{
    internal class InCondition : ICondition
    {
        private const string IN_TEMPLATE = "`{0}` IN ({1})";
        private string _memberName;
        private string[] _values;

        public InCondition(string memberName, string[] values)
        {
            _memberName = memberName;
            _values = values;
        }

        public string Condition => 
            string.Format(IN_TEMPLATE, _memberName, string.Join(",", _values.Select(v => $"'{QueryEncodingUtility.EncodeValue(v)}'"))); 
    }
}