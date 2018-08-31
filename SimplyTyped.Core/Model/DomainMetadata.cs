using System;
using System.Collections.Generic;
using System.Text;

namespace SimplyTyped.Core
{
    public class DomainMetadata
    {
        /// <summary>
        /// The number of unique attribute names in the domain
        /// </summary>
        public int AttributeNameCount { get; set; }
        
        /// <summary>
        /// The total size of all unique attribute names in the domain, in bytes.
        /// </summary>
        public long AttributeNamesSizeBytes { get; set; }
        
        /// <summary>
        /// The number of all attribute name/value pairs in the domain.
        /// </summary>
        public int AttributeValueCount { get; set; }
        
        /// <summary>
        /// The total size of all attribute values in the domain, in bytes.
        /// </summary>
        public long AttributeValuesSizeBytes { get; set; }
        
        /// <summary>
        /// The number of all items in the domain.
        /// </summary>
        public int ItemCount { get; set; }
        
        /// <summary>
        /// The total size of all item names in the domain, in bytes.
        /// </summary>
        public long ItemNamesSizeBytes { get; set; }

        /// <summary>
        /// The data and time when metadata was calculated.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
