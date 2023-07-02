using Klyte.Commons.Interfaces.Warehouse;
using Klyte.TransportLinesManager.Data.Base.ConfigurationContainers;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Data.DataContainers
{
    [XmlRoot("BuildingDataContainer")]
    public class TLMBuildingDataContainer : ExtensionInterfaceIndexableImpl<TLMBuildingsConfiguration, TLMBuildingDataContainer>
    {
        public override string SaveId => "K45_TLM_TLMBuildingDataContainer";
    }
}
