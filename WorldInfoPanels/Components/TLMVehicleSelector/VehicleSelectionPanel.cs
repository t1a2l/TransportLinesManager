using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using Commons.Utils;
using Commons.UI.Components;
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
        private UIList _vehicleList;

        // Search panel.
        private UITextField _nameSearch;

        /// <summary>
        /// Gets or sets the parent reference.
        /// </summary>
        internal VehicleSelection ParentPanel { get; set; }

        /// <summary>
        /// Gets the vehicle selection list.
        /// </summary>
        internal UIList VehicleList => _vehicleList;

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
                _vehicleList = UIList.AddUIList<VehicleSelectionRow>(
                    this,
                    0f,
                    0f,
                    VehicleSelection.ListWidth,
                    VehicleSelection.VehicleListHeight,
                    VehicleSelectionRow.VehicleRowHeight);
                _vehicleList.EventSelectionChanged += (c, selectedItem) => SelectedVehicle = (selectedItem as VehicleItem)?.Info;

                // Search field.
                _nameSearch = UITextFields.AddSmallTextField(_vehicleList, 25f, -23f, VehicleSelection.ListWidth - 25f);
                _nameSearch.eventTextChanged += (c, text) => PopulateList();
                UISprite searchSprite = _nameSearch.AddUIComponent<UISprite>();
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
        internal void ClearSelection() => _vehicleList.SelectedIndex = -1;

        /// <summary>
        /// Refreshes the list with current information.
        /// </summary>
        internal void RefreshList()
        {
            // Clear selected index.
            _vehicleList.SelectedIndex = -1;

            // Repopulate the list.
            PopulateList();
        }

        /// <summary>
        /// Populates the list with available vehicles.
        /// </summary>
        protected virtual void PopulateList()
        {
            // Ensure valid building selection.
            ushort currentBuilding = ParentPanel.CurrentBuilding;
            if (currentBuilding == 0)
            {
                return;
            }

            // Local reference.
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            // Generated lists.
            List<VehicleItem> items = [];
            List<VehicleInfo> trailers = [];
            List<VehicleInfo> locomotives = [];

            // Determine effective building class for vehicle matching.
            VehicleControl.GetEffectiveClass(currentBuilding, buildingBuffer, ParentPanel.TransferReason, out ItemClass.Service buildingService, out ItemClass.SubService buildingSubService, out ItemClass.Level buildingLevel);

            // Get list of already-selected vehicles.
            List<VehicleInfo> selectedList = VehicleControl.GetVehicles(currentBuilding, ParentPanel.TransferReason);

            // Iterate through all loaded vehicles.
            for (uint i = 0; i < PrefabCollection<VehicleInfo>.LoadedCount(); ++i)
            {
                if (PrefabCollection<VehicleInfo>.GetLoaded(i) is VehicleInfo vehicle)
                {
                    // Looking for service, sub-service and level match.
                    // Level match is ignored if the service is PlayerIndustry, to let general Industries DLC cargo vehicles (level 0) also transfer luxury products (level 1),
                    // Level match is also ignored for zoned industry due to builing level-up.
                    // Ignore any trailer vehicles.
                    // Ignore any procedural vehicles (e.g. fire helicopter buckets).
                    if (vehicle.m_class.m_service == buildingService &&
                        vehicle.m_class.m_subService == buildingSubService &&
                        (vehicle.m_class.m_level == buildingLevel || buildingService == ItemClass.Service.Industrial || buildingService == ItemClass.Service.PlayerIndustry) &&
                        !(vehicle.m_vehicleAI is CarTrailerAI) &&
                        !(vehicle.m_placementStyle == ItemClass.Placement.Procedural) &&
                        (selectedList == null || !selectedList.Contains(vehicle)))
                    {
                        // Special check for fishing boats, to stop fishing boats being included in fish truck lists (and vice-versa).
                        if (buildingService == ItemClass.Service.Fishing)
                        {
                            bool isFishingBoat = vehicle.m_vehicleAI is FishingBoatAI;
                            bool isFishingBoatService = ParentPanel.TransferReason == TransferManager.TransferReason.None;
                            if (isFishingBoat != isFishingBoatService)
                            {
                                continue;
                            }
                        }

                        // Check vehicle type, if applicable.
                        if (buildingBuffer[currentBuilding].Info.m_buildingAI is PlayerBuildingAI playerBuildingAI)
                        {
                            VehicleInfo.VehicleType vehicleType = playerBuildingAI.GetVehicleType();

                            // Additional check for passenger planes.
                            if (buildingSubService == ItemClass.SubService.PublicTransportPlane && playerBuildingAI.m_info.m_class.m_level != ItemClass.Level.Level5)
                            {
                                vehicleType = VehicleInfo.VehicleType.Plane;
                            }

                            // Check to separate disaster helicopters and trucks.
                            else if (ParentPanel.TransferReason == TransferManager.TransferReason.Collapsed)
                            {
                                vehicleType = VehicleInfo.VehicleType.Car;
                            }
                            else if (ParentPanel.TransferReason == TransferManager.TransferReason.Collapsed2)
                            {
                                vehicleType = VehicleInfo.VehicleType.Helicopter;
                            }

                            // Check to include fishing boats.
                            else if (buildingService == ItemClass.Service.Fishing && ParentPanel.TransferReason == TransferManager.TransferReason.None)
                            {
                                vehicleType = VehicleInfo.VehicleType.Ship;
                            }

                            // Check vehicle type.
                            if (vehicleType != VehicleInfo.VehicleType.None && vehicleType != vehicle.m_vehicleType)
                            {
                                continue;
                            }
                        }

                        // Record any trailers.
                        if (vehicle.m_trailers != null && vehicle.m_trailers.Length > 0)
                        {
                            // Any vehicle that has trailers is a locomotive - record it to ensure that we don't accidentally remove it later (trailing driving vehicles).
                            locomotives.Add(vehicle);

                            // Record all trailers.
                            foreach (VehicleInfo.VehicleTrailer trailer in vehicle.m_trailers)
                            {
                                trailers.Add(trailer.m_info);
                            }
                        }

                        // All filters passed so far - generate vehicle record for name filtering.
                        VehicleItem thisItem = new VehicleItem(vehicle);

                        // Apply name filter.
                        if (!NameFilter(thisItem.Name))
                        {
                            continue;
                        }

                        // Name filter passed - add to available list.
                        items.Add(thisItem);
                    }
                }
            }

            // Remove any trailers from list.
            foreach (VehicleInfo trailer in trailers)
            {
                // Exempt any recorded locomotives from removal.
                items.Remove(items.Find(x => x.Info == trailer && !locomotives.Contains(trailer)));
            }

            // Set display list items, without changing the display.
            _vehicleList.Data = new FastList<object>
            {
                m_buffer = items.OrderBy(x => x.Name).ToArray(),
                m_size = items.Count,
            };
        }

        /// <summary>
        /// Applies the name text filter to the specified display name.
        /// </summary>
        /// <param name="displayName">Vehicle display name.</param>
        /// <returns>True if the item should be displayed (empty or matching search result), false otherwise.</returns>
        protected bool NameFilter(string displayName) => _nameSearch.text.IsNullOrWhiteSpace() || displayName.ToLower().Contains(_nameSearch.text.ToLower());
    }
}
