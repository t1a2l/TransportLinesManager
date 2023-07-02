using ICities;
using Klyte.Commons.Interfaces.Warehouse;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Data.Base;
using Klyte.TransportLinesManager.Data.Base.ConfigurationContainers;
using Klyte.TransportLinesManager.Data.TsdImplementations;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.TransportLinesManager.Data.DataContainers
{
    [XmlRoot("TransportTypeDataContainer")]
    public class TLMTransportTypeDataContainer : ExtensionInterfaceIndexableImpl<TLMTransportTypeConfigurations, TLMTransportTypeDataContainer>
    {
        public override string SaveId => "K45_TLM_TLMTransportTypeDataContainer";

        private static readonly Dictionary<string, TransportSystemDefinition> legacyLinks = new Dictionary<string, TransportSystemDefinition>
        {
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorBus"] = TransportSystemDefinitionType.BUS,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorBlp"] = TransportSystemDefinitionType.BLIMP,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionEvcBus"] = TransportSystemDefinitionType.EVAC_BUS,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorFer"] = TransportSystemDefinitionType.FERRY,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorMet"] = TransportSystemDefinitionType.METRO,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorMnr"] = TransportSystemDefinitionType.MONORAIL,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorPln"] = TransportSystemDefinitionType.PLANE,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorShp"] = TransportSystemDefinitionType.SHIP,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrn"] = TransportSystemDefinitionType.TRAIN,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrm"] = TransportSystemDefinitionType.TRAM,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionTouBus"] = TransportSystemDefinitionType.TOUR_BUS,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionTouPed"] = TransportSystemDefinitionType.TOUR_PED,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrl"] = TransportSystemDefinitionType.TROLLEY,
            ["K45_TLM_Klyte.TransportLinesManager.Extensions.TLMTransportTypeExtensionNorHel"] = TransportSystemDefinitionType.HELICOPTER,
        };

        public void RefreshCapacities()
        {
            var items = m_cachedList.ToList();
            foreach (var tsdItem in items)
            {
                tsdItem.Value.InitCapacitiesInAssets();
            }
        }
        public override void LoadDefaults(ISerializableData serializableData)
        {
            base.LoadDefaults(serializableData);
            foreach (var entry in legacyLinks)
            {
                var legacyData = TLMTransportTypeConfigurations.GetLoadData(serializableData, entry.Key);
                if (legacyData != null)
                {
                    LogUtils.DoWarnLog($"Loaded transport type extension from legacy: {entry.Key} to {entry.Value}");
                    m_cachedList[entry.Value.Id] = legacyData;
                }
            }
        }
    }
}
