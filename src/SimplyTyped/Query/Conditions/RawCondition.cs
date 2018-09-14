using SimplyTyped.Core.Query;

namespace SimplyTyped.Query
{
    internal class RawCondition : ICondition
    {
        private string _condition;
        public RawCondition(string condition)
        {
            _condition = condition;
        }

        public string Condition => _condition;
    }
}