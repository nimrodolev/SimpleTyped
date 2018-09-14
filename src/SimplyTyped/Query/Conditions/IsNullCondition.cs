using System.Collections.Generic;
using System.Linq;
using SimplyTyped.Core.Query;

namespace SimplyTyped.Query
{
    internal class IsNullCondition : ICondition
    {
        private const string IS_NULL_TEMPLATE = "`{0}` IS NULL";
        private string _memberName;

        public IsNullCondition(string memberName)
        {
            _memberName = memberName;
        }

        public string Condition => string.Format(IS_NULL_TEMPLATE, _memberName); 
    }
}