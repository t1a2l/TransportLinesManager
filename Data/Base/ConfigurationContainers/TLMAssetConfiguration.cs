using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Data.Base.ConfigurationContainers
{
    [XmlRoot("AssetConfiguration")]
    public class TLMAssetConfiguration
    {
        [XmlAttribute("capacity")]
        public int Capacity { get; set; }

    }
}
