using SimplyTyped.Core.Query;
using SimplyTyped.Serialization;
using SimplyTyped.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SimplyTyped.Query
{
    internal class ExpressionParser
    {
        private PrimitiveAttributeSerializer _serializer = new PrimitiveAttributeSerializer();
        private static readonly Dictionary<ExpressionType, string> OperationMap = new Dictionary<ExpressionType, string>
        {
            [ExpressionType.Equal] = "=",
            [ExpressionType.NotEqual] = "!=",
            [ExpressionType.AndAlso] = "AND",
            [ExpressionType.OrElse] = "OR",
            [ExpressionType.GreaterThan] = ">",
            [ExpressionType.GreaterThanOrEqual] = ">=",
            [ExpressionType.LessThan] = "<",
            [ExpressionType.LessThanOrEqual] = "<="
        };

        internal ICondition ParseBinaryExpression<T>(BinaryExpression exp)
        {
            var parsedExp = ParseExpression<T>(exp);
            return new RawCondition(parsedExp);
        }
        internal string ExtractMemberName(MemberExpression exp) => exp.Member.Name;

        private string ParseExpression<T>(BinaryExpression exp)
        {
            var operation = exp.NodeType;

            if (!OperationMap.ContainsKey(operation))
                throw new NotSupportedException($"Operation {operation} is not supported");

            var leftStr = ParseExpression<T>((dynamic)exp.Left);
            var rightStr = ParseExpression<T>((dynamic)exp.Right);

            return $"({leftStr} {OperationMap[operation]} {rightStr})";
        }
        private string ParseExpression<T>(MemberExpression exp)
        {
            var classMap = ClassMap.Get<T>();
            var memberName = ExtractMemberName(exp);
            return $"`{classMap.GetMember(memberName).AttributeName}`";
        }
        private string ParseExpression<T>(ConstantExpression exp)
        {
            return $"'{QueryEncodingUtility.EncodeValue(_serializer.Serialize(exp.Value))}'";
        }
        private string ParseExpression<T>(Expression exp) //unknown
        {
            throw new NotSupportedException($"Expression of type {exp.GetType().Name} are not supported");
        }
    }
}
