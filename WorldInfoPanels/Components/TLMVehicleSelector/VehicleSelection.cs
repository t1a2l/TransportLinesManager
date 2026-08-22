using System;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.UI;
using Commons.UI.Components;
using Commons.Utils;
using TransportLinesManager.Interfaces;
using TransportLinesManager.Utils;
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
        private VehicleInfo m_selectedBuildingVehicle;
        private VehicleInfo m_selectedListVehicle;

        /// <summary>
        /// Sets the currently selected vehicle from the list of currently selected vehicles.
        /// </summary>
        internal VehicleInfo SelectedBuildingVehicle
        {
            set
            {
                m_selectedBuildingVehicle = value;

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

        /// <summary>
        /// Sets the currently selected vehicle from the list of all currently unselected vehicles.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the parent tab reference.
        /// </summary>
        internal LinePanel ParentPanel { get; set; }

        /// <summary>
        /// Gets the current transfer reason.
        /// </summary>
        internal TransferManager.TransferReason TransferReason { get; private set; }

        /// <summary>
        /// Gets the currently selected line.
        /// </summary>
        internal ushort CurrentLine { get; private set; }

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
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
            m_addButton.eventClicked += (c, p) => AddVehicle(m_selectedListVehicle);

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
            m_removeButton.eventClicked += (c, p) => RemoveVehicle();

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

        /// <summary>
        /// Sets/changes the currently selected building.
        /// </summary>
        /// <param name="lineID">New building ID.</param>
        /// <param name="title">Selection list title string.</param>
        /// <param name="reason">Transfer reason for this vehicle selection.</param>
        internal void SetTarget(ushort lineID, string title, TransferManager.TransferReason reason)
        {
            // Ensure valid building.
            if (lineID != 0)
            {
                CurrentLine = lineID;
                TransferReason = reason;
                m_titleLabel.text = title;

                // Regenerate lists and set button states..
                Refresh();
            }
        }

        /// <summary>
        /// Refreshes list contents, clears the preview display, and updates button states.
        /// </summary>
        internal void Refresh()
        {
            // Clear preview.
            m_previewRenderer.RenderVehicle(null);

            m_selectedVehiclePanel.RefreshList();
            m_vehicleSelectionPanel.RefreshList();

            UpdateButtonStates();
        }

        /// <summary>
        /// Updates button states according to the current state.
        /// </summary>
        internal void UpdateButtonStates()
        {
            // Null check.
            if (m_addAllButton != null)
            {
                FastList<object> selectionList = m_vehicleSelectionPanel?.VehicleList?.Data;
                FastList<object> selectedList = m_selectedVehiclePanel?.VehicleList?.Data;

                m_addButton.isEnabled = m_selectedListVehicle != null;
                m_addAllButton.isEnabled = selectionList != null && selectionList.m_size > 0;
                m_removeButton.isEnabled = m_selectedBuildingVehicle != null;
                m_removeAllButton.isEnabled = selectedList != null && selectedList.m_size > 0;
            }
        }

        /// <summary>
        /// Adds a vehicle to the list for this transfer.
        /// </summary>
        /// <param name="vehicle">Vehicle prefab to add.</param>
        private void AddVehicle(VehicleInfo vehicle)
        {
            // Add vehicle to building.
            IBasicExtension extension = TLMLineUtils.GetEffectiveExtensionForLine(CurrentLine);
            IBasicExtensionStorage currentConfig = TLMLineUtils.GetEffectiveConfigForLine(CurrentLine);

            extension.AddAssetToLine(CurrentLine, m_currentAsset, m_capacityEditor.text, m_weightEditor.text, ProfileTarget.Weekday);
            if (currentConfig != null && currentConfig.UseSeparateWeekendProfile)
            {
                extension.AddAssetToLine(fromBuilding ? (ushort)0 : lineId, m_currentAsset, m_capacityEditor.text, m_weightEditor.text, ProfileTarget.Weekend);
            }





            // Update lists.
            Refresh();
        }

        /// <summary>
        /// Removes the currently selected vehicle from the list for this building.
        /// </summary>
        private void RemoveVehicle()
        {
            // Remove selected vehicle from building.
            VehicleControl.RemoveVehicle(CurrentBuilding, TransferReason, _selectedBuildingVehicle);

            // Update lists.
            Refresh();
        }

        /// <summary>
        /// Adds all vehicles in the available vehicle list to this building.
        /// </summary>
        private void AddAllVehicles()
        {
            // Add all vehicles in target list to bulding.
            foreach (VehicleItem item in _vehicleSelectionPanel.VehicleList.Data)
            {
                VehicleControl.AddVehicle(CurrentBuilding, TransferReason, item.Info);
            }

            // Update lists.
            Refresh();
        }

        /// <summary>
        /// Adds all vehicles in the available vehicle list to this building.
        /// </summary>
        private void RemoveAllVehicles()
        {
            // Add all vehicles in target list to bulding.
            foreach (VehicleItem item in _selectedVehiclePanel.VehicleList.Data)
            {
                VehicleControl.RemoveVehicle(CurrentBuilding, TransferReason, item.Info);
            }

            // Update lists.
            Refresh();
        }

        private void SetPreviewWindow()
        {
            MonoUtils.CreateUIElement(out m_previewPanel, this.transform);
            m_previewPanel.backgroundSprite = "GenericPanel";
            m_previewPanel.width = this.width - 15;
            m_previewPanel.height = 140;
            m_previewPanel.relativePosition = new Vector3(MidControlX, VehicleListY + ArrowSize + Margin);
            MonoUtils.CreateUIElement(out m_preview, m_previewPanel.transform);
            m_preview.size = m_previewPanel.size;
            m_preview.relativePosition = Vector3.zero;
            MonoUtils.CreateElement(out m_previewRenderer, this.transform);
            m_previewRenderer.Size = m_preview.size * 2f;
            m_preview.texture = m_previewRenderer.Texture;
            m_previewRenderer.Zoom = 3;
            m_previewRenderer.CameraRotation = 40;
        }

    }
}
