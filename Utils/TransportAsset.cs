using System;
using System.Xml.Serialization;
using Commons.Utils.UtilitiesClasses;

namespace TransportLinesManager.Utils
{
	public struct TransportAsset
	{
        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("capacity")]
        public int capacity;

        [XmlElement("SpawnPercent")]
        public SimpleXmlDictionary<string, SpawnPercentEntry> spawn_percent;

        [XmlElement("Count")]
        public SimpleXmlDictionary<string, CountEntry> count;
	}

    public class SpawnPercentEntry
    {
        [XmlAttribute("value")]
        public int Value { get; set; }
    }

    public class CountEntry
    {
        [XmlAttribute("total")]
        public int TotalCount { get; set; }

        [XmlAttribute("used")]
        [Obsolete("Legacy serialized field. Runtime used count is no longer persisted.")]
        public int UsedCount { get; set; }
    }
}
