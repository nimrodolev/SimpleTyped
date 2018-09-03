using FastMember;
using SimplyTyped.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SimplyTyped.Serialization
{
    public class ClassMap
    {
        private static ConcurrentDictionary<Type, Lazy<ClassMap>> _cache = new ConcurrentDictionary<Type, Lazy<ClassMap>>();
        internal static ClassMap<T> Get<T>()
        {
            var type = typeof(T);
            return _cache.GetOrAdd(type, (k) => new Lazy<ClassMap>(() =>
            {
                var m = new ClassMap<T>();
                m.Validate(); //no user configuration, validate AutoMap after instantiation
                return m;
            })).Value as ClassMap<T>;
        }
        public static void RegisterClassMap<T>(Action<ClassMap<T>> mapper)
        {
            var type = typeof(T);
            var map = _cache.GetOrAdd(type, (k) => new Lazy<ClassMap>(() => InstantiateMap<T>())).Value as ClassMap<T>;
            mapper.Invoke(map);
            map.Validate(); //validate after user completed configuring mapping
        }
        private static ClassMap<T> InstantiateMap<T>()
        {
            return new ClassMap<T>();
        }
    }
    public class ClassMap<T> : ClassMap
    {
        private static readonly HashSet<string> ID_NAME_LOOKUPS = new HashSet<string>(new string[] { "id", "Id", "ID" });

        private ExpressionParser _parser = new ExpressionParser();
        private Dictionary<string, MemberDescriptor> _descriptors;
        private TypeAccessor _accessor;
        private MemberDescriptor _idMemberDescriptor;

        internal ClassMap()
        {
            _descriptors = new Dictionary<string, MemberDescriptor>();
            _accessor = TypeAccessor.Create(typeof(T));
            AutoMap();
        }


        public MemberDescriptor Member<TMember>(Expression<Func<T, TMember>> member)
        {
            MemberExpression memExp = null;
            if ((memExp = member.Body as MemberExpression) == null)
                throw new NotSupportedException($"Member can not be called with an expression of type {member.Body.GetType().Name}");

            var memberName = _parser.ExtractMemberName(memExp);
            return _descriptors[memberName];
        }

        internal void AutoMap()
        {
            var type = typeof(T);
            foreach (var member in _accessor.GetMembers())
            {
                var attrName = member.Name;
                if (member.IsDefined(typeof(AttributeKeyAttribute)))
                    attrName = ((AttributeKeyAttribute)member.GetAttribute(typeof(AttributeKeyAttribute), false)).Name;

                var desc = new MemberDescriptor
                {
                    AttributeName = attrName,
                    MemberName = member.Name,
                    MemberType = member.Type
                };

                if (!member.CanRead || !member.CanWrite)
                    desc.IsIgnored = true;

                if (!PrimitiveAttributeSerializer.IsPrimitive(member.Type))
                    desc.IsQueryable = false;

                if (member.IsDefined(typeof(IdAttribute)))
                {
                    if (!desc.IsQueryable)
                        throw new Exception("Id member must by Queryable");
                    if (desc.IsIgnored)
                        throw new Exception($"Id member must not be an ignored member. It must have a public getter and setter, and must not be decorated with {nameof(IgnoreAttribute)}");
                    if (_idMemberDescriptor != null)
                        throw new Exception($"{nameof(IdAttribute)} can not be set for more than one member.");
                    desc.IsId = true;
                }
                else if (_idMemberDescriptor == null && ID_NAME_LOOKUPS.Contains(member.Name) && desc.IsQueryable && !desc.IsIgnored)
                {
                    desc.IsIdCandidate = true;
                }

                _descriptors[desc.MemberName] = desc;
            }
        }
        internal void Validate()
        {
            var markedId = _descriptors.Values.FirstOrDefault(v => v.IsId);
            if (markedId != null)
                _idMemberDescriptor = markedId;
            else
            {
                var autoId = _descriptors.Values.FirstOrDefault(v => v.IsIdCandidate);
                if (autoId != null)
                    _idMemberDescriptor = autoId;
            }
            if (_idMemberDescriptor == null)
                throw new Exception($"No Id member found - no member was decorated with {nameof(IdAttribute)}, and no other member could be automatically detected");
        }
        internal MemberDescriptor GetMember(string member)
        {
            return _descriptors[member];
        }
        internal bool TryGetMemberByAtributeName(string member, out MemberDescriptor descriptor)
        {
            descriptor = _descriptors.Values.FirstOrDefault(a => a.AttributeName == member);
            return descriptor != null; //TODO : can be improved in terms of performance
        }
        internal MemberDescriptor GetIdMemberDescriptor()
        {
            return _idMemberDescriptor;
        }
        internal IEnumerable<string> ListMembers()
        {
            return _descriptors.Keys;
        }
        internal IEnumerable<MemberDescriptor> GetSerializationDescriptors()
        {
            return _descriptors.Values.Where(v => !v.IsIgnored);
        }
    }
}
