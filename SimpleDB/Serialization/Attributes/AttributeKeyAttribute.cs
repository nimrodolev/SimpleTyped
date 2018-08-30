using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDB.Serialization
{
    class AttributeKeyAttribute : Attribute
    {
        private string _name;
        internal string Name => _name;
        public AttributeKeyAttribute(string name)
        {
            _name = name;
        }
    }
}
