using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDB.Serialization
{
    public class MemberDescriptor
    {
        internal string AttributeName { get; set; }
        internal string MemberName { get; set; }
        internal Type MemberType { get; set; }
        internal bool IsQueryable { get; set; } = true;
        internal bool IsId { get; set; }
        internal bool IsIdCandidate { get; set; }
        internal bool IsIgnored { get; set; }

        public MemberDescriptor SetId()
        {
            IsIdCandidate = true;
            return this;
        }
        public MemberDescriptor SetIgnore()
        {
            IsIgnored = true;
            return this;
        }
        public MemberDescriptor SetNonQueraible()
        {
            IsQueryable = false;
            return this;
        }
        public MemberDescriptor SetNameRepresentation(string name)
        {
            AttributeName = name;
            return this;
        }


        //private object _getterSync = new object();
        //private object _setterSync = new object();
        //private Action<object, object> _setter;
        //private Func<object, object> _getter;

        //public Action<object, object> Setter;
        //public Func<object, object> Getter
        //{
        //    get
        //    {
        //        if (_getter == null)
        //        {
        //            lock (_getterSync)
        //            {
        //                if (_getter == null)
        //                {
        //                    _getter = get
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
