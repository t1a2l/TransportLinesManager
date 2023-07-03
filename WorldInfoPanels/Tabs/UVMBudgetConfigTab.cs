using ColossalFramework.UI;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Components;
using System.Collections.Generic;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Tabs
{

    public class UVMBudgetConfigTab : TLMBaseTimedConfigTab<UVMBudgetConfigTab, UVMBudgetTimeChart, UVMBudgetEditorLine, BudgetEntryXml>
    {
        private UICheckBox m_showAbsoluteCheckbox;
        public override string GetTitleLocale() => "K45_TLM_PER_HOUR_BUDGET_TITLE";
        public override string GetValueColumnLocale() => "K45_TLM_BUDGET";
        public override float GetMaxSliderValue() => 500;
        public override void ExtraAwake()
        {
            m_showAbsoluteCheckbox = m_uiHelper.AddCheckboxLocale("K45_TLM_SHOW_ABSOLUTE_VALUE", false, (x) =>
            {
                if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding))
                {
                    if (!fromBuilding)
                    {
                        TLMTransportLineExtension.Instance.SetDisplayAbsoluteValues(lineId, x);
                        RebuildList();
                    }
                }              
            });
            KlyteMonoUtils.LimitWidthAndBox(m_showAbsoluteCheckbox.label, m_uiHelper.Self.width - 40f);
        }

        public override void ExtraOnSetTarget(ushort lineID)
        {
            m_showAbsoluteCheckbox.isVisible = TLMTransportLineExtension.Instance.IsUsingCustomConfig(lineID);
            m_showAbsoluteCheckbox.isChecked = TLMTransportLineExtension.Instance.IsDisplayAbsoluteValues(lineID);
        }

        internal override List<Color> ColorOrder { get; } = new List<Color>()
        {
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
        };

        public static bool IsAbsoluteValue() => Instance.m_showAbsoluteCheckbox.isChecked;
        protected override TimeableList<BudgetEntryXml> Config
            => UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && !fromBuilding
            ? TLMLineUtils.GetEffectiveConfigForLine(lineId).BudgetEntries
            : null;


        protected override BudgetEntryXml DefaultEntry() => new BudgetEntryXml()
        {
            HourOfDay = 0,
            Value = 100
        };
        public override string GetTemplateName() => UVMBudgetEditorLine.BUDGET_LINE_TEMPLATE;
        public override void EnsureTemplate() => UVMBudgetEditorLine.EnsureTemplate();
    }
}