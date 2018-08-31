using System;
using System.Collections.Generic;
using System.Text;

namespace SimplyTyped.Serialization
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
