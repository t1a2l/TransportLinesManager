using System.Collections.Generic;
using System.Linq;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Interfaces;
using UnityEngine;

namespace TransportLinesManager.Utils
{
    public static class TLMCountModeUtils
    {
        // Called when budget slider changes value in count mode
        public static void OnBudgetChangedInCountMode(ushort lineId, IBasicExtension config, int budgetIndex, int newBudget, int oldBudget)
        {
            List<TransportAsset> assets = config.GetAssetTransportListForLine(lineId);
            string key = budgetIndex.ToString();
            int currentSum = assets.Sum(a => a.count.TryGetValue(key, out var ce) ? ce.TotalCount : 0);

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

        private static void ScaleCountsDown(List<TransportAsset> assets, string key, int newBudget)
        {
            int currentSum = assets.Sum(a => a.count.TryGetValue(key, out var ce) ? ce.TotalCount : 0);
            if (currentSum == 0) return;

            float ratio = (float)newBudget / currentSum;
            int assigned = 0;

            // Find highest-weight (highest TotalCount) asset for remainder
            TransportAsset highestAsset = assets.OrderByDescending(a => a.count.TryGetValue(key, out var ce) ? ce.TotalCount : 0).First();

            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (!asset.count.TryGetValue(key, out var ce)) continue;
                int scaled = Mathf.FloorToInt(ce.TotalCount * ratio);
                scaled = Mathf.Max(0, scaled); // never negative
                ce.TotalCount = scaled;
                ce.UsedCount = Mathf.Min(ce.UsedCount, scaled);
                asset.count[key] = ce;
                assets[i] = asset;
                assigned += scaled;
            }

            // Give remainder to highest-weight asset
            int remainder = newBudget - assigned;
            if (remainder > 0)
            {
                int idx = assets.IndexOf(highestAsset); // by name match
                var ce = assets[idx].count[key];
                ce.TotalCount += remainder;
                var a = assets[idx]; a.count[key] = ce; assets[idx] = a;
            }
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
    }
}
