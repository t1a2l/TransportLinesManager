using System.Xml.Serialization;

namespace TransportLinesManager.Data.Base.ConfigurationContainers
{
    [XmlRoot("AssetConfiguration")]
    public class TLMAssetConfiguration
    {
        [XmlAttribute("capacity")]
        public int Capacity { get; set; }

    }
}
