using Commons.Interfaces.Warehouse;
using TransportLinesManager.Data.Base.ConfigurationContainers;
using System.Xml.Serialization;

namespace TransportLinesManager.Data.DataContainers
{
    [XmlRoot("BuildingDataContainer")]
    public class TLMBuildingDataContainer : ExtensionInterfaceIndexableImpl<TLMBuildingsConfiguration, TLMBuildingDataContainer>
    {
        public override string SaveId => "TLM_TLMBuildingDataContainer";

        public override string LegacySaveId => "K45_TLM_TLMBuildingDataContainer";

    }
}
