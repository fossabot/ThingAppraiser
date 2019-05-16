﻿using System.Xml.Serialization;
using System.Xml.Linq;

namespace ThingAppraiser.Data.Configuration
{
    public class OutputManagerConfig
    {
        [XmlAttribute(DataType = "string")]
        public string DefaultOutStorageName { get; set; }

        [XmlAnyElement(Name = "OutputManagerParameters")]
        public XElement[] OutputManagerParameters { get; set; }

        [XmlAnyElement(Name = "Outputters")]
        public XElement[] Outputters { get; set; }


        public OutputManagerConfig()
        {
        }
    }
}
