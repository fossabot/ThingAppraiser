﻿using System.Xml.Linq;
using System.Xml.Serialization;

namespace ThingAppraiser.Data.Configuration
{
    public class CrawlersManagerConfig
    {
        [XmlAttribute(DataType = "boolean")]
        public bool CrawlersOutputFlag { get; set; }

        [XmlAnyElement(Name = "CrawlersManagerParameters")]
        public XElement[] CrawlersManagerParameters { get; set; }

        [XmlAnyElement(Name = "Crawlers")]
        public XElement[] Crawlers { get; set; }


        public CrawlersManagerConfig()
        {
        }
    }
}
