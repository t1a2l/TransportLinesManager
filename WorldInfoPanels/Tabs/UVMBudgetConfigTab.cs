using ColossalFramework.UI;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Components;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TransportLinesManager.Interfaces;
using TransportLinesManager.Data.Extensions;

namespace TransportLinesManager.WorldInfoPanels.Tabs
{

    public class UVMBudgetConfigTab : TLMBaseTimedConfigTab<UVMBudgetConfigTab, UVMBudgetTimeChart, UVMBudgetEditorLine, BudgetEntryXml>
    {
        private UICheckBox m_showAbsoluteCheckbox;

        private bool m_isLoading;

        public override string GetTitleLocale() => "TLM_PER_HOUR_BUDGET_TITLE";

        public override string GetValueColumnLocale() => "TLM_BUDGET";

        public override float GetMaxSliderValue() => 500;

        public override void ExtraAwake()
        {
            m_showAbsoluteCheckbox = m_uiHelper.AddCheckboxLocale("TLM_SHOW_ABSOLUTE_VALUE", false, (isAbsolute) =>
            {
                if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && !fromBuilding)
                {
                    TLMTransportLineExtension.Instance.SetDisplayAbsoluteValues(lineId, isAbsolute);

                    IBasicExtension ext = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
                    List<TransportAsset> assets = ext.GetAssetTransportListForLine(lineId);
                    IBasicExtensionStorage cfg = TLMLineUtils.GetEffectiveConfigForLine(lineId);

                    for (int i = 0; i < cfg.BudgetEntries.Count; i++)
                    {
                        string key = i.ToString();
                        float pct = cfg.BudgetEntries[i].Value / 100f;
                        int budget = TLMLineUtils.ProjectTargetVehicleCount(
                            TransportManager.instance.m_lines.m_buffer[lineId].Info,
                            TransportManager.instance.m_lines.m_buffer[lineId].m_totalLength, pct);

                        if (m_isLoading) return;

                        if (isAbsolute) // percent → count
                        {
                            TLMCountModeUtils.ConvertPercentToCount(assets, key, budget); 
                        }
                        else // count → percent
                        { 
                            TLMCountModeUtils.ConvertCountToPercent(assets, key, budget); 
                        }
                    }

                    ext.SetAssetTransportListForLine(lineId, assets);
                    RebuildList();
                    UVMPublicTransportWorldInfoPanel.MarkDirty(typeof(TLMAssetSelectorTab));
                }
            });
            MonoUtils.LimitWidthAndBox(m_showAbsoluteCheckbox.label, m_uiHelper.Self.width - 40f);
        }

        public override void ExtraOnSetTarget(ushort lineID)
        {
            m_isLoading = true;  // prevent eventCheckChanged from firing
            m_showAbsoluteCheckbox.isVisible = TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineID);
            m_showAbsoluteCheckbox.isChecked = TLMTransportLineExtension.Instance.IsDisplayAbsoluteValues(lineID);
            m_isLoading = false;
        }

        internal override List<Color> ColorOrder { get; } =
        [
            Color.red,
            Color.Lerp(Color.red,Color.yellow,0.5f),
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.Lerp(Color.blue,Color.cyan,0.5f),
            Color.blue,
            Color.Lerp(Color.blue,Color.magenta,0.5f),
            Color.magenta,
            Color.Lerp(Color.red,Color.magenta,0.5f),
        ];

        public static bool IsAbsoluteValue() => Instance.m_showAbsoluteCheckbox.isChecked;

        protected override TimeableList<BudgetEntryXml> Config
            => UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && !fromBuilding
            ? TLMLineUtils.GetEffectiveConfigForLine(lineId).BudgetEntries
            : null;

        protected override BudgetEntryXml DefaultEntry()
        {
            // find first hour not already used
            var usedHours = new HashSet<int>(Config.Select(x => x.HourOfDay ?? -1));
            int availableHour = Enumerable.Range(0, 24).FirstOrDefault(h => !usedHours.Contains(h));

            return new()
            {
                HourOfDay = availableHour >= 0 ? availableHour : 0,
                Value = 100
            };
        }

        public override string GetTemplateName() => UVMBudgetEditorLine.BUDGET_LINE_TEMPLATE;

        public override void EnsureTemplate() => UVMBudgetEditorLine.EnsureTemplate();
    }
}