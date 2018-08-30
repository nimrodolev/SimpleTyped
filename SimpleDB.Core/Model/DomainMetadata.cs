using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDB.Core
{
    public class DomainMetadata
    {
        public int AttributeNameCount { get; set; }
        public long AttributeNamesSizeBytes { get; set; }
        public int AttributeValueCount { get; set; }
        public long AttributeValuesSizeBytes { get; set; }
        public int ItemCount { get; set; }
        public long ItemNamesSizeBytes { get; set; }
    }
}
