using System.Collections.Generic;
using System.Linq;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Interfaces;
using TransportLinesManager.WorldInfoPanels.Tabs;
using UnityEngine;
using static TransportLinesManager.Data.Extensions.ExtensionStaticExtensionMethods;

namespace TransportLinesManager.Utils
{
    public static class TLMCountModeUtils
    {
        // Called when budget slider changes value in count mode
        public static void OnBudgetChangedInCountMode(ushort lineId, IBasicExtension config, int budgetIndex, int newBudget, ProfileTarget profileTarget)
        {
            List<TransportAsset> assets = config.GetAssetTransportListForLine(lineId);
            string key = budgetIndex.ToString();
            int currentSum = assets.Sum(a => a.count.TryGetValue(key, out var ce) ? ce.TotalCount : 0);

            var lineExt = TLMTransportLineExtension.Instance;

            // If auto-adjust is enabled for this line, just rebalance everything for this profile.
            if (lineExt.IsUsingCustomConfig(lineId) && lineExt.IsDisplayAbsoluteValues(lineId) && lineExt.IsAutoAdjustAbsoluteCountsEnabled(lineId))
            {
                // Update the budget entry itself (if you haven’t already done that in the caller),
                // then rebalance counts to match all budgets in this profile.
                RebalanceAbsoluteCountsForLine(lineId, profileTarget);
                return;
            }

            if (newBudget == 0)
            {
                // Disable all — zero everything and make read-only in UI (handled by UI layer)
                return; // counts stay stored, just hidden
            }

            if (currentSum == 0)
            {
                // No counts set yet — nothing to scale, UI shows "X unassigned"
                return;
            }

            if (newBudget >= currentSum)
            {
                // Budget raised: leave counts as-is, extra goes to unassigned pool
                return;
            }

            // Budget reduced: scale down proportionally
            ScaleCountsDown(assets, key, newBudget);
            config.SetAssetTransportListForLine(lineId, assets);


        }

        // Called when a line is created or when the user switches to count mode for an existing line
        public static void RebalanceAbsoluteCountsForLine(ushort lineId, ProfileTarget profileTarget)
        {
            if (lineId == 0)
            {
                return;
            }

            // Get the effective extension and config
            var lineExt = TLMTransportLineExtension.Instance;

            // Only run when custom config + absolute mode + auto-adjust enabled + auto adjust per-line flag
            if (!lineExt.IsUsingCustomConfig(lineId) || !lineExt.IsDisplayAbsoluteValues(lineId) || !lineExt.IsAutoAdjustAbsoluteCountsEnabled(lineId))
            {
                return;
            }

            // Asset list for this line
            List<TransportAsset> assets = lineExt.GetAssetTransportListForLine(lineId);
            if (assets == null || assets.Count == 0)
            {
                return;
            }

            // Budget entries for this profile
            var budgetEntries = lineExt.GetBudgetsMultiplierForLine(lineId, profileTarget);

            if (budgetEntries == null || budgetEntries.Count == 0)
            {
                return;
            }

            // For each budget index, redistribute counts
            for (int budgetIndex = 0; budgetIndex < budgetEntries.Count; budgetIndex++)
            {
                float budgetMultiplier = budgetEntries[budgetIndex].Value / 100f;

                ref TransportLine line = ref TransportManager.instance.m_lines.m_buffer[lineId];

                int maxVehicles = TLMLineUtils.ProjectTargetVehicleCount(line.Info, line.m_totalLength, budgetMultiplier);

                if (maxVehicles <= 0)
                {
                    continue;
                }

                string key = budgetIndex.ToString();

                int baseCount = maxVehicles / assets.Count;
                int remainder = maxVehicles % assets.Count;

                for (int i = 0; i < assets.Count; i++)
                {
                    var asset = assets[i];

                    asset.count ??= [];

                    if (!asset.count.TryGetValue(key, out var countEntry))
                    {
                        countEntry = new CountEntry();
                        asset.count[key] = countEntry;
                    }

                    int countForAsset = baseCount + (i < remainder ? 1 : 0);
                    countEntry.TotalCount = countForAsset;
                }
            }

            // Save updated list back and notify; runtime counts will be reconciled when needed
            lineExt.SetAssetTransportListForLine(lineId, assets);

            // Notify for the active slot so UI refreshes; you can pick index 0 or active slot
            var index = TLMAssetSelectorTab.GetBudgetSelectedIndex();
            if (index == -1)
            {
                var hourIndex = lineExt.GetActiveBudgetEntries(lineId).GetAtHourExact(TLMLineUtils.ReferenceTimer).Second;
                index = hourIndex != -1 ? hourIndex : 0;
                if(index < 0 || index >= budgetEntries.Count)
                {
                    index = 0;
                }
            }

            TLMLineUtils.NotifyAssetUsedCountChanged(lineId, index);
        }

        // Called when sum of counts > budget → auto-raise budget
        // Returns the new budget value to apply
        public static int ReconcileOverAssigned(List<TransportAsset> assets, string key, int currentBudget)
        {
            int sum = assets.Sum(a => a.count.TryGetValue(key, out var ce) ? ce.TotalCount : 0);
            return sum > currentBudget ? sum : currentBudget;
        }

        // Returns unassigned count for UI indicator
        public static int GetUnassignedCount(List<TransportAsset> assets, string key, int budget)
        {
            int sum = assets.Sum(a => a.count.TryGetValue(key, out var ce) ? ce.TotalCount : 0);
            return Mathf.Max(0, budget - sum);
        }

        // Percent → Count conversion when switching modes
        public static void ConvertPercentToCount(List<TransportAsset> assets, string key, int budget)
        {
            foreach (var asset in assets)
            {
                if (!asset.spawn_percent.TryGetValue(key, out var sp)) continue;
                int count = Mathf.RoundToInt(budget * sp.Value / 100f);
                var ce = asset.count.TryGetValue(key, out var existing) ? existing : new CountEntry();
                ce.TotalCount = count;
                asset.count[key] = ce;
            }
        }

        // Count → Percent conversion when switching modes
        public static void ConvertCountToPercent(List<TransportAsset> assets, string key, int budget)
        {
            if (budget == 0) return;
            foreach (var asset in assets)
            {
                if (!asset.count.TryGetValue(key, out var ce)) continue;
                int percent = Mathf.RoundToInt(100f * ce.TotalCount / budget);
                var sp = asset.spawn_percent.TryGetValue(key, out var existing) ? existing : new SpawnPercentEntry();
                sp.Value = percent;
                asset.spawn_percent[key] = sp;
            }
        }

        // Scale down counts proportionally to fit within new budget
        private static void ScaleCountsDown(List<TransportAsset> assets, string key, int newBudget)
        {
            int currentSum = assets.Sum(a => a.count.TryGetValue(key, out var ce) ? ce.TotalCount : 0);
            if (currentSum == 0)
            {
                return;
            }

            float ratio = (float)newBudget / currentSum;
            int assigned = 0;
            int highestIdx = -1;
            int highestCount = -1;

            // Find highest-weight (highest TotalCount) asset for remainder
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (!asset.count.TryGetValue(key, out var ce))
                {
                    continue;
                }

                if (ce.TotalCount > highestCount)
                {
                    highestCount = ce.TotalCount;
                    highestIdx = i;
                }

                int scaled = Mathf.FloorToInt(ce.TotalCount * ratio);
                ce.TotalCount = Mathf.Max(0, scaled); // never negative
                asset.count[key] = ce;
                assets[i] = asset;
                assigned += ce.TotalCount;
            }

            // Give remainder to highest-weight asset
            int remainder = newBudget - assigned;
            if (remainder > 0 && highestIdx >= 0)
            {
                var asset = assets[highestIdx];
                var ce = asset.count[key];
                ce.TotalCount += remainder;
                asset.count[key] = ce;
                assets[highestIdx] = asset;
            }
        }
    }
}
