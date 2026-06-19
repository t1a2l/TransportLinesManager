using ColossalFramework.Globalization;
using Commons.Interfaces.Warehouse;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.Base.ConfigurationContainers;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Interfaces;
using TransportLinesManager.ModShared;
using TransportLinesManager.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace TransportLinesManager.Data.DataContainers
{
    public class TLMTransportLineExtension : DataExtensionBase<TLMTransportLineExtension>, ISafeGettable<TLMTransportLineConfiguration>, IBasicExtension
    {
        [XmlElement("Configurations")]
        public SimpleNonSequentialList<TLMTransportLineConfiguration> Configurations { get; set; } = [];

        internal void SafeCleanEntry(ushort lineID)
        {
            Configurations[lineID] = new TLMTransportLineConfiguration();
            m_lastUsedCountSlotByLine.Remove(lineID);
        }
        
        public TLMTransportLineConfiguration SafeGet(uint lineId)
        {
            if (!Configurations.ContainsKey(lineId))
            {
                Configurations[lineId] = new TLMTransportLineConfiguration();
            }
            return Configurations[lineId];
        }

        IAssetSelectorStorage ISafeGettable<IAssetSelectorStorage>.SafeGet(uint index) => SafeGet(index);

        IBudgetStorage ISafeGettable<IBudgetStorage>.SafeGet(uint index) => SafeGet(index);

        ITicketPriceStorage ISafeGettable<ITicketPriceStorage>.SafeGet(uint index) => SafeGet(index);

        IDepotSelectionStorage ISafeGettable<IDepotSelectionStorage>.SafeGet(uint index) => SafeGet(index);

        IBasicExtensionStorage ISafeGettable<IBasicExtensionStorage>.SafeGet(uint index) => SafeGet(index);

        public override string SaveId => $"TLM_TLMTransportLineExtension";

        private readonly Dictionary<TransportSystemDefinition, List<TransportAsset>> m_basicAssetsList = [];

        private readonly Dictionary<ushort, int> m_lastUsedCountSlotByLine = [];

        public void SetUseCustomConfig(ushort lineId, bool value)
        {
            SafeGet(lineId).IsCustom = value;
            TLMFacade.Instance?.OnLineSymbolParameterChanged();
        }

        public bool IsUsingCustomConfig(ushort lineId) => SafeGet(lineId).IsCustom;

        public void SetDisplayAbsoluteValues(ushort lineId, bool value) => SafeGet(lineId).DisplayAbsoluteValues = value;

        public bool IsDisplayAbsoluteValues(ushort lineId) => SafeGet(lineId).DisplayAbsoluteValues;

        #region Asset List
        public List<TransportAsset> GetBasicAssetListForLine(ushort lineId)
        {
            var tsd = TransportSystemDefinition.FromLineId(lineId, false);
            if (!m_basicAssetsList.ContainsKey(tsd))
            {
                m_basicAssetsList[tsd] = TLMPrefabUtils.LoadBasicAssets(tsd);
            }
            return m_basicAssetsList[tsd];
        }

        public Dictionary<TransportAsset, string> GetSelectedBasicAssetsForLine(ushort lineId) => this.GetAssetTransportListForLine(lineId).Where(x => PrefabCollection<VehicleInfo>.FindLoaded(x.name) != null).ToDictionary(x => x, x => Locale.Get("VEHICLE_TITLE", x.name));
        
        public Dictionary<TransportAsset, string> GetAllBasicAssetsForLine(ushort lineId)
        {
            var tsd = TransportSystemDefinition.FromLineId(lineId, false);
            if (!m_basicAssetsList.ContainsKey(tsd))
            {
                m_basicAssetsList[tsd] = TLMPrefabUtils.LoadBasicAssets(tsd);
            }

            return m_basicAssetsList[tsd].ToDictionary(x => x, x => Locale.Get("VEHICLE_TITLE", x.name));
        }
       
        public VehicleInfo GetAModel(ushort lineId)
        {
            VehicleInfo info = null;
            List<TransportAsset> assetTransportList = ExtensionStaticExtensionMethods.GetAssetTransportListForLine(this, lineId);
            while (info == null && assetTransportList.Count > 0)
            {
                string modelName = null;
                if (lineId != 0)
                {
                    info = VehicleUtils.GetModelByPercentageOrCount(assetTransportList, lineId, out modelName);
                }
                else
                {
                    // Regional lines (lineId == 0) use the basic randomizer
                    var simpleStringList = assetTransportList.Select(a => a.name).ToList();
                    info = VehicleUtils.GetRandomModel(simpleStringList, out modelName);
                }
                if (info == null)
                {
                    if (string.IsNullOrEmpty(modelName))
                    {
                        LogUtils.DoErrorLog($"GetAModel: GetModelByPercentageOrCount returned null model name for line {lineId} — breaking to avoid infinite loop");
                        break;
                    }
                    ExtensionStaticExtensionMethods.RemoveAssetFromLine(this, lineId, modelName);
                    assetTransportList = ExtensionStaticExtensionMethods.GetAssetTransportListForLine(this, lineId);
                }
            }
            return info;
        }

        public void EditVehicleUsedCount(ushort lineID, string selectedModel, string status)
        {
            if (lineID == 0)
            {
                return;
            }

            int index = GetCurrentExactBudgetSlot(lineID);
            EnsureUsedCountSlotSynchronized(lineID, index);

            List<TransportAsset> assetTransportList = ExtensionStaticExtensionMethods.GetAssetTransportListForLine(this, lineID);
            int assetindex = assetTransportList.FindIndex(item => item.name == selectedModel);
            if (assetindex == -1)
            {
                LogUtils.DoErrorLog($"EditVehicleUsedCount: Could not find asset {selectedModel} in line {lineID} asset list");
                return;
            }

            string key = index.ToString();
            TransportAsset asset = assetTransportList[assetindex];

            asset.count ??= [];

            if (!asset.count.ContainsKey(key))
            {
                asset.count[key] = new CountEntry
                {
                    TotalCount = 0,
                    UsedCount = 0
                };
            }

            CountEntry assetcount = asset.count[key];

            if (status == "Add")
            {
                assetcount.UsedCount++;
            }
            else if (status == "Remove" && assetcount.UsedCount > 0)
            {
                assetcount.UsedCount--;
            }

            asset.count[key] = assetcount;
            assetTransportList[assetindex] = asset;
            ExtensionStaticExtensionMethods.SetAssetTransportListForLine(this, lineID, assetTransportList);
        }

        private int GetCurrentExactBudgetSlot(ushort lineID)
        {
            if (lineID == 0)
            {
                return 0;
            }

            var exactBudget = TLMLineUtils.GetEffectiveConfigForLine(lineID).BudgetEntries.GetAtHourExact(SimulationManager.instance.m_currentGameTime.Hour);

            int index = exactBudget.Second;
            return index < 0 ? 0 : index;
        }

        private void EnsureUsedCountSlotSynchronized(ushort lineID, int currentSlot)
        {
            if (lineID == 0)
            {
                return;
            }

            if (!m_lastUsedCountSlotByLine.TryGetValue(lineID, out int lastSlot) || lastSlot != currentSlot)
            {
                RebuildUsedCountForCurrentSlot(lineID, currentSlot);
                m_lastUsedCountSlotByLine[lineID] = currentSlot;
            }
        }

        private void RebuildUsedCountForCurrentSlot(ushort lineID, int slotIndex)
        {
            if (lineID == 0 || slotIndex < 0)
            {
                return;
            }

            List<TransportAsset> assetTransportList = ExtensionStaticExtensionMethods.GetAssetTransportListForLine(this, lineID);
            if (assetTransportList == null || assetTransportList.Count == 0)
            {
                return;
            }

            var tm = TransportManager.instance;
            var vm = VehicleManager.instance;
            ref TransportLine tl = ref tm.m_lines.m_buffer[lineID];

            Dictionary<string, int> vehicleCountPerAsset = [];
            int vehicleCount = tl.CountVehicles(lineID);

            for (int v = 0; v < vehicleCount; v++)
            {
                ushort vehicleId = tl.GetVehicle(v);
                if (vehicleId == 0)
                {
                    continue;
                }

                VehicleInfo info = vm.m_vehicles.m_buffer[vehicleId].Info;
                if (info == null || string.IsNullOrEmpty(info.name))
                {
                    continue;
                }

                if (!vehicleCountPerAsset.ContainsKey(info.name))
                {
                    vehicleCountPerAsset[info.name] = 0;
                }

                vehicleCountPerAsset[info.name]++;
            }

            string key = slotIndex.ToString();

            for (int i = 0; i < assetTransportList.Count; i++)
            {
                TransportAsset asset = assetTransportList[i];

                asset.count ??= [];

                if (!asset.count.ContainsKey(key))
                {
                    asset.count[key] = new CountEntry
                    {
                        TotalCount = 0,
                        UsedCount = 0
                    };
                }

                CountEntry entry = asset.count[key];
                entry.UsedCount = vehicleCountPerAsset.TryGetValue(asset.name, out int used) ? used : 0;
                asset.count[key] = entry;

                assetTransportList[i] = asset;
            }

            ExtensionStaticExtensionMethods.SetAssetTransportListForLine(this, lineID, assetTransportList);
        }

        #endregion

        #region Ticket Price

        public uint GetDefaultTicketPrice(uint lineId = 0)
        {
            var tsd = TransportSystemDefinition.FromLineId((ushort)lineId, false);
            switch (tsd.SubService)
            {
                case ItemClass.SubService.PublicTransportCableCar:
                case ItemClass.SubService.PublicTransportBus:
                case ItemClass.SubService.PublicTransportMonorail:
                    return 100;
                case ItemClass.SubService.PublicTransportMetro:
                case ItemClass.SubService.PublicTransportTaxi:
                case ItemClass.SubService.PublicTransportTrain:
                case ItemClass.SubService.PublicTransportTram:
                    return 200;
                case ItemClass.SubService.PublicTransportPlane:
                    if (tsd.VehicleType == VehicleInfo.VehicleType.Blimp)
                    {
                        return 100;
                    }
                    else
                    {
                        return 1000;
                    }
                case ItemClass.SubService.PublicTransportShip:
                    if (tsd.VehicleType == VehicleInfo.VehicleType.Ferry)
                    {
                        return 100;
                    }
                    else
                    {
                        return 500;
                    }
                case ItemClass.SubService.PublicTransportTours:
                    if (tsd.VehicleType == VehicleInfo.VehicleType.Car)
                    {
                        return 100;
                    }
                    else if (tsd.VehicleType == VehicleInfo.VehicleType.None)
                    {
                        return 0;
                    }
                    return 102;
                default:
                    LogUtils.DoLog("subservice not found: {0}", tsd.SubService);
                    return 103;
            }

        }
        #endregion

        #region Depot
        public uint LineToIndex(ushort lineId) => lineId > 0 ? lineId : throw new System.Exception("Line 0 cannot have specific configuration!");
        #endregion

    }
}
