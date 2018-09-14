using System;
using System.Collections.Generic;
using SimplyTyped.Core.Query;
using SimplyTyped.Utils;
namespace SimplyTyped.Query
{
    internal class SimpleOperatorCondition : ICondition
    {
        private static readonly Dictionary<SimpleOperator, string> OPERATORS = new Dictionary<SimpleOperator, string>
        {
            [SimpleOperator.IsEqualTo] = "=",
            [SimpleOperator.GreaterThan] = ">",
            [SimpleOperator.GreaterThanOrEqualTo] = ">=",
            [SimpleOperator.LessThan] = "<",
            [SimpleOperator.LessThanOrEqualTo] = "<="            
        };

        private const string TEMPLATE = "`{0}` {1} '{2}'";

        private string _operator;
        private string _member;
        private string _value;
        public SimpleOperatorCondition(SimpleOperator @operator, string member, string value)
        {
            if (!OPERATORS.TryGetValue(@operator, out _operator))
                throw new Exception($"Unknown operator: {@operator.ToString()}");
            _member = member;
            _value = value;
        }

        public string Condition => string.Format(TEMPLATE, _member, _operator, QueryEncodingUtility.EncodeValue(_value));
    }
}