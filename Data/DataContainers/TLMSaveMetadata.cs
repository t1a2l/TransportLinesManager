using System.Xml.Serialization;
using Commons.Interfaces.Warehouse;

namespace TransportLinesManager.Data.DataContainers
{
    [XmlRoot("TLM_SaveMetadata")]
    public sealed class TLMSaveMetadata : DataExtensionBase<TLMSaveMetadata>
    {
        public override string SaveId => "TransportLinesManager.SaveMetadata";

        [XmlAttribute("dataVersion")]
        public int DataVersion { get; set; }
    }
}
