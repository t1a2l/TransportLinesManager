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
using TransportLinesManager.WorldInfoPanels.Tabs;

namespace TransportLinesManager.Data.DataContainers
{
    public class TLMTransportLineExtension : DataExtensionBase<TLMTransportLineExtension>, ISafeGettable<TLMTransportLineConfiguration>, IBasicExtension
    {
        [XmlElement("Configurations")]
        public SimpleNonSequentialList<TLMTransportLineConfiguration> Configurations { get; set; } = new SimpleNonSequentialList<TLMTransportLineConfiguration>();
        internal void SafeCleanEntry(ushort lineID) => Configurations[lineID] = new TLMTransportLineConfiguration();
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

        private readonly Dictionary<TransportSystemDefinition, List<TransportAsset>> m_basicAssetsList = new();

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
        public VehicleInfo GetAModel(ushort lineId, string status)
        {
            VehicleInfo info = null;
            List<TransportAsset> assetTransportList = ExtensionStaticExtensionMethods.GetAssetTransportListForLine(this, lineId);
            while (info == null && assetTransportList.Count > 0)
            {
                info = VehicleUtils.GetModelByPercentageOrCount(assetTransportList, lineId, out string modelName, status);
                if (info == null)
                {
                    ExtensionStaticExtensionMethods.RemoveAssetFromLine(this, lineId, modelName);
                    assetTransportList = ExtensionStaticExtensionMethods.GetAssetTransportListForLine(this, lineId);
                }
            }
            return info;
        }

        public void EditVehicleUsedCount(ushort lineID, string selectedModel, string status)
        {
            IBasicExtensionStorage currentConfig = TLMLineUtils.GetEffectiveConfigForLine(lineID);
            List<TransportAsset> assetTransportList = ExtensionStaticExtensionMethods.GetAssetTransportListForLine(this, lineID);
            var index = TLMAssetSelectorTab.GetBudgetSelectedIndex();
            if (index == -1)
            {
                index = 0;
            }
            var asset_index = assetTransportList.FindIndex(item => item.name == selectedModel);
            var asset_count = assetTransportList[asset_index].count[index];
            if (status == "Add")
            {
                asset_count.usedCount++;
            }
            else if (status == "Remove")
            {
                asset_count.usedCount--;
            }
            assetTransportList[asset_index].count[index] = asset_count;
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
