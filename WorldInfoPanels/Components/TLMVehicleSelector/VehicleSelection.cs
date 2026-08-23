using System.Collections.Generic;
using System.Linq;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.UI;
using Commons.UI.Components;
using Commons.Utils;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Tabs;
using UnityEngine;
using static TransportLinesManager.Data.Extensions.ExtensionStaticExtensionMethods;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    internal class VehicleSelection : UIPanel
    {
        /// <summary>
        /// Panel height.
        /// </summary>
        internal const float PanelHeight = VehicleListY + VehicleListHeight + Margin;

        /// <summary>
        /// List height.
        /// </summary>
        internal const float VehicleListHeight = 200f;

        /// <summary>
        /// Selection list column width.
        /// </summary>
        internal const float ListWidth = 340f;

        /// <summary>
        /// Preview column width.
        /// </summary>
        internal const float PreviewWidth = 150f;

        /// <summary>
        /// Panel width.
        /// </summary>
        internal const float PanelWidth = RightColumnX + ListWidth + Margin;

        // Layout constants - private.
        private const float Margin = 5f;
        private const float TitleOffsetY = 40f;
        private const float VehicleListY = 70f;
        private const float ArrowSize = 32f;
        private const float MidControlX = Margin + ListWidth + Margin;
        private const float RightColumnX = MidControlX + PreviewWidth + Margin;

        // Panel components.
        private UILabel m_titleLabel;
        private UIButton m_addButton;
        private UIButton m_removeButton;
        private UIButton m_addAllButton;
        private UIButton m_removeAllButton;
        private VehicleSelectionPanel m_vehicleSelectionPanel;
        private SelectedVehiclePanel m_selectedVehiclePanel;
        private UIPanel m_previewPanel;
        private UITextureSprite m_preview;
        private AVOPreviewRenderer m_previewRenderer;

        // Currently selected vehicles.
        private VehicleInfo m_selectedLineVehicle;
        private VehicleInfo m_selectedListVehicle;

        private VehicleInfo m_lastInfo;

        internal VehicleInfo SelectedLineVehicle
        {
            set
            {
                m_selectedLineVehicle = value;

                if (value != null)
                {
                    // Clear other vehicle list selection if this is active.
                    m_vehicleSelectionPanel.ClearSelection();
                    m_previewRenderer.RenderVehicle(value);
                }
                else
                {
                    // Null value set; clear list selection.
                    m_selectedVehiclePanel.ClearSelection();
                }

                // Update button states.
                UpdateButtonStates();
            }
        }

        internal VehicleInfo SelectedListVehicle
        {
            set
            {
                m_selectedListVehicle = value;

                if (value != null)
                {
                    // Clear other vehicle list selection if this is active.
                    m_selectedVehiclePanel.ClearSelection();
                    m_previewRenderer.RenderVehicle(value);
                }
                else
                {
                    // Null value set; clear list selection.
                    m_vehicleSelectionPanel.ClearSelection();
                }

                // Update button states.
                UpdateButtonStates();
            }
        }

        internal LinePanel ParentPanel { get; set; }

        internal ushort CurrentLine { get; private set; }

        public override void Awake()
        {
            base.Awake();

            // Set size.
            height = PanelHeight;
            width = PanelWidth;

            // Appearance.
            atlas = TextureAtlasUtils.DefaultTextureAtlas;
            backgroundSprite = "GenericPanelLight";
            color = new Color32(160, 160, 160, 255);

            // Title.
            m_titleLabel = UILabels.AddLabel(this, 0f, 10f, "Select vehicle", PanelWidth, 1f, UIHorizontalAlignment.Center);

            // 'Add vehicle' button.
            m_addButton = UIButtons.AddIconButton(
                this,
                RightColumnX - ArrowSize - Margin,
                VehicleListY,
                ArrowSize,
                TextureAtlasUtils.LoadQuadSpriteAtlas("__Add"),
                Locale.Get("ADD_VEHICLE_TIP"));
            m_addButton.isEnabled = false;
            m_addButton.eventClicked += (c, p) => AddVehicle(m_selectedListVehicle.name);

            // 'Add all vehicles' button.
            m_addAllButton = UIButtons.AddIconButton(
                this,
                RightColumnX - ArrowSize - Margin - ArrowSize - Margin,
                VehicleListY,
                ArrowSize,
                TextureAtlasUtils.LoadQuadSpriteAtlas("__AddAll"),
                Locale.Get("ADD_ALL_TIP"));
            m_addAllButton.isEnabled = false;
            m_addAllButton.eventClicked += (c, p) => AddAllVehicles();

            // Remove vehicle button.
            m_removeButton = UIButtons.AddIconButton(
                this,
                MidControlX,
                VehicleListY,
                ArrowSize,
                TextureAtlasUtils.LoadQuadSpriteAtlas("__Remove"),
                Locale.Get("REMOVE_VEHICLE_TIP"));
            m_removeButton.isEnabled = false;
            m_removeButton.eventClicked += (c, p) => RemoveVehicle(m_selectedListVehicle.name);

            // 'Remove all vehicles' button.
            m_removeAllButton = UIButtons.AddIconButton(
                this,
                MidControlX + ArrowSize + Margin,
                VehicleListY,
                ArrowSize,
                TextureAtlasUtils.LoadQuadSpriteAtlas("__RemoveAll"),
                Locale.Get("REMOVE_ALL_TIP"));
            m_removeAllButton.isEnabled = false;
            m_removeAllButton.eventClicked += (c, p) => RemoveAllVehicles();

            // Vehicle selection panels.
            m_selectedVehiclePanel = this.AddUIComponent<SelectedVehiclePanel>();
            m_selectedVehiclePanel.relativePosition = new Vector2(Margin, VehicleListY);
            m_selectedVehiclePanel.ParentPanel = this;
            m_vehicleSelectionPanel = this.AddUIComponent<VehicleSelectionPanel>();
            m_vehicleSelectionPanel.ParentPanel = this;
            m_vehicleSelectionPanel.relativePosition = new Vector2(RightColumnX, VehicleListY);

            // Vehicle selection list labels.
            UILabels.AddLabel(m_vehicleSelectionPanel.VehicleList, 0f, -TitleOffsetY, Locale.Get("AVAILABLE_VEHICLES"), ListWidth, 0.8f, UIHorizontalAlignment.Center);
            UILabels.AddLabel(m_selectedVehiclePanel.VehicleList, 0f, -TitleOffsetY, Locale.Get("SELECTED_VEHICLES"), ListWidth, 0.8f, UIHorizontalAlignment.Center);

            SetPreviewWindow();
        }

        public void SetTarget(ushort lineID)
        {
            // Ensure valid building.
            if (lineID != 0)
            {
                CurrentLine = lineID;
                m_titleLabel.text = Locale.Get("TLM_VEHICLE_MANAGEMENT_TITLE");

                // Regenerate lists and set button states..
                Refresh();
            }
        }

        public void Refresh()
        {
            m_selectedLineVehicle = null;
            m_selectedListVehicle = null;

            // Clear preview.
            m_previewRenderer.RenderVehicle(null);

            m_selectedVehiclePanel.RefreshList();
            m_vehicleSelectionPanel.RefreshList();

            UpdateButtonStates();
        }

        public void UpdateButtonStates()
        {
            // Null check.
            if (m_addAllButton != null)
            {
                FastList<object> selectionList = m_vehicleSelectionPanel?.VehicleList?.Data;
                FastList<object> selectedList = m_selectedVehiclePanel?.VehicleList?.Data;

                m_addButton.isEnabled = m_selectedListVehicle != null;
                m_addAllButton.isEnabled = selectionList != null && selectionList.m_size > 0;
                m_removeButton.isEnabled = m_selectedLineVehicle != null;
                m_removeAllButton.isEnabled = selectedList != null && selectedList.m_size > 0;
            }
        }

        private void AddVehicle(string assetName)
        {
            var lineId = CurrentLine;
            if (lineId == 0)
            {
                return;
            }

            var tsd = UVMPublicTransportWorldInfoPanel.GetCurrentTSD();

            var config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, tsd);

            var currentConfig = TLMLineUtils.GetEffectiveConfigForLine(CurrentLine);

            if (config.GetAssetTransportListForLine(lineId).Any(x => x.name == assetName))
            {
                return;
            }

            var info = PrefabCollection<VehicleInfo>.FindLoaded(assetName);

            if (info == null)
            {
                return;
            }

            int capacity = VehicleUtils.GetCapacity(info);

            ProfileTarget profile = TLMAssetSelectorTab.Instance.CurrentProfileTarget;

            config.AddAssetToLine(lineId, assetName, capacity.ToString(), "100", ProfileTarget.Weekday);

            if (currentConfig?.UseSeparateWeekendProfile == true)
            {
                config.AddAssetToLine(lineId, assetName, capacity.ToString(), "100", ProfileTarget.Weekend);
            }

            Refresh();
            TLMAssetSelectorTab.MarkDirty();
        }

        private void RemoveVehicle(string assetName)
        {
            var lineId = CurrentLine;
            if (lineId == 0)
            {
                return;
            }

            var tsd = UVMPublicTransportWorldInfoPanel.GetCurrentTSD();

            var config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, tsd);

            config.RemoveAssetFromLine(lineId, assetName);

            Refresh();
            TLMAssetSelectorTab.MarkDirty();
        }

        private void AddAllVehicles()
        {
            if (CurrentLine == 0)
            {
                return;
            }

            var extension = TLMLineUtils.GetEffectiveExtensionForLine(CurrentLine);

            var currentConfig = TLMLineUtils.GetEffectiveConfigForLine(CurrentLine);

            foreach (object entry in m_vehicleSelectionPanel.VehicleList.Data)
            {
                if (entry is not VehicleItem item || item.Info == null)
                {
                    continue;
                }

                int capacity = VehicleUtils.GetCapacity(item.Info);

                extension.AddAssetToLine(CurrentLine, item.Info.name, capacity.ToString(), "100", ProfileTarget.Weekday);

                if (currentConfig?.UseSeparateWeekendProfile == true)
                {
                    extension.AddAssetToLine(CurrentLine, item.Info.name, capacity.ToString(), "100", ProfileTarget.Weekend);
                }
            }

            Refresh();
            TLMAssetSelectorTab.MarkDirty();
        }

        private void RemoveAllVehicles()
        {
            if (CurrentLine == 0)
            {
                return;
            }

            var extension = TLMLineUtils.GetEffectiveExtensionForLine(CurrentLine);

            var vehiclesToRemove = new List<VehicleInfo>();

            foreach (object entry in m_selectedVehiclePanel.VehicleList.Data)
            {
                if (entry is VehicleItem item && item.Info != null)
                {
                    vehiclesToRemove.Add(item.Info);
                }
            }

            foreach (VehicleInfo vehicle in vehiclesToRemove)
            {
                extension.RemoveAssetFromLine(CurrentLine, vehicle.name);
            }

            Refresh();
            TLMAssetSelectorTab.MarkDirty();
        }

        private void SetPreviewWindow()
        {
            MonoUtils.CreateUIElement(out m_previewPanel, this.transform);
            m_previewPanel.backgroundSprite = "GenericPanel";
            m_previewPanel.width = 150;
            m_previewPanel.height = 150;
            m_previewPanel.relativePosition = new Vector3(MidControlX, VehicleListY + ArrowSize + Margin);
            MonoUtils.CreateUIElement(out m_preview, m_previewPanel.transform);
            m_preview.size = m_previewPanel.size;
            m_preview.relativePosition = new Vector3(10, 0);
            MonoUtils.CreateElement(out m_previewRenderer, this.transform);
            m_previewRenderer.Size = m_preview.size * 2f;
            m_preview.texture = m_previewRenderer.Texture;
            m_previewRenderer.Zoom = 3;
            m_previewRenderer.CameraRotation = 40;
        }

    }
}
