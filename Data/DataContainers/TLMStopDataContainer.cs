using Klyte.Commons.Interfaces.Warehouse;
using Klyte.TransportLinesManager.Data.Base.ConfigurationContainers;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Data.DataContainers
{
    [XmlRoot("TransportStopDataContainer")]
    public class TLMStopDataContainer : ExtensionInterfaceIndexableImpl<TLMStopsConfiguration, TLMStopDataContainer>
    {
        public override string SaveId => "K45_TLM_TLMTransportStopDataContainer";
    }
}
