using ICities;
using Commons.Interfaces.Warehouse;
using Commons.Utils;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.Base.ConfigurationContainers;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace TransportLinesManager.Data.DataContainers
{
    [XmlRoot("TransportTypeDataContainer")]
    public class TLMTransportTypeDataContainer : ExtensionInterfaceIndexableImpl<TLMTransportTypeConfigurations, TLMTransportTypeDataContainer>
    {
        public override string SaveId => "TLM_TLMTransportTypeDataContainer";

        private static readonly Dictionary<string, TransportSystemDefinition> legacyLinks = new Dictionary<string, TransportSystemDefinition>
        {
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorBus"] = TransportSystemDefinition.BUS,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorBlp"] = TransportSystemDefinition.BLIMP,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionEvcBus"] = TransportSystemDefinition.EVAC_BUS,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorFer"] = TransportSystemDefinition.FERRY,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorMet"] = TransportSystemDefinition.METRO,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorMnr"] = TransportSystemDefinition.MONORAIL,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorPln"] = TransportSystemDefinition.PLANE,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorShp"] = TransportSystemDefinition.SHIP,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrn"] = TransportSystemDefinition.TRAIN,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrm"] = TransportSystemDefinition.TRAM,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionTouBus"] = TransportSystemDefinition.TOUR_BUS,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionTouPed"] = TransportSystemDefinition.TOUR_PED,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrl"] = TransportSystemDefinition.TROLLEY,
            ["TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorHel"] = TransportSystemDefinition.HELICOPTER,
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
