using Polenter.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SimpleDB.Serialization
{
    internal class DefaultAttributeSerializer : ISerializer
    {
        private SharpSerializer _complexSerializer = new SharpSerializer(new SharpSerializerBinarySettings
        {
            Mode = BinarySerializationMode.SizeOptimized
        });

        public object Deserialize(string obj, Type type)
        {
            var bytes = Convert.FromBase64String(obj);
            var mem = new MemoryStream(bytes);
            return _complexSerializer.Deserialize(mem);
        }

        public string Serialize(object obj)
        {
            var mem = new MemoryStream();
            _complexSerializer.Serialize(obj, mem);
            return Convert.ToBase64String(mem.ToArray());
        }
    }
}
