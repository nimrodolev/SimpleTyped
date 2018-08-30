using ISerializer = SimpleDB.Serialization.ISerializer;
using System;
namespace SimpleDB.MessagePack
{
    public class MessagePackSerializer : ISerializer
    {
        public object Deserialize(string obj, Type type)
        {
            var buf = Convert.FromBase64String(obj);
            return global::MessagePack.MessagePackSerializer.Deserialize<object>(buf);
        }

        public string Serialize(object obj)
        {
            var buf = global::MessagePack.MessagePackSerializer.Serialize(obj);
            return Convert.ToBase64String(buf);
        }
    }
}
