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
    public class QueryBuilder<T> : IQueryBuilder<T>
    {
        private ExpressionParser _parser = new ExpressionParser();
        private PrimitiveAttributeSerializer _serializer = new PrimitiveAttributeSerializer();
        private ClassMap<T> _classMap;

        public QueryBuilder()
        {
            _classMap = ClassMap.Get<T>();
        }

        public IQuery<T> Empty() => new Query<T>(new EmptyCondition());
        //TODO: NOT SAFE - can't make sure member is queryable
        public IQuery<T> Where(Expression<Func<T, bool>> condition)
        {
            BinaryExpression bin = null;
            if ((bin = condition.Body as BinaryExpression) == null)
                throw new NotSupportedException($"Where can not be called with an expression of type {condition.Body.GetType().Name}");

            var parsed = _parser.ParseBinaryExpression<T>(bin);
            return new Query<T>(parsed);
        }
        public IQuery<T> In<TMember>(Expression<Func<T, TMember>> member, params TMember[] values)
        {
            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"In can not be called with an expression of type {member.Body.GetType().Name}");

            var memberName = _parser.ExtractMemberName(memExp);
            EnsureMemberQueryable(memberName);
            var attrName = GetMemberAttributeName(memberName);

            var valueStrings = values.Select(v => _serializer.Serialize(v)).ToArray();
            return new Query<T>(new InCondition(attrName, valueStrings));
        }
        public IQuery<T> Like(Expression<Func<T, string>> member, string pattern)
        {
            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"Like can not be called with an expression of type {member.Body.GetType().Name}");

            var memberName = _parser.ExtractMemberName(memExp);
            EnsureMemberQueryable(memberName);
            var attrName = GetMemberAttributeName(memberName);

            return new Query<T>(new LikeCondition(attrName, pattern));
        }
        public IQuery<T> Between<TMember>(Expression<Func<T, TMember>> member, TMember left, TMember right)
        {
            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"Between can not be called with an expression of type {member.Body.GetType().Name}");

            var memberName = _parser.ExtractMemberName(memExp);
            EnsureMemberQueryable(memberName);
            var attrName = GetMemberAttributeName(memberName);

            return new Query<T>(new BetweenCondition(attrName, _serializer.Serialize(left), _serializer.Serialize(right)));
        }
        public IQuery<T> IsNull<TMember>(Expression<Func<T, TMember>> member)
        {
            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"IsNull can not be called with an expression of type {member.Body.GetType().Name}");

            var memberName = _parser.ExtractMemberName(memExp);
            EnsureMemberQueryable(memberName);
            var attrName = GetMemberAttributeName(memberName);

            return new Query<T>(new IsNullCondition(attrName));
        }

        public IQuery<T> And(params IQuery<T>[] queries)
        {
            return JoinQueries("AND", queries);
        }
        public IQuery<T> Or(params IQuery<T>[] queries)
        {
            return JoinQueries("OR", queries);
        }
        public IQuery<T> Intersection(params IQuery<T>[] queries)
        {
            return JoinQueries("INTERSECTION", queries);
        }
        public IQuery<T> Not(IQuery<T> query)
        {
            return new Query<T>(new NotCondition(query.Condition));
        }
        private IQuery<T> JoinQueries(string joiner, params IQuery<T>[] queries)
        {
            return new Query<T>(new JoiningCondition(joiner, queries.Select(q => q.Condition).ToArray()));
        }

        private void EnsureMemberQueryable(string memberName)
        {
            var desc = _classMap.GetMember(memberName);
            if (!desc.IsQueryable)
                throw new NotSupportedException($"Member {memberName} can not be queried over.");
        }

        private string GetMemberAttributeName(string memberName)
        {
            var desc = _classMap.GetMember(memberName);
            return desc.AttributeName;
        }
    }
}
