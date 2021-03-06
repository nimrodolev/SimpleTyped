﻿using SimplyTyped.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimplyTyped.Serialization
{
    internal class PrimitiveAttributeSerializer : ISerializer
    {
        private const string DATETIME_FORMAT = "yyyy-MM-ddThh:mm:ss.s";
        private static Dictionary<Type, Func<string, object>> _deserializationMapping = new Dictionary<Type, Func<string, object>>
        {
            [typeof(Byte)] = (s) => Byte.Parse(s),
            [typeof(SByte)] = (s) => (SByte)(Byte.Parse(s) - SByte.MaxValue - 1),
            [typeof(Int16)] = (s) => (Int16)(UInt16.Parse(s) - Int16.MaxValue - 1),
            [typeof(UInt16)] = (s) => UInt16.Parse(s),
            [typeof(Int32)] = (s) => (Int32)(UInt32.Parse(s) - Int32.MaxValue - 1),
            [typeof(UInt32)] = (s) => UInt32.Parse(s),
            [typeof(Int64)] = (s) => (Int64)(UInt64.Parse(s) - Int64.MaxValue - 1),
            [typeof(UInt64)] = (s) => UInt64.Parse(s),
            [typeof(Char)] = (s) => Char.Parse(s),
            [typeof(DateTime)] = (s) => DateTime.ParseExact(s, DATETIME_FORMAT, null),
            [typeof(String)] = (s) => s
        };
        private static Dictionary<Type, Func<object, string>> _serializationMapping = new Dictionary<Type, Func<object, string>>
        {
            [typeof(Byte)] = (o) => ((Byte)o).ToString("D3"),
            [typeof(SByte)] = (o) => ((SByte)o + SByte.MaxValue + 1).ToString("D3"),
            [typeof(Int16)] = (o) => ((Int16)o + Int16.MaxValue + 1).ToString("D5"),
            [typeof(UInt16)] = (o) => ((UInt16)o).ToString("D5"),
            [typeof(Int32)] = (o) => (((UInt32)(Int32)o) + Int32.MaxValue + 1).ToString("D10"),
            [typeof(UInt32)] = (o) => ((UInt32)o).ToString("D10"),
            [typeof(Int64)] = (o) => ((UInt64)(Int64)o + Int64.MaxValue + 1).ToString("D19"),
            [typeof(UInt64)] = (o) => ((UInt64)o).ToString("D20"),
            [typeof(DateTime)] = (o) => ((DateTime)o).ToString(DATETIME_FORMAT)
        };


        public static bool IsPrimitive(Type type)
        {
            return type.IsEnum || _deserializationMapping.ContainsKey(type);
        }
        public string Serialize(object obj)
        {
            var type = obj.GetType();
            EnsureEnumFunctionsReady(type);
            if (_serializationMapping.ContainsKey(type))
                return _serializationMapping[type].Invoke(obj);
            return obj.ToString();
        }
        public object Deserialize(string obj, Type type)
        {
            EnsureEnumFunctionsReady(type);
            if (_deserializationMapping.ContainsKey(type))
                return _deserializationMapping[type].Invoke(obj);
            else
                throw new NotImplementedException($"Type {type.Name} is not a primitive type");
        }

        private void EnsureEnumFunctionsReady(Type type)
        {
            if (!type.IsEnum)
                return;
            if(_deserializationMapping.ContainsKey(type) && _serializationMapping.ContainsKey(type))
                return;

            Type numberType = Enum.GetUnderlyingType(type);
            var serFunc = new Func<object, string>(o => {
                var asNumber = Convert.ChangeType(o, numberType);
                return _serializationMapping[numberType].Invoke(asNumber);
            });
            var desFunc = new Func<string, object>(s => 
            {
                var numValue = _deserializationMapping[numberType].Invoke(s);
                return Enum.ToObject(type, numValue);
            });

            _serializationMapping[type] = serFunc;
            _deserializationMapping[type] = desFunc;
        }
    }
}
