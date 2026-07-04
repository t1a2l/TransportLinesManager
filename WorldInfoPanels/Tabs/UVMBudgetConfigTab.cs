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
using static TransportLinesManager.Data.Extensions.ExtensionStaticExtensionMethods;
using ColossalFramework.Globalization;
using Commons.Extensions.UI;

namespace TransportLinesManager.WorldInfoPanels.Tabs
{
    public class UVMBudgetConfigTab : TLMBaseTimedConfigTab<UVMBudgetConfigTab, UVMBudgetTimeChart, UVMBudgetEditorLine, BudgetEntryXml>
    {
        private UICheckBox m_showAbsoluteCheckbox;

        private UICheckBox m_useSeparateWeekendBudgetCheckbox;

        private UILabel m_budgetProfileLabel;

        private UIDropDown m_budgetProfileDropdown;

        private bool m_isLoading;

        private bool m_editingWeekendBudget;

        public override string GetTitleLocale() => "TLM_PER_HOUR_BUDGET_TITLE";

        public override string GetValueColumnLocale() => "TLM_BUDGET";

        public override float GetMaxSliderValue() => 500;

        public BudgetTarget CurrentBudgetTarget => m_editingWeekendBudget ? BudgetTarget.Weekend : BudgetTarget.Weekday;

        public override void ExtraAwake()
        {
            m_showAbsoluteCheckbox = m_uiHelper.AddCheckboxLocale("TLM_SHOW_ABSOLUTE_VALUE", false, (isAbsolute) =>
            {
                if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && !fromBuilding)
                {
                    TLMTransportLineExtension.Instance.SetDisplayAbsoluteValues(lineId, isAbsolute);

                    IBasicExtension ext = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
                    List<TransportAsset> assets = ext.GetAssetTransportListForLine(lineId);

                    var budgetEntries = ext.GetActiveBudgetEntries(lineId);

                    for (int i = 0; i < budgetEntries.Count; i++)
                    {
                        string key = i.ToString();
                        float pct = budgetEntries[i].Value / 100f;
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
                    TLMAssetSelectorTab.MarkDirty();
                }
            });
            MonoUtils.LimitWidthAndBox(m_showAbsoluteCheckbox.label, m_uiHelper.Self.width - 40f);
            CreateWeekendBudgetControls(this.component);
        }

        public override void ExtraOnSetTarget(ushort lineID)
        {
            m_isLoading = true;  // prevent eventCheckChanged from firing

            var lineExt = TLMTransportLineExtension.Instance;
            var cfg = TLMLineUtils.GetEffectiveConfigForLine(lineID);

            m_showAbsoluteCheckbox.isVisible = lineExt.IsUsingCustomConfig(lineID);
            m_showAbsoluteCheckbox.isChecked = lineExt.IsDisplayAbsoluteValues(lineID);
            m_useSeparateWeekendBudgetCheckbox.isVisible = TLMController.IsRealTimeEnabled && RealTimeUtils.IsWeekendEnabled();
            m_useSeparateWeekendBudgetCheckbox.isChecked = cfg.UseSeparateWeekendBudget;

            m_budgetProfileLabel.relativePosition = new Vector3(0f, 22f);
            m_budgetProfileDropdown?.relativePosition = new Vector3(130f, 18f);

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
        {
            get
            {
                if (!TryGetCurrentLineConfig(out ushort _, out IBudgetStorage cfg))
                {
                    return null;
                }

                return CurrentBudgetTarget == BudgetTarget.Weekend ? (cfg.WeekendBudgetEntries ?? []) : cfg.BudgetEntries;
            }
        }

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

        private void CreateWeekendBudgetControls(UIComponent parent)
        {
            m_useSeparateWeekendBudgetCheckbox = m_uiHelper.AddCheckboxLocale("TLM_USE_SEPARATE_WEEKEND_BUDGET", false, OnUseSeparateWeekendBudgetChanged);
            m_useSeparateWeekendBudgetCheckbox.name = "UseSeparateWeekendBudget";
            m_useSeparateWeekendBudgetCheckbox.relativePosition = new Vector3(12f, parent.height - 52f);

            MonoUtils.CreateUIElement(out m_budgetProfileLabel, parent.transform);
            m_budgetProfileLabel.name = "BudgetProfileLabel";
            m_budgetProfileLabel.text = Locale.Get("TLM_BUDGET_PROFILE");
            m_budgetProfileLabel.textScale = 0.9f;
            m_budgetProfileLabel.autoSize = false;
            m_budgetProfileLabel.width = 120f;
            m_budgetProfileLabel.height = 22f;
            m_budgetProfileLabel.relativePosition = new Vector3(0f, 22f);

            var ddGo = Instantiate(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel>().Find<UIDropDown>("Dropdown").gameObject, parent.transform);

            m_budgetProfileDropdown = ddGo.GetComponent<UIDropDown>();
            m_budgetProfileDropdown.name = "BudgetProfileDropdown";
            m_budgetProfileDropdown.width = 140f;
            m_budgetProfileDropdown.height = 24f;
            m_budgetProfileDropdown.textScale = 0.85f;
            m_budgetProfileDropdown.itemHeight = 24;
            m_budgetProfileDropdown.textFieldPadding = new RectOffset(6, 6, 4, 4);
            m_budgetProfileDropdown.itemPadding = new RectOffset(6, 6, 2, 2);
            m_budgetProfileDropdown.normalBgSprite = "OptionsDropboxListbox";
            m_budgetProfileDropdown.items =
            [
                Locale.Get("TLM_BUDGET_PROFILE_WEEKDAY"),
                Locale.Get("TLM_BUDGET_PROFILE_WEEKEND")
            ];
            m_budgetProfileDropdown.selectedIndex = 0;
            m_budgetProfileDropdown.relativePosition = new Vector3(130f, 18f);
            m_budgetProfileDropdown.eventSelectedIndexChanged += OnBudgetProfileChanged;

            UpdateWeekendBudgetUiState();
        }

        private void OnUseSeparateWeekendBudgetChanged(bool value)
        {
            if (!TryGetCurrentLineConfig(out ushort _, out IBudgetStorage cfg))
            {
                return;
            }

            cfg.UseSeparateWeekendBudget = value;

            if (value && (cfg.WeekendBudgetEntries == null || cfg.WeekendBudgetEntries.Count == 0))
            {
                cfg.WeekendBudgetEntries = CloneBudgetEntries(cfg.BudgetEntries);
            }

            if (!value)
            {
                m_editingWeekendBudget = false;
                m_budgetProfileDropdown?.selectedIndex = 0;
                m_budgetProfileDropdown.relativePosition = new Vector3(130f, 18f);
            }

            UpdateWeekendBudgetUiState();
            ReloadBudgetListFromCurrentProfile();
            MarkDirty();
        }

        private void OnBudgetProfileChanged(UIComponent component, int value)
        {
            m_editingWeekendBudget = value == 1;
            ReloadBudgetListFromCurrentProfile();
            UpdateWeekendBudgetUiState();
        }

        private void UpdateWeekendBudgetUiState()
        {
            bool enabled = false;

            if (TryGetCurrentLineConfig(out ushort _, out IBudgetStorage cfg))
            {
                enabled = cfg?.UseSeparateWeekendBudget == true;
            }

            m_budgetProfileLabel?.isVisible = enabled;
            m_budgetProfileDropdown?.isVisible = enabled;

            m_budgetProfileLabel.relativePosition = new Vector3(0f, 22f);
            m_budgetProfileDropdown?.relativePosition = new Vector3(130f, 18f);
        }

        private TimeableList<BudgetEntryXml> CloneBudgetEntries(TimeableList<BudgetEntryXml> src)
        {
            var result = new TimeableList<BudgetEntryXml>();

            if (src == null || src.Count == 0)
            {
                result.Add(new BudgetEntryXml
                {
                    HourOfDay = 0,
                    Value = 100
                });
                return result;
            }

            for (int i = 0; i < src.Count; i++)
            {
                result.Add(new BudgetEntryXml
                {
                    HourOfDay = src[i].HourOfDay,
                    Value = src[i].Value
                });
            }

            return result;
        }

        private void ReloadBudgetListFromCurrentProfile()
        {
            RebuildList();
        }

        private bool TryGetCurrentLineConfig(out ushort lineId, out IBudgetStorage cfg)
        {
            cfg = null;
            if (!UVMPublicTransportWorldInfoPanel.GetLineID(out lineId, out bool fromBuilding) || fromBuilding)
            {
                return false;
            }

            cfg = TLMLineUtils.GetEffectiveConfigForLine(lineId);
            return cfg != null;

        }
    }
}