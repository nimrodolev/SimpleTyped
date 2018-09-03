using System;

namespace SimplyTyped.Serialization
{
    public interface ISerializer
    {
        /// <summary>
        /// Converts an object into a string representation that can be stored on as a SimpleDB attribute value
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>A string containing the serialized representation of the object</returns>
        string Serialize(object obj);

        /// <summary>
        /// Converts an object into a string representation that can be stored on as a SimpleDB attribute value
        /// </summary>
        /// <param name="obj">The string containing the data to deserialize</param>
        /// <param name="type">The type that to deserialize the data to</param>
        /// <returns>An object of the type passed as the 'type' parameter</returns>
        object Deserialize(string obj, Type type);
    }
}
