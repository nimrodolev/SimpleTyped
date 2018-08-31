using System;

namespace SimplyTyped.Serialization
{
    public interface ISerializer
    {
        string Serialize(object obj);
        object Deserialize(string obj, Type type);
    }
}
