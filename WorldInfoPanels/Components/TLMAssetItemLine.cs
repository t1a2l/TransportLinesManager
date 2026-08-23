using ColossalFramework.Globalization;
using ColossalFramework.UI;
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
using static TransportLinesManager.Data.Extensions.ExtensionStaticExtensionMethods;

namespace TransportLinesManager.WorldInfoPanels.Components
{
    public class TLMAssetItemLine : UICustomControl
    {
        public const string TEMPLATE_NAME = "TLM_AssetSelectionTabLineTemplate";
        private bool m_isLoading;
        private UILabel m_assetNameLabel;
        private UITextField m_capacityEditor;
        private UITextField m_weightEditor;
        private UILabel m_usedCount;
        private string m_currentAsset;
        public Action OnMouseEnter;

        public void Awake()
        {
            var panel = GetComponent<UIPanel>();

            m_assetNameLabel = panel.Find<UILabel>("AssetName");
            m_capacityEditor = panel.Find<UITextField>("Cap");
            m_weightEditor = panel.Find<UITextField>("Weg");
            m_usedCount = panel.Find<UILabel>("UsedCount");

            m_capacityEditor.eventTextSubmitted += CapacityEditor_eventTextSubmitted;
            m_weightEditor.eventTextSubmitted += WeightEditor_eventTextSubmitted;
            m_usedCount.text = "0";

            m_assetNameLabel.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();
            m_capacityEditor.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();
            m_weightEditor.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();
        }

        public void SetAsset(TransportAsset asset, ushort lineId, int index)
        {
            m_isLoading = true;
            m_currentAsset = asset.name;

            var info = PrefabCollection<VehicleInfo>.FindLoaded(m_currentAsset);

            m_assetNameLabel.text = info != null ? PrefabUtils.GetDisplayName(info) : m_currentAsset;

            var tsd = TransportSystemDefinition.From(info);
            UpdateMaintenanceCost(info, tsd);

            bool isIntercity = lineId == 0;
            m_weightEditor.isVisible = !isIntercity;

            var lineExt = TLMTransportLineExtension.Instance;
            bool isCustomConfig = !isIntercity && lineExt.IsUsingCustomConfig(lineId);
            bool isAbsolute = isCustomConfig && lineExt.IsDisplayAbsoluteValues(lineId);
            bool autoAdjustEnabled = lineExt.IsAutoAdjustAbsoluteCountsEnabled(lineId);

            m_weightEditor.isInteractive = true;
            m_weightEditor.opacity = 1f;
            m_usedCount.isVisible = true;

            bool isActiveSlot = false;
            if (!isIntercity)
            {
                int activeSlot = GetRuntimeActiveSlotIndex(lineId);
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
                m_weightEditor.isInteractive = !autoAdjustEnabled;
                m_weightEditor.opacity = autoAdjustEnabled ? 0.7f : 1f;
            }
            else
            {
                m_weightEditor.text = asset.spawn_percent.ContainsKey(index.ToString()) ? asset.spawn_percent[index.ToString()].Value.ToString() : "100";
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

            bool isActiveSlot = index == GetRuntimeActiveSlotIndex(lineId);
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
                    var budgetEntries = config.GetBudgetsMultiplierForLine(lineId, TLMAssetSelectorTab.Instance.CurrentProfileTarget);

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
            if (m_assetNameLabel == null)
            {
                return;
            }

            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);

            if (lineId == 0 || fromBuilding || info == null || tsd == null)
            {
                m_assetNameLabel.suffix = string.Empty;
                return;
            }

            int upkeep = Mathf.RoundToInt(VehicleUtils.GetCapacity(info) * tsd.GetEffectivePassengerCapacityCost() * 100);

            m_assetNameLabel.processMarkup = true;
            m_assetNameLabel.suffix = $"\n<color #aaaaaa>{LocaleFormatter.FormatUpkeep(upkeep, false)}</color>";
        }

        public static void EnsureTemplate()
        {
            var go = new GameObject();
            UIPanel panel = go.AddComponent<UIPanel>();
            panel.size = new Vector2(290, 32);
            panel.autoLayout = true;
            panel.wrapLayout = false;
            panel.autoLayoutDirection = LayoutDirection.Horizontal;

            MonoUtils.CreateUIElement(out UILabel assetNameLabel, panel.transform, "AssetName", new Vector4(0, 0, 220, 32));
            assetNameLabel.autoSize = false;
            assetNameLabel.height = 32f;
            assetNameLabel.width = 220f;
            assetNameLabel.textScale = 0.8f;
            assetNameLabel.processMarkup = true;
            assetNameLabel.verticalAlignment = UIVerticalAlignment.Middle;
            assetNameLabel.wordWrap = true;
            assetNameLabel.padding = new RectOffset(4, 4, 1, 1);

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

        private static int GetRuntimeActiveSlotIndex(ushort lineId)
        {
            var activeEntries = TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetActiveBudgetEntries(lineId);
            return activeEntries?.GetAtHourExact(TLMLineUtils.ReferenceTimer).Second ?? -1;
        }
    }

}

