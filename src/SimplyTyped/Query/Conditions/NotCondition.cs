using System.Collections.Generic;
using System.Linq;
using SimplyTyped.Core.Query;

namespace SimplyTyped.Query
{
    public class NotCondition : ICondition
    {
        private const string NOT_TEMPLATE = "NOT ({0})";
        private ICondition _condition;

        public NotCondition(ICondition condition)
        {
            _condition = condition;
        }

        public string Condition => string.Format(NOT_TEMPLATE, _condition.Condition); 
    }
}