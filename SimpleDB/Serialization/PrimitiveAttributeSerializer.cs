using SimpleDB.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDB
{
    internal class PrimitiveAttributeSerializer : ISerializer
    {
        private const string DATETIME_FORMAT = "YYYY-MM-DDThh:mm:ss.sTZD";
        private static Dictionary<Type, Func<string, object>> _deserializationMapping = new Dictionary<Type, Func<string, object>>
        {
            [typeof(Boolean)] = (s) => Boolean.Parse(s),
            [typeof(Byte)] = (s) => Byte.Parse(s),
            [typeof(SByte)] = (s) => SByte.Parse(s),
            [typeof(Int16)] = (s) => Int16.Parse(s),
            [typeof(UInt16)] = (s) => UInt16.Parse(s),
            [typeof(Int32)] = (s) => Int32.Parse(s),
            [typeof(UInt32)] = (s) => UInt32.Parse(s),
            [typeof(Int64)] = (s) => Int64.Parse(s),
            [typeof(UInt64)] = (s) => UInt64.Parse(s),
            [typeof(Char)] = (s) => Char.Parse(s),
            [typeof(DateTime)] = (s) => DateTime.ParseExact(s, DATETIME_FORMAT, null),
            [typeof(String)] = (s) => s
        };
        private static Dictionary<Type, Func<object, string>> _serializationMapping = new Dictionary<Type, Func<object, string>>
        {
            [typeof(Byte)] = (o) => ((Byte)o).ToString("D3"),
            [typeof(SByte)] = (o) => ((SByte)o).ToString("D3"),
            [typeof(Int16)] = (o) => ((Int16)o).ToString("D5"),
            [typeof(UInt16)] = (o) => ((UInt16)o).ToString("D5"),
            [typeof(Int32)] = (o) => ((Int32)o).ToString("D10"),
            [typeof(UInt32)] = (o) => ((UInt32)o).ToString("D10"),
            [typeof(Int64)] = (o) => ((Int64)o).ToString("D19"),
            [typeof(UInt64)] = (o) => ((UInt64)o).ToString("D20"),
            [typeof(DateTime)] = (o) => ((DateTime)o).ToString(DATETIME_FORMAT),
        };

        public static bool IsPrimitive(Type type)
        {
            return _deserializationMapping.ContainsKey(type);
        }
        public string Serialize(object obj)
        {
            var type = obj.GetType();
            if (_serializationMapping.ContainsKey(type))
                return _serializationMapping[type].Invoke(obj);
            return obj.ToString();
        }
        public object Deserialize(string obj, Type type)
        {
            if (_deserializationMapping.ContainsKey(type))
                return _deserializationMapping[type].Invoke(obj);
            else
                throw new NotImplementedException($"Type {type.Name} is not a primitive type");
        }
    }
}
