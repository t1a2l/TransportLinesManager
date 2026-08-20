using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using ColossalFramework.Globalization;
using Commons;
using Commons.Interfaces.Warehouse;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base.ConfigurationContainers;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Interfaces;
using TransportLinesManager.ModShared;
using TransportLinesManager.Utils;

namespace TransportLinesManager.Data.DataContainers
{
    public class TLMTransportLineExtension : DataExtensionBase<TLMTransportLineExtension>, ISafeGettable<TLMTransportLineConfiguration>, IBasicExtension
    {
        [XmlElement("Configurations")]
        public SimpleNonSequentialList<TLMTransportLineConfiguration> Configurations { get; set; } = [];

        internal void SafeCleanEntry(ushort lineID)
        {
            Configurations[lineID] = new TLMTransportLineConfiguration();
            TLMLineUtils.ClearRuntimeUsedCountForLine(lineID);
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

        public void SetUseCustomConfig(ushort lineId, bool value)
        {
            SafeGet(lineId).IsCustom = value;
            TLMFacade.Instance?.OnLineSymbolParameterChanged();
        }

        public bool IsUsingCustomConfig(ushort lineId) => SafeGet(lineId).IsCustom;

        public void SetDisplayAbsoluteValues(ushort lineId, bool value) => SafeGet(lineId).DisplayAbsoluteValues = value;

        public bool IsDisplayAbsoluteValues(ushort lineId) => SafeGet(lineId).DisplayAbsoluteValues;

        public bool IsAutoAdjustAbsoluteCountsEnabled(ushort lineId) => SafeGet(lineId).AutoAdjustAbsoluteCounts;

        public void SetAutoAdjustAbsoluteCounts(ushort lineId, bool value) => SafeGet(lineId).AutoAdjustAbsoluteCounts = value;

        public uint LineToIndex(ushort lineId) => lineId > 0 ? lineId : throw new System.Exception("Line 0 cannot have specific configuration!");

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

            // If no configured assets and auto-spawn is allowed, use vanilla random behavior.
            if (assetTransportList == null || assetTransportList.Count == 0)
            {
                if (TLMBaseConfigXML.CurrentContextConfig.AllowAutoSpawnAllVehicles)
                {
                    var tsd = TransportSystemDefinition.FromLineId(lineId, false);
                    // Use your existing logic for “basic” assets or fall back to vanilla:
                    var basicList = GetBasicAssetListForLine(lineId);
                    if (basicList != null && basicList.Count > 0)
                    {
                        info = VehicleUtils.GetRandomModel([.. basicList.Select(a => a.name)], out string _);
                    }

                    // Note: no EditVehicleUsedCount here if the asset isn’t in your list.
                    return info;
                }

                // If auto-spawn is disabled, keep your current “no asset” behavior.
                return null;
            }

            while (info == null && assetTransportList.Count > 0)
            {
                string modelName = null;
                if (lineId != 0)
                {
                    LogUtils.DoLog("Calling GetModelByPercentageOrCount");
                    info = VehicleUtils.GetModelByPercentageOrCount(assetTransportList, lineId, out modelName);
                }
                else
                {
                    LogUtils.DoLog("Calling GetRandomModel");
                    // Regional lines (lineId == 0) use the basic randomizer
                    var simpleStringList = assetTransportList.Select(a => a.name).ToList();
                    info = VehicleUtils.GetRandomModel(simpleStringList, out modelName);
                }
                if (info == null)
                {
                    if (string.IsNullOrEmpty(modelName))
                    {
                        LogUtils.DoErrorLog($"Null model name for line {lineId} — breaking to avoid infinite loop");
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
            if (lineID == 0 || string.IsNullOrEmpty(selectedModel))
            {
                return;
            }

            List<TransportAsset> assetTransportList = ExtensionStaticExtensionMethods.GetAssetTransportListForLine(this, lineID);

            bool isAutomaticAssetMode = TLMBaseConfigXML.CurrentContextConfig.AllowAutoSpawnAllVehicles && (assetTransportList == null || assetTransportList.Count == 0);

            // No selected vehicle assets means automatic selection.
            // There are no per-model quotas or runtime used counts to track.
            if (isAutomaticAssetMode)
            {
                return;
            }

            int index = TLMLineUtils.GetEffectiveExtensionForLine(lineID).GetActiveBudgetEntries(lineID).GetAtHourExact(TLMLineUtils.ReferenceTimer).Second;

            if (index < 0)
            {
                index = 0;
            }

            TLMLineUtils.EnsureUsedCountSlotSynchronized(lineID, index);

            int assetindex = assetTransportList?.FindIndex(item => item.name == selectedModel) ?? -1;

            if (assetindex == -1)
            {
                if (status == "Add")
                {
                    LogUtils.DoErrorLog($"EditVehicleUsedCount: Could not find asset {selectedModel} in line {lineID} asset list while adding.");
                }
                else if (CommonProperties.DebugMode)
                {
                    LogUtils.DoLog($"EditVehicleUsedCount: Asset {selectedModel} not found in line {lineID} asset list while removing; ignoring.");
                }
                return;
            }

            if (status == "Add")
            {
                TLMLineUtils.ChangeRuntimeUsedCount(lineID, index, selectedModel, 1);
            }
            else if (status == "Remove")
            {
                TLMLineUtils.ChangeRuntimeUsedCount(lineID, index, selectedModel, -1);
            }

            TLMLineUtils.NotifyAssetUsedCountChanged(lineID, index);
        }

        #endregion
    }
}
