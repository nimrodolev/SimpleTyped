using SimplyTyped.Core.Query;

namespace SimplyTyped.Query
{
    internal class EmptyCondition : ICondition
    {
        public string Condition => string.Empty;
    }
}