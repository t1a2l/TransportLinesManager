using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using Commons.UI.Components;
using Commons.Utils;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Utils;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    internal class VehicleSelectionPanel : UIPanel
    {
        /// <summary>
        /// Layout margin.
        /// </summary>
        protected const float Margin = 5f;

        // Vehicle selection list.
        private UIList m_vehicleList;

        // Search panel.
        private UITextField m_nameSearch;

        /// <summary>
        /// Gets or sets the parent reference.
        /// </summary>
        internal VehicleSelection ParentPanel { get; set; }

        /// <summary>
        /// Gets the vehicle selection list.
        /// </summary>
        internal UIList VehicleList => m_vehicleList;

        /// <summary>
        /// Sets the currently selected vehicle.
        /// </summary>
        protected virtual VehicleInfo SelectedVehicle { set => ParentPanel.SelectedListVehicle = value; }

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            try
            {
                // Basic setup.
                name = "VehicleSelectionPanel";
                autoLayout = false;
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = VehicleSelection.ListWidth;
                height = VehicleSelection.VehicleListHeight;

                // Vehicle selection list.
                m_vehicleList = UIList.AddUIList<VehicleSelectionRow>(
                    this,
                    0f,
                    0f,
                    VehicleSelection.ListWidth,
                    VehicleSelection.VehicleListHeight,
                    VehicleSelectionRow.VehicleRowHeight);
                m_vehicleList.EventSelectionChanged += (c, selectedItem) => SelectedVehicle = (selectedItem as VehicleItem)?.Info;

                // Search field.
                m_nameSearch = UITextFields.AddSmallTextField(m_vehicleList, 25f, -23f, VehicleSelection.ListWidth - 25f);
                m_nameSearch.eventTextChanged += (c, text) => PopulateList();
                UISprite searchSprite = m_nameSearch.AddUIComponent<UISprite>();
                searchSprite.atlas = TextureAtlasUtils.DefaultTextureAtlas;
                searchSprite.spriteName = "LineDetailButtonHovered";
                searchSprite.relativePosition = new Vector2(-25f, 0f);
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog(e + " exception setting up vehicle selection panel");
            }
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        internal void ClearSelection() => m_vehicleList.SelectedIndex = -1;

        /// <summary>
        /// Refreshes the list with current information.
        /// </summary>
        internal void RefreshList()
        {
            // Clear selected index.
            m_vehicleList.SelectedIndex = -1;

            // Repopulate the list.
            PopulateList();
        }

        /// <summary>
        /// Populates the list with available vehicles.
        /// </summary>
        protected virtual void PopulateList()
        {
            ushort lineId = ParentPanel.CurrentLine;

            if (lineId == 0)
            {
                VehicleList.Data = new FastList<object>
                {
                    m_buffer = [],
                    m_size = 0,
                };

                return;
            }

            var extension = TLMLineUtils.GetEffectiveExtensionForLine(lineId);

            var selectedAssets =  extension.GetAssetTransportListForLine(lineId) ?? [];

            var selectedNames =  new HashSet<string>(selectedAssets.Where(x => !string.IsNullOrEmpty(x.name)).Select(x => x.name));

            var allAssets = extension.GetAllBasicAssetsForLine(lineId);

            var items = new List<VehicleItem>();

            foreach (var entry in allAssets)
            {
                TransportAsset asset = entry.Key;
                string assetName = asset.name;

                if (string.IsNullOrEmpty(assetName) || selectedNames.Contains(assetName))
                {
                    continue;
                }

                VehicleInfo vehicle = PrefabCollection<VehicleInfo>.FindLoaded(assetName);

                if (vehicle == null)
                {
                    continue;
                }

                var item = new VehicleItem(vehicle);

                if (!NameFilter(item.Name))
                {
                    continue;
                }

                items.Add(item);
            }

            // Set display list items, without changing the display.
            m_vehicleList.Data = new FastList<object>
            {
                m_buffer = [.. items.OrderBy(x => x.Name)],
                m_size = items.Count,
            };
        }

        /// <summary>
        /// Applies the name text filter to the specified display name.
        /// </summary>
        /// <param name="displayName">Vehicle display name.</param>
        /// <returns>True if the item should be displayed (empty or matching search result), false otherwise.</returns>
        protected bool NameFilter(string displayName) => m_nameSearch.text.IsNullOrWhiteSpace() || displayName.ToLower().Contains(m_nameSearch.text.ToLower());
    }
}
