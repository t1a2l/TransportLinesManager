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
        public override string SaveId => "K45_TLM_TLMTransportTypeDataContainer";

        private static readonly Dictionary<string, TransportSystemDefinition> legacyLinks = new Dictionary<string, TransportSystemDefinition>
        {
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorBus"] = TransportSystemDefinition.BUS,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorBlp"] = TransportSystemDefinition.BLIMP,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionEvcBus"] = TransportSystemDefinition.EVAC_BUS,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorFer"] = TransportSystemDefinition.FERRY,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorMet"] = TransportSystemDefinition.METRO,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorMnr"] = TransportSystemDefinition.MONORAIL,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorPln"] = TransportSystemDefinition.PLANE,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorShp"] = TransportSystemDefinition.SHIP,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrn"] = TransportSystemDefinition.TRAIN,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrm"] = TransportSystemDefinition.TRAM,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionTouBus"] = TransportSystemDefinition.TOUR_BUS,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionTouPed"] = TransportSystemDefinition.TOUR_PED,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorTrl"] = TransportSystemDefinition.TROLLEY,
            ["K45_TLM_TransportLinesManager.Extensions.TLMTransportTypeExtensionNorHel"] = TransportSystemDefinition.HELICOPTER,
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
