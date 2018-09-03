using SimplyTyped.Core.Query;

namespace SimplyTyped.Query
{
    public class EmptyCondition : ICondition
    {
        public string Condition => string.Empty;
    }
}