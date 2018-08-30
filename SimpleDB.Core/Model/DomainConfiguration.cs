using SimpleDB.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDB.Core
{
    public class DomainConfiguration
    {
        /// <summary>
        /// An instance of ISerializer that will be used to serialize composite types. Simple types are handled internally.
        /// </summary>
        public ISerializer Serializer { get; set; }
        /// <summary>
        /// Determines whether the SimpleDB domain should be created on the fly in the event that it is missing
        /// </summary>
        public bool CreateDomainIfNotExists { get; set; }
        /// <summary>
        /// Determines what should be the behavior in case SimpleDB returns attributes with keys that don't match the known members.
        /// If false, an exception will be thrown.
        /// </summary>
        public bool IgnoreUnknownAttributes { get; set; }
    }
}
