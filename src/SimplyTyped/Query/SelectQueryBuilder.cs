using Dynamitey;
using SimplyTyped.Core.Query;
using SimplyTyped.Serialization;
using SimplyTyped.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SimplyTyped.Query
{
    internal class SelectQueryBuilder<T> : ISelectQueryBuilder<T>
    {
        private ExpressionParser _parser = new ExpressionParser();
        private PrimitiveAttributeSerializer _serializer = new PrimitiveAttributeSerializer();
        private string _domainName;
        private ClassMap<T> _classMap;

        internal SelectQueryBuilder(string domainName)
        {
            _domainName = domainName;
            _classMap = ClassMap.Get<T>();
        }

        public ISelectQuery<T> Empty() => new SelectQuery<T>(string.Empty);
        //TODO: NOT SAFE - can't make sure member is queryable
        public ISelectQuery<T> Where(Expression<Func<T, bool>> condition)
        {
            BinaryExpression bin = null;
            if ((bin = condition.Body as BinaryExpression) == null)
                throw new NotSupportedException($"Where can not be called with an expression of type {condition.Body.GetType().Name}");

            var parsed = _parser.ParseBinaryExpression(bin);
            return new SelectQuery<T>(parsed);
        }
        public ISelectQuery<T> In<TMember>(Expression<Func<T, TMember>> member, params TMember[] values)
        {
            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"In can not be called with an expression of type {member.Body.GetType().Name}");

            var memberName = _parser.ExtractMemberName(memExp);
            EnsureMemberQueryable(memberName);

            var valueStrings = values.Select(v => $"'{QueryEncodingUtility.EncodeValue(_serializer.Serialize(v))}'");
            return new SelectQuery<T>($"`{memberName}` IN ({string.Join(",", valueStrings)})");
        }
        public ISelectQuery<T> Like(Expression<Func<T, string>> member, string pattern)
        {
            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"Like can not be called with an expression of type {member.Body.GetType().Name}");

            var memberName = _parser.ExtractMemberName(memExp);
            EnsureMemberQueryable(memberName);

            return new SelectQuery<T>($"`{memberName}` LIKE '{QueryEncodingUtility.EncodeLikePattern(pattern)}'");
        }
        public ISelectQuery<T> Between<TMember>(Expression<Func<T, TMember>> member, TMember left, TMember right)
        {
            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"Between can not be called with an expression of type {member.Body.GetType().Name}");

            var memberName = _parser.ExtractMemberName(memExp);
            EnsureMemberQueryable(memberName);

            return new SelectQuery<T>($"`{memberName}` BETWEEN '{QueryEncodingUtility.EncodeValue(_serializer.Serialize(left))}' AND '{QueryEncodingUtility.EncodeValue(_serializer.Serialize(right))}'");
        }
        public ISelectQuery<T> IsNull<TMember>(Expression<Func<T, TMember>> member)
        {
            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"IsNull can not be called with an expression of type {member.Body.GetType().Name}");

            var memberName = _parser.ExtractMemberName(memExp);
            EnsureMemberQueryable(memberName);

            return new SelectQuery<T>($"`{memberName}` IS NULL");
        }

        public ISelectQuery<T> And(params ISelectQuery<T>[] queries)
        {
            return JoinQueries("AND", queries);
        }
        public ISelectQuery<T> Or(params ISelectQuery<T>[] queries)
        {
            return JoinQueries("OR", queries);
        }
        public ISelectQuery<T> Intersection(params ISelectQuery<T>[] queries)
        {
            return JoinQueries("INTERSECTION", queries);
        }
        public ISelectQuery<T> Not(ISelectQuery<T> query)
        {
            return new SelectQuery<T>($"NOT ({query.Selector})");
        }

        private ISelectQuery<T> JoinQueries(string joiner, params ISelectQuery<T>[] queries)
        {
            return new SelectQuery<T>(string.Join($" {joiner} ", queries.Select(q => q.Selector)));
        }

        private void EnsureMemberQueryable(string memberName)
        {
            var desc = _classMap.GetMember(memberName);
            if (!desc.IsQueryable)
                throw new NotSupportedException($"Member {memberName} can not be queried over.");
        }
    }
}
