using System.Collections.Generic;
using System.Linq;
using SimplyTyped.Core.Query;

namespace SimplyTyped.Query
{
    public class JoiningCondition : ICondition
    {
        private List<ICondition> _children;
        private string _joiner;

        public JoiningCondition(string @operator, params ICondition[] conditions)
        {
            _joiner = @operator;
            _children = conditions.ToList();
        }

        public string Condition => string.Join(_joiner, _children.Select(c => $"({c.Condition})")); 
    }
}