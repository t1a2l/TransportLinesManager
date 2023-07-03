using Commons.Interfaces.Warehouse;
using TransportLinesManager.Data.Base.ConfigurationContainers;
using System.Xml.Serialization;

namespace TransportLinesManager.Data.DataContainers
{
    [XmlRoot("TransportStopDataContainer")]
    public class TLMStopDataContainer : ExtensionInterfaceIndexableImpl<TLMStopsConfiguration, TLMStopDataContainer>
    {
        public override string SaveId => "K45_TLM_TLMTransportStopDataContainer";
    }
}
