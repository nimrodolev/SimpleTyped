using SimplyTyped.Core.Query;
using SimplyTyped.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SimplyTyped.Query
{
    internal class SelectQuery<T> : ISelectQuery<T>
    {
        private static readonly int DEFAULT_LIMIT = 2500; //actually page size

        private ExpressionParser _parser = new ExpressionParser();
        private int _limit = DEFAULT_LIMIT;

        private string _selector;
        protected HashSet<string> _memberNames = ClassMap.Get<T>().GetSerializationDescriptors().Select(d => d.MemberName).ToHashSet();
        protected string[] _includeMembers;
        protected string[] _excludeMembers;
        protected Direction _orderDirection = Direction.ASC;
        protected string _orderByMember;

        protected SelectQuery() { }
        internal SelectQuery(string selector)
        {
            _selector = selector;
        }

        public ISelectQuery<T> Limit(int limit)
        {
            _limit = limit;
            return this;
        }
        public ISelectQuery<T> Include(params Expression<Func<T, object>>[] members)
        {
            var memberNames = ExtractMemberNames(members);
            return Include(memberNames);
        }
        public ISelectQuery<T> Exclude(params Expression<Func<T, object>>[] members)
        {
            var memberNames = ExtractMemberNames(members);
            return Exclude(memberNames);
        }
        public ISelectQuery<T> OrderBy<TMember>(Expression<Func<T, TMember>> member, Direction direction)
        {
            if (!string.IsNullOrEmpty(_orderByMember))
                throw new Exception($"Only one call to {nameof(OrderBy)} is allowed. Query was already set to oder by {_orderByMember}");

            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"{nameof(OrderBy)} can not be called with an expression of type {member.Body.GetType().Name}");
            _orderByMember = _parser.ExtractMemberName(memExp);
            _orderDirection = direction;
            return this;
        }
        public ISelectQuery<T> Include(params string[] members)
        {
            if (_excludeMembers != null)
                throw new Exception($"{nameof(Include)} and {nameof(Exclude)} can not be used in the same query, {nameof(Exclude)} was already used.");

            foreach (var member in members)
                if (!_memberNames.Contains(member))
                    throw new Exception($"Member {member} is unknown.");

            _includeMembers = members;
            return this;
        }
        public ISelectQuery<T> Exclude(params string[] members)
        {
            if (_includeMembers != null)
                throw new Exception($"{nameof(Include)} and {nameof(Exclude)} can not be used in the same query, {nameof(Include)} was already used.");

            foreach (var member in members)
                if (!_memberNames.Contains(member))
                    throw new Exception($"Member {member} is unknown.");

            _excludeMembers = members;
            return this;
        }
        public string Selector => _selector;

        public override string ToString()
        {
            return Assemble("<DOMAIN>", false);
        }
        public string Assemble(string domainName, bool isCount)
        {
            var parts = new List<string>();
            parts.Add(GetSelectClause(isCount));
            parts.Add($"FROM `{domainName}`");
            if (!string.IsNullOrEmpty(_selector))
                parts.Add($"WHERE {_selector}");
            if (!string.IsNullOrEmpty(_orderByMember))
                parts.Add($"ORDER BY `{_orderByMember}` {_orderDirection.ToString()}");
            if (!isCount) //omit "LIMIT" if this is a count query
                parts.Add($"LIMIT {_limit}");

            return string.Join($" {Environment.NewLine}", parts);
        }
        private string GetSelectClause(bool isCount)
        {
            if (isCount)
                return "SELECT COUNT(*)";
            else if (_includeMembers != null)
                return $"SELECT {string.Join(", ", _includeMembers.Select(m => $"`{m}`"))}";
            else if (_excludeMembers != null)
                return $"SELECT {string.Join(", ", _memberNames.Except(_excludeMembers).Select(m => $"`{m}`"))}";
            else
                return "SELECT *";
        }
        private string[] ExtractMemberNames(params Expression<Func<T, object>>[] members)
        {
            var res = new List<string>();
            foreach (var exp in members)
            {
                MemberExpression memExp = null;
                if ((memExp = exp.Body as MemberExpression) == null)
                    throw new NotSupportedException($"Expression of type {exp.Body.GetType().Name} are not supported");

                var memberName = _parser.ExtractMemberName(memExp);
                res.Add(memberName);
            }
            return res.ToArray();
        }
    }
}
