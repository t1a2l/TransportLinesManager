using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Extensions.UI;
using Commons.Utils;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Interfaces;
using TransportLinesManager.Utils;
using System;
using UnityEngine;
using TransportLinesManager.Data.DataContainers;
using System.Collections.Generic;
using ColossalFramework;
using TransportLinesManager.WorldInfoPanels.Tabs;
using System.Linq;

namespace TransportLinesManager.WorldInfoPanels.Components
{
    public class TLMAssetItemLine : UICustomControl
    {
        public const string TEMPLATE_NAME = "TLM_AssetSelectionTabLineTemplate";
        private bool m_isLoading;
        private UICheckBox m_checkbox;
        private UITextField m_capacityEditor;
        private UITextField m_weightEditor;
        private UILabel m_usedCount;
        private string m_currentAsset;
        public Action OnMouseEnter;

        public void Awake()
        {
            var panel = GetComponent<UIPanel>();
            m_checkbox = panel.GetComponentInChildren<UICheckBox>();

            m_capacityEditor = panel.Find<UITextField>("Cap");
            m_weightEditor = panel.Find<UITextField>("Weg");
            m_usedCount = panel.Find<UILabel>("UsedCount");

            m_checkbox.eventCheckChanged += (x, y) =>
            {
                if (m_isLoading)
                {
                    return;
                }

                if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding))
                {
                    IBasicExtension extension = lineId > 0 && !fromBuilding ? TLMLineUtils.GetEffectiveExtensionForLine(lineId) : UVMPublicTransportWorldInfoPanel.GetCurrentTSD().GetTransportExtension();

                    LogUtils.DoLog($"checkbox event: {x.objectUserData} => {y} at {extension}[{lineId}-{fromBuilding}]");
                    if (y)
                    {
                        extension.AddAssetToLine(fromBuilding ? (ushort)0 : lineId, m_currentAsset, m_capacityEditor.text, m_weightEditor.text);
                    }
                    else
                    {
                        extension.RemoveAssetFromLine(fromBuilding ? (ushort)0 : lineId, m_currentAsset);
                    }

                    List<TransportAsset> allowedTransportAssets = extension.GetAssetTransportListForLine(fromBuilding ? (ushort)0 : lineId);
                    var index = TLMAssetSelectorTab.GetBudgetSelectedIndex();
                    if (index == -1)
                    {
                        var hourIndex = TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetActiveBudgetEntries(lineId).GetAtHourExact(TLMLineUtils.ReferenceTimer).Second;
                        index = hourIndex != -1 ? hourIndex : 0;
                    }
                    bool isAllowed = allowedTransportAssets.Any(item => item.name == m_currentAsset);
                    TransportAsset asset = isAllowed ? allowedTransportAssets.Find(item => item.name == m_currentAsset) : new TransportAsset { name = m_currentAsset };
                    SetAsset(asset, isAllowed, fromBuilding ? (ushort)0 : lineId, index);
                }
            };
            MonoUtils.LimitWidthAndBox(m_checkbox.label, 225, out UIPanel container);
            container.relativePosition = new Vector3(container.relativePosition.x, 0);
            m_capacityEditor.eventTextSubmitted += CapacityEditor_eventTextSubmitted;
            m_weightEditor.eventTextSubmitted += WeightEditor_eventTextSubmitted;
            m_usedCount.text = "0";

            m_checkbox.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();
            m_capacityEditor.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();
            m_weightEditor.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();
        }

        public void SetAsset(TransportAsset asset, bool isAllowed, ushort lineId, int index)
        {
            m_isLoading = true;
            m_currentAsset = asset.name;
            m_checkbox.label.text = Locale.GetUnchecked("VEHICLE_TITLE", asset.name);
            m_checkbox.isChecked = isAllowed;

            var info = PrefabCollection<VehicleInfo>.FindLoaded(m_currentAsset);
            var tsd = TransportSystemDefinition.From(info);
            UpdateMaintenanceCost(info, tsd);

            bool isIntercity = lineId == 0;
            m_weightEditor.isVisible = !isIntercity;

            var lineExt = TLMTransportLineExtension.Instance;
            bool isCustomConfig = !isIntercity && lineExt.IsUsingCustomConfig(lineId);
            bool isAbsolute = isCustomConfig && lineExt.IsDisplayAbsoluteValues(lineId);

            if (isAllowed)
            {
                m_weightEditor.isInteractive = true;
                m_weightEditor.opacity = 1f;
                m_usedCount.isVisible = true;

                bool isActiveSlot = false;
                if (!isIntercity)
                {
                    int activeSlot = TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetActiveBudgetEntries(lineId).GetAtHourExact(TLMLineUtils.ReferenceTimer).Second;
                    isActiveSlot = index == activeSlot;
                }

                if (isActiveSlot)
                {
                    TLMLineUtils.EnsureUsedCountSlotSynchronized(lineId, index);
                    m_usedCount.opacity = 1f;
                    m_usedCount.text = TLMLineUtils.GetRuntimeUsedCount(lineId, index, asset.name).ToString();
                    m_usedCount.tooltip = Locale.Get("TLM_ASSET_USED_LABEL_DESCRIPTION");
                }
                else
                {
                    m_usedCount.opacity = 0.3f;
                    m_usedCount.text = "-";
                    m_usedCount.tooltip = null;
                }

                if (isAbsolute)
                {
                    m_weightEditor.text = asset.count.ContainsKey(index.ToString()) ? asset.count[index.ToString()].TotalCount.ToString() : "0"; 
                }
                else
                {
                    m_weightEditor.text = asset.spawn_percent.ContainsKey(index.ToString()) ? asset.spawn_percent[index.ToString()].Value.ToString() : "100";
                }
            }
            else
            {
                m_weightEditor.isInteractive = false;
                m_weightEditor.opacity = 0.3f;
                m_weightEditor.text = "0";
                m_usedCount.isVisible = false;
            }

            m_capacityEditor.text = asset.capacity != 0 ? asset.capacity.ToString() : VehicleUtils.GetCapacity(info).ToString("0");

            if (isAbsolute)
            {
                m_weightEditor.tooltip = Locale.Get("TLM_ASSET_COUNT_FIELD_DESCRIPTION");
            }
            else
            {
                m_weightEditor.tooltip = Locale.Get("TLM_ASSET_WEIGHT_FIELD_DESCRIPTION");
            }

            m_isLoading = false;
        }

        public void RefreshUsageDisplay(ushort lineId, int index)
        {
            if (lineId == 0 || string.IsNullOrEmpty(m_currentAsset))
            {
                m_usedCount.text = string.Empty;
                return;
            }

            bool isActiveSlot = index == TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetActiveBudgetEntries(lineId).GetAtHourExact(TLMLineUtils.ReferenceTimer).Second;
            if (!isActiveSlot)
            {
                m_usedCount.opacity = 0.3f;
                m_usedCount.text = "-";
                m_usedCount.tooltip = null;
                return;
            }

            TLMLineUtils.EnsureUsedCountSlotSynchronized(lineId, index);
            m_usedCount.opacity = 1f;
            m_usedCount.text = TLMLineUtils.GetRuntimeUsedCount(lineId, index, m_currentAsset).ToString();
            m_usedCount.tooltip = Locale.Get("TLM_ASSET_USED_LABEL_DESCRIPTION");
        }

        private void CapacityEditor_eventTextSubmitted(UIComponent x, string y)
        {
            if (m_isLoading || !int.TryParse(y.IsNullOrWhiteSpace() ? "0" : y, out int value))
            {
                return;
            }
            VehicleInfo info = PrefabCollection<VehicleInfo>.FindLoaded(m_currentAsset);
            var tsd = TransportSystemDefinition.From(info);
            if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding))
            {
                if(!fromBuilding)
                {
                    IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, tsd);
                    List<TransportAsset> allowedTransportAssets = config.GetAssetTransportListForLine(lineId);
                    
                    if(allowedTransportAssets.Any(item => item.name == m_currentAsset))
                    {
                        var asset_index = allowedTransportAssets.FindIndex(item => item.name == m_currentAsset);
                        var asset = allowedTransportAssets[asset_index];
                        asset.capacity = value;
                        allowedTransportAssets[asset_index] = asset;
                        config.SetAssetTransportListForLine(lineId, allowedTransportAssets);
                        tsd.GetTransportExtension().SetVehicleCapacity(m_currentAsset, value);
                        m_capacityEditor.text = VehicleUtils.GetCapacity(info).ToString("0");
                        UpdateMaintenanceCost(info, tsd);
                        TLMAssetSelectorTab.MarkDirty();
                    }
                }
            }
        }

        private void WeightEditor_eventTextSubmitted(UIComponent x, string y)
        {
            if (m_isLoading || !int.TryParse(y.IsNullOrWhiteSpace() ? "0" : y, out int value))
            {
                return;
            }
            VehicleInfo info = PrefabCollection<VehicleInfo>.FindLoaded(m_currentAsset);
            var tsd = TransportSystemDefinition.From(info);
            if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding))
            {
                if (!fromBuilding)
                {
                    IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, tsd);
                    List<TransportAsset> allowedTransportAssets = config.GetAssetTransportListForLine(lineId);
                    var budgetEntries = config.GetActiveBudgetEntries(lineId);

                    if (allowedTransportAssets.Any(item => item.name == m_currentAsset))
                    {
                        IBasicExtensionStorage currentConfig = TLMLineUtils.GetEffectiveConfigForLine(lineId);
                        var asset_index = allowedTransportAssets.FindIndex(item => item.name == m_currentAsset);
                        if (asset_index == -1)
                        {
                            return;
                        }
                        var asset = allowedTransportAssets[asset_index];
                        var index = TLMAssetSelectorTab.GetBudgetSelectedIndex();
                        if (index == -1)
                        {
                            var hourIndex = budgetEntries.GetAtHourExact(TLMLineUtils.ReferenceTimer).Second;
                            index = hourIndex != -1 ? hourIndex : 0;
                        }

                        var lineExt = TLMTransportLineExtension.Instance;
                        bool isAbsolute = lineExt.IsUsingCustomConfig(lineId) && lineExt.IsDisplayAbsoluteValues(lineId);

                        if (isAbsolute)
                        {
                            float budgetPercent = budgetEntries[index].Value / 100f;
                            float lineLength = TransportManager.instance.m_lines.m_buffer[lineId].m_totalLength;
                            TransportInfo transportInfo = TransportManager.instance.m_lines.m_buffer[lineId].Info;
                            int maxVehicles = TLMLineUtils.ProjectTargetVehicleCount(transportInfo, lineLength, budgetPercent);

                            int otherTotal = 0;
                            for (int i = 0; i < allowedTransportAssets.Count; i++)
                            {
                                if (allowedTransportAssets[i].name != m_currentAsset)
                                {
                                    otherTotal += allowedTransportAssets[i].count.ContainsKey(index.ToString()) ? allowedTransportAssets[i].count[index.ToString()].TotalCount : 0;
                                }
                            }

                            int remaining = maxVehicles - otherTotal;
                            if (value > remaining)
                            {
                                value = Mathf.Clamp(value, 0, remaining);
                            }

                            var item_count = asset.count.ContainsKey(index.ToString()) ? asset.count[index.ToString()] : new CountEntry();
                            item_count.TotalCount = value;
                            asset.count[index.ToString()] = item_count;

                            allowedTransportAssets[asset_index] = asset; // update list first so ReconcileOverAssigned sees new value
                            int newBudget = TLMCountModeUtils.ReconcileOverAssigned(allowedTransportAssets, index.ToString(), maxVehicles);
                            if (newBudget > maxVehicles)
                            {
                                m_weightEditor.tooltip = string.Format(Locale.Get("TLM_BUDGET_RAISED_TO_MATCH_TOOLTIP"), newBudget);
                            }
                            else
                            {
                                m_weightEditor.tooltip = Locale.Get("TLM_ASSET_COUNT_FIELD_TOOLTIP");
                            }
                        }
                        else
                        {
                            value = Mathf.Clamp(value, 0, 100);
                            asset.spawn_percent[index.ToString()] = new SpawnPercentEntry { Value = value };
                        }
                        allowedTransportAssets[asset_index] = asset;
                        config.SetAssetTransportListForLine(lineId, allowedTransportAssets);
                        m_weightEditor.text = value.ToString("0");
                        UpdateMaintenanceCost(info, tsd);
                        TLMAssetSelectorTab.MarkDirty();
                    }
                }
            }
        }

        private void UpdateMaintenanceCost(VehicleInfo info, TransportSystemDefinition tsd)
        {
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
            m_checkbox.label.suffix = lineId == 0 || fromBuilding ? "" : $"\n<color #aaaaaa>{LocaleFormatter.FormatUpkeep(Mathf.RoundToInt(VehicleUtils.GetCapacity(info) * tsd.GetEffectivePassengerCapacityCost() * 100), false)}</color>";
        }

        public static void EnsureTemplate()
        {
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(290, 32);
            panel.autoLayout = true;
            panel.wrapLayout = false;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;

            UICheckBox uiCheckbox = UIHelperExtension.AddCheckbox(panel, "AAAAAA", false);
            uiCheckbox.name = "AssetCheckbox";
            uiCheckbox.height = 32;
            uiCheckbox.width = 220f;
            uiCheckbox.label.processMarkup = true;
            uiCheckbox.label.textScale = 0.8f;
            uiCheckbox.label.verticalAlignment = UIVerticalAlignment.Middle;
            uiCheckbox.label.minimumSize = new Vector2(0, 24);

            MonoUtils.CreateUIElement(out UITextField capEditField, panel.transform, "Cap", new Vector4(0, 0, 50, 32));
            MonoUtils.UiTextFieldDefaults(capEditField);
            MonoUtils.InitButtonFull(capEditField, false, "OptionsDropboxListbox");
            capEditField.isTooltipLocalized = true;
            capEditField.tooltipLocaleID = "TLM_ASSET_CAPACITY_FIELD_DESCRIPTION";
            capEditField.tooltip = Locale.Get("TLM_ASSET_CAPACITY_FIELD_DESCRIPTION");
            capEditField.numericalOnly = true;
            capEditField.maxLength = 5;
            capEditField.padding = new RectOffset(2, 2, 9, 2);

            MonoUtils.CreateUIElement(out UITextField wegEditField, panel.transform, "Weg", new Vector4(0, 0, 50, 32));
            MonoUtils.UiTextFieldDefaults(wegEditField);
            MonoUtils.InitButtonFull(wegEditField, false, "OptionsDropboxListbox");
            wegEditField.isTooltipLocalized = true;
            wegEditField.tooltipLocaleID = "TLM_ASSET_WEIGHT_FIELD_DESCRIPTION";
            wegEditField.tooltip = Locale.Get("TLM_ASSET_WEIGHT_FIELD_DESCRIPTION");
            wegEditField.numericalOnly = true;
            wegEditField.maxLength = 5;
            wegEditField.padding = new RectOffset(2, 2, 9, 2);

            MonoUtils.CreateUIElement(out UILabel usedCountField, panel.transform, "UsedCount", new Vector4(0, 0, 30, 32));
            usedCountField.padding = new RectOffset(2, 2, 9, 2);
            usedCountField.textAlignment = UIHorizontalAlignment.Center;

            go.AddComponent<TLMAssetItemLine>();
            TLMUiTemplateUtils.GetTemplateDict()[TEMPLATE_NAME] = panel;
        }
    }

}

