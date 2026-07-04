using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Interfaces;
using TransportLinesManager.ModShared;
using TransportLinesManager.Utils;
using System.Collections.Generic;
using UnityEngine;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.WorldInfoPanels.Tabs;
using System.Linq;

namespace TransportLinesManager.Data.Extensions
{
    public static class ExtensionStaticExtensionMethods
    {
        #region Assets List

        public static List<TransportAsset> GetAssetTransportListForLine<T>(this T it, ushort lineId) where T : IAssetSelectorExtension => it.SafeGet(it.LineToIndex(lineId)).AssetTransportList;

        public static void SetAssetTransportListForLine<T>(this T it, ushort lineId, List<TransportAsset> list) where T : IAssetSelectorExtension => it.SafeGet(it.LineToIndex(lineId)).AssetTransportList = [.. list];

        public static List<string> GetAssetListForLine<T>(this T it, ushort lineId) where T : IAssetSelectorExtension => it.SafeGet(it.LineToIndex(lineId)).AssetList;

        public static void SetAssetListForLine<T>(this T it, ushort lineId, List<string> list) where T : IAssetSelectorExtension => it.SafeGet(it.LineToIndex(lineId)).AssetList = [.. list];

        public static void AddAssetToLine<T>(this T it, ushort lineId, string assetId, string capacity, string weight, BudgetTarget budgetTarget) where T : IAssetSelectorExtension
        {
            List<TransportAsset> list = it.GetAssetTransportListForLine(lineId);
            IBasicExtensionStorage currentConfig = TLMLineUtils.GetEffectiveConfigForLine(lineId);
            var budgetEntries = TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetBudgetsMultiplierForLine(lineId, budgetTarget);
            var ext = TLMTransportLineExtension.Instance;

            bool isCustomConfig = ext.IsUsingCustomConfig(lineId);
            bool isAbsolute = isCustomConfig && ext.IsDisplayAbsoluteValues(lineId);

            if (list.Any(item => item.name == assetId))
            {
                return;
            }

            var item = new TransportAsset
            {
                name = assetId,
                capacity = int.Parse(capacity),
                count = [],
                spawn_percent = []
            };

            if (lineId != 0)
            {    
                for (int i = 0; i < budgetEntries.Count; i++)
                {
                    var count = new CountEntry
                    {
                        TotalCount = 0
                    };
                    item.count.Add(i.ToString(), count);

                    var spawnPercent = new SpawnPercentEntry
                    {
                        Value = 100
                    };
                    item.spawn_percent.Add(i.ToString(), spawnPercent);
                }
                var index = TLMAssetSelectorTab.GetBudgetSelectedIndex();
                if (index == -1)
                {
                    var hourIndex = budgetEntries.GetAtHourExact(TLMLineUtils.ReferenceTimer).Second;
                    index = hourIndex != -1 ? hourIndex : 0;
                }
                if (isAbsolute)
                {
                    var totalCount = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        totalCount += list[i].count[index.ToString()].TotalCount;
                    }
                    var newCount = int.Parse(weight);
                    // check if the new total is more then allowed if so make it zero
                    if (totalCount + newCount > budgetEntries[index].Value)
                    {
                        newCount = 0;
                    }
                    var item_count = item.count[index.ToString()];
                    item_count.TotalCount = newCount;
                    item.count[index.ToString()] = item_count;
                }
                else
                {
                    int parsedWeight = int.Parse(weight);
                    item.spawn_percent[index.ToString()].Value = parsedWeight > 0 ? parsedWeight : 100;
                }
            }
            list.Add(item);
            SetAssetTransportListForLine(it, lineId, list);
        }

        public static void AddDefaultToNewBudgetEntry<T>(this T it, ushort lineId, BudgetTarget budgetTarget) where T : IAssetSelectorExtension
        {
            List<TransportAsset> list = it.GetAssetTransportListForLine(lineId);

            var budgetEntries = TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetBudgetsMultiplierForLine(lineId, budgetTarget);

            int newIndex = budgetEntries.Count - 1;
            for (int i = 0; i < list.Count; i++)
            {
                var count = new CountEntry
                {
                    TotalCount = 0
                };
                list[i].count[newIndex.ToString()] = count;

                var spawnPercent = new SpawnPercentEntry
                {
                    Value = 100
                };
                list[i].spawn_percent[newIndex.ToString()] = spawnPercent;
            }
            SetAssetTransportListForLine(it, lineId, list);
        }

        public static void RemoveBudgetEntryByIndex<T>(this T it, ushort lineId, int indexToRemove)  where T : IAssetSelectorExtension
        {
            List<TransportAsset> list = it.GetAssetTransportListForLine(lineId);
            for (int i = 0; i < list.Count; i++)
            {
                // Remove the specific index
                list[i].count.Remove(indexToRemove.ToString());
                list[i].spawn_percent.Remove(indexToRemove.ToString());

                var newCount = new SimpleXmlDictionary<string, CountEntry>();
                var newPercent = new SimpleXmlDictionary<string, SpawnPercentEntry>();
                foreach (var kvp in list[i].count)
                {
                    newCount[kvp.Key.CompareTo(indexToRemove.ToString()) > 0 ? (int.Parse(kvp.Key) - 1).ToString() : kvp.Key] = kvp.Value;
                } 
                foreach (var kvp in list[i].spawn_percent)
                { 
                    newPercent[kvp.Key.CompareTo(indexToRemove.ToString()) > 0 ? (int.Parse(kvp.Key) - 1).ToString() : kvp.Key] = kvp.Value; 
                }
                var item = list[i];
                item.count = newCount;
                item.spawn_percent = newPercent;
                list[i] = item;
                it.SetAssetTransportListForLine(lineId, list);
            }
            SetAssetTransportListForLine(it, lineId, list);
        }

        public static void RemoveAssetFromLine<T>(this T it, ushort lineId, string assetId) where T : IAssetSelectorExtension
        {
            if (string.IsNullOrEmpty(assetId)) return; // guard against null loop
            List<TransportAsset> list = it.GetAssetTransportListForLine(lineId);
            if (!list.Any(item => item.name == assetId))
            {
                return;
            }
            list.RemoveAll(x => x.name == assetId);
        }

        public static void UseDefaultAssetsAtLine<T>(this T it, ushort lineId) where T : IAssetSelectorExtension => it.GetAssetListForLine(lineId).Clear();

        #endregion

        #region Name

        public static string GetName<T>(this T it, uint prefix) where T : INameableExtension => it.SafeGet(prefix).Name;

        public static void SetName<T>(this T it, uint prefix, string name) where T : INameableExtension => it.SafeGet(prefix).Name = name;

        #endregion

        #region Budget Multiplier

        public enum BudgetTarget
        {
            Weekday,
            Weekend
        }

        public static TimeableList<BudgetEntryXml> GetBudgetsMultiplierForLine<T>(this T it, ushort lineId, BudgetTarget budgetTarget) where T : IBudgetableExtension
        {
            var budgetStorage = it.SafeGet(it.LineToIndex(lineId));

            if (budgetTarget == BudgetTarget.Weekend && budgetStorage.UseSeparateWeekendBudget)
            {
                return budgetStorage.WeekendBudgetEntries ?? budgetStorage.BudgetEntries;
            }

            return budgetStorage.BudgetEntries;
        }

        public static uint GetBudgetMultiplierForHourForLine<T>(this T it, ushort lineId, float hour, BudgetTarget budgetTarget) where T : IBudgetableExtension
        {
            TimeableList<BudgetEntryXml> budget = it.GetBudgetsMultiplierForLine(lineId, budgetTarget);
            Tuple<Tuple<BudgetEntryXml, int>, Tuple<BudgetEntryXml, int>, float> currentBudget = budget.GetAtHour(hour);
            return (uint)Mathf.Lerp(currentBudget.First.First.Value, currentBudget.Second.First.Value, currentBudget.Third);
        }

        public static void SetBudgetMultiplierForLine<T>(this T it, ushort lineId, uint multiplier, int hour, BudgetTarget budgetTarget) where T : IBudgetableExtension => it.GetBudgetsMultiplierForLine(lineId, budgetTarget).Add(new BudgetEntryXml()
        {
            Value = multiplier,
            HourOfDay = hour
        });

        public static void RemoveBudgetMultiplierForLine<T>(this T it, ushort lineId, int hour, BudgetTarget budgetTarget) where T : IBudgetableExtension => it.GetBudgetsMultiplierForLine(lineId, budgetTarget).RemoveAtHour(hour);

        public static void RemoveAllBudgetMultipliersOfLine<T>(this T it, ushort lineId, BudgetTarget budgetTarget) where T : IBudgetableExtension
        {
            var budgetStorage = it.SafeGet(it.LineToIndex(lineId));

            if (budgetTarget == BudgetTarget.Weekend && budgetStorage.UseSeparateWeekendBudget)
            {
                budgetStorage.WeekendBudgetEntries =
                [
                    new() { HourOfDay = 0, Value = 100 }
                ];
            }
            else
            {
                budgetStorage.BudgetEntries =
                [
                    new() { HourOfDay = 0, Value = 100 }
                ];
            }
        }

        public static void SetAllBudgetMultipliersForLine<T>(this T it, ushort lineId, BudgetTarget budgetTarget, TimeableList<BudgetEntryXml> newValue) where T : IBudgetableExtension
        {
            var budgetStorage = it.SafeGet(it.LineToIndex(lineId));

            if (budgetTarget == BudgetTarget.Weekend && budgetStorage.UseSeparateWeekendBudget)
            {
                budgetStorage.WeekendBudgetEntries = newValue;
            }
            else
            {
                budgetStorage.BudgetEntries = newValue;
            }
        }

        public static bool IsWeekendBudgetActive(IBudgetStorage cfg)
        {
            if (cfg is null || !TLMController.IsRealTimeEnabled)
            {
                return false;
            }

            if (!cfg.UseSeparateWeekendBudget)
            {
                return false;
            }

            return RealTimeUtils.IsWeekend();
        }

        public static TimeableList<BudgetEntryXml> GetActiveBudgetEntries<T>(this T it, ushort lineId) where T : IBudgetableExtension
        {
            var budgetStorage = it.SafeGet(it.LineToIndex(lineId));
            bool useWeekend = IsWeekendBudgetActive(budgetStorage);
            return useWeekend ? (budgetStorage.WeekendBudgetEntries ?? []) : budgetStorage.BudgetEntries;
        }

        public static void SetActiveBudgetMultiplierForLine<T>(this T it, ushort lineId, uint multiplier, int hour) where T : IBudgetableExtension
        {
            var budgetStorage = it.SafeGet(it.LineToIndex(lineId));
            bool useWeekend = IsWeekendBudgetActive(budgetStorage);

            TimeableList<BudgetEntryXml> targetList;
            if (useWeekend)
            {
                targetList = budgetStorage.WeekendBudgetEntries ??= [];
            }
            else
            {
                targetList = budgetStorage.BudgetEntries;
            }

            targetList.Add(new BudgetEntryXml
            {
                HourOfDay = hour,
                Value = multiplier
            });
        }

        #endregion

        #region Ticket Price

        public static TimeableList<TicketPriceEntryXml> GetTicketPricesForLine<T>(this T it, ushort lineId) where T : ITicketPriceExtension => it.SafeGet(it.LineToIndex(lineId)).TicketPriceEntries;
        
        public static void SetTicketPricesForLine<T>(this T it, ushort lineId, TimeableList<TicketPriceEntryXml> newPrices) where T : ITicketPriceExtension => it.SafeGet(it.LineToIndex(lineId)).TicketPriceEntries = newPrices;
        
        public static void ClearTicketPricesOfLine<T>(this T it, ushort lineId) where T : ITicketPriceExtension => it.SafeGet(it.LineToIndex(lineId)).TicketPriceEntries =
        [
            new(){HourOfDay=0,Value=0}
        ];
        
        public static Tuple<TicketPriceEntryXml, int> GetTicketPriceForHourForLine<T>(this T it, ushort lineId, float hour) where T : ITicketPriceExtension
        {
            TimeableList<TicketPriceEntryXml> ticketPrices = it.GetTicketPricesForLine(lineId);
            return ticketPrices.GetAtHourExact(hour);
        }
        
        public static void SetTicketPriceToLine<T>(this T it, ushort lineId, uint multiplier, int hour) where T : ITicketPriceExtension => it.SafeGet(it.LineToIndex(lineId)).TicketPriceEntries.Add(new TicketPriceEntryXml()
        {
            Value = multiplier,
            HourOfDay = hour
        });
       
        public static void RemoveTicketPriceEntryToLine<T>(this T it, ushort lineId, int hour) where T : ITicketPriceExtension => it.SafeGet(it.LineToIndex(lineId)).TicketPriceEntries.RemoveAtHour(hour);

        #endregion

        #region Color

        public static Color GetColor<T>(this T it, uint prefix) where T : IColorSelectableExtension => it.SafeGet(prefix).Color;

        public static void SetColor<T>(this T it, uint prefix, Color value) where T : IColorSelectableExtension
        {
            if (value.a < 1)
            {
                it.CleanColor(prefix);
            }
            else
            {
                it.SafeGet(prefix).Color = value;
            }
            TLMFacade.Instance?.OnLineSymbolParameterChanged();
        }

        public static void CleanColor<T>(this T it, uint prefix) where T : IColorSelectableExtension => it.SafeGet(prefix).Color = default;

        #endregion

        #region Depot

        private static IDepotSelectionStorage EnsureCreationDepotConfig<T>(T it, uint idx) where T : IDepotSelectableExtension
        {
            IDepotSelectionStorage config = it.SafeGet(idx);
            config.DepotsAllowed ??= [];
            return config;
        }

        public static void AddDepotForLine<T>(this T it, ushort lineId, ushort buildingID) where T : IDepotSelectableExtension => EnsureCreationDepotConfig(it, it.LineToIndex(lineId)).DepotsAllowed.Add(buildingID);

        public static void RemoveDepotForLine<T>(this T it, ushort lineId, ushort buildingID) where T : IDepotSelectableExtension => EnsureCreationDepotConfig(it, it.LineToIndex(lineId)).DepotsAllowed.Remove(buildingID);

        public static void RemoveAllDepotsForLine<T>(this T it, ushort lineId) where T : IDepotSelectableExtension => EnsureCreationDepotConfig(it, it.LineToIndex(lineId)).DepotsAllowed.Clear();

        public static void AddAllDepots<T>(this T it, uint idx) where T : IDepotSelectableExtension => it.SafeGet(idx).DepotsAllowed = null;
        
        public static List<ushort> GetAllowedDepots<T>(this T it, TransportSystemDefinition tsd, ushort lineId) where T : IDepotSelectableExtension
        {
            IDepotSelectionStorage data = it.SafeGet(it.LineToIndex(lineId));
            if(TLMController.IsSchoolBusesEnabled)
            {
                var buildingId = SchoolBusUtils.GetSchoolBuilding(lineId);
                if (buildingId != 0)
                {
                    return [buildingId];
                }
            }
            List<ushort> saida = TLMDepotUtils.GetAllDepotsFromCity(tsd);
            if (data.DepotsAllowed == null)
            {
                return saida;
            }
            else
            {
                return [.. data.DepotsAllowed];
            }
        }

        #endregion

    }
}
