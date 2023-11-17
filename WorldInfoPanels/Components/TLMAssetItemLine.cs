using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Extensions.UI;
using Commons.Utils;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Interfaces;
using TransportLinesManager.UI;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Tabs;
using System;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components
{
    public class TLMAssetItemLine : UICustomControl
    {
        public const string TEMPLATE_NAME = "TLM_AssetSelectionTabLineTemplate";
        private bool m_isLoading;
        private UICheckBox m_checkbox;
        private UITextField m_capacityEditor;
        private string m_currentAsset;
        public Action OnMouseEnter;

        public void Awake()
        {
            m_checkbox = Find<UICheckBox>("AssetCheckbox");
            m_capacityEditor = Find<UITextField>("Cap");
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
                        extension.AddAssetToLine(fromBuilding ? (ushort)0 : lineId, m_currentAsset);
                    }
                    else
                    {
                        extension.RemoveAssetFromLine(fromBuilding ? (ushort)0 : lineId, m_currentAsset);
                    }
                }
            };
            MonoUtils.LimitWidthAndBox(m_checkbox.label, 265, out UIPanel container);
            container.relativePosition = new Vector3(container.relativePosition.x, 0);
            m_capacityEditor.eventTextSubmitted += CapacityEditor_eventTextSubmitted;

            m_checkbox.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();
            m_capacityEditor.eventMouseEnter += (x, y) => OnMouseEnter?.Invoke();
        }

        public void SetAsset(string assetName, bool isAllowed)
        {
            m_isLoading = true;
            m_currentAsset = assetName;
            m_checkbox.label.text = Locale.GetUnchecked("VEHICLE_TITLE", assetName);
            m_checkbox.isChecked = isAllowed;
            var info = PrefabCollection<VehicleInfo>.FindLoaded(m_currentAsset);
            var tsd = TransportSystemDefinition.From(info);
            UpdateMaintenanceCost(info, tsd);
            m_capacityEditor.text = VehicleUtils.GetCapacity(PrefabCollection<VehicleInfo>.FindLoaded(assetName)).ToString("0");
            m_isLoading = false;
        }

        private void CapacityEditor_eventTextSubmitted(UIComponent x, string y)
        {
            if (m_isLoading || !int.TryParse(y.IsNullOrWhiteSpace() ? "0" : y, out int value))
            {
                return;
            }
            VehicleInfo info = PrefabCollection<VehicleInfo>.FindLoaded(m_currentAsset);
            var tsd = TransportSystemDefinition.From(info);
            tsd.GetTransportExtension().SetVehicleCapacity(m_currentAsset, value);
            m_capacityEditor.text = VehicleUtils.GetCapacity(info).ToString("0");
            UpdateMaintenanceCost(info, tsd);

            UVMPublicTransportWorldInfoPanel.MarkDirty(typeof(TLMAssetSelectorTab));
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
            uiCheckbox.width = 285f;
            uiCheckbox.label.processMarkup = true;
            uiCheckbox.label.textScale = 0.8f;
            uiCheckbox.label.verticalAlignment = UIVerticalAlignment.Middle;
            uiCheckbox.label.minimumSize = new Vector2(0, 32);

            MonoUtils.CreateUIElement(out UITextField capEditField, panel.transform, "Cap", new Vector4(0, 0, 50, 32));
            MonoUtils.UiTextFieldDefaults(capEditField);
            MonoUtils.InitButtonFull(capEditField, false, "OptionsDropboxListbox");
            capEditField.isTooltipLocalized = true;
            capEditField.tooltip = Locale.Get("TLM_ASSET_CAPACITY_FIELD_DESCRIPTION");
            capEditField.numericalOnly = true;
            capEditField.maxLength = 5;
            capEditField.padding = new RectOffset(2, 2, 9, 2);

            go.AddComponent<TLMAssetItemLine>();
            TLMUiTemplateUtils.GetTemplateDict()[TEMPLATE_NAME] = panel;
        }
    }

}

