using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Utils;
using Commons.UI.Components;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    internal class SelectedVehiclePanel : VehicleSelectionPanel
    {
        // Panel to display when no item is selected.
        private UIPanel _randomPanel;
        private UILabel _randomLabel;

        /// <summary>
        /// Gets or sets a value indicating whether the Transport Lines Manager mod is active.
        /// </summary>
        internal static bool TLMActive { get; set; } = false;

        /// <summary>
        /// Sets the currently selected vehicle.
        /// </summary>
        protected override VehicleInfo SelectedVehicle { set => ParentPanel.SelectedBuildingVehicle = value; }

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Panel setup.
            _randomPanel = VehicleList.AddUIComponent<UIPanel>();
            _randomPanel.width = VehicleList.width;
            _randomPanel.height = VehicleList.height;
            _randomPanel.relativePosition = new Vector2(0f, 0f);

            // Random sprite.
            UISprite randomSprite = _randomPanel.AddUIComponent<UISprite>();
            randomSprite.atlas = TextureAtlasUtils.DefaultTextureAtlas;
            randomSprite.spriteName = "Random";

            // Label.
            _randomLabel = UILabels.AddLabel(_randomPanel, 0f, 0f, Locale.Get("ANY_VEHICLE"), VehicleList.width, 0.8f);

            // Size is 56x33, so offset -8 from left and 3.5 from top to match normal row sizing.
            randomSprite.size = new Vector2(56f, 33f);
            randomSprite.relativePosition = new Vector2(-8, (40f - randomSprite.height) / 2f);
            _randomLabel.relativePosition = new Vector2(48f, (randomSprite.height - _randomLabel.height) / 2f);
        }

        /// <summary>
        /// Populates the list.
        /// </summary>
        protected override void PopulateList()
        {
            List<VehicleItem> items = [];
            List<VehicleInfo> buildingVehicles = VehicleControl.GetVehicles(ParentPanel.CurrentBuilding, ParentPanel.TransferReason);

            // Any selected vehicles?
            if (buildingVehicles != null && buildingVehicles.Count > 0)
            {
                // Yes - hide random panel.
                _randomPanel.Hide();

                // Generate filtered display list.
                foreach (VehicleInfo vehicle in buildingVehicles)
                {
                    // Generate vehicle record for name filtering.
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
            else
            {
                // No selected vehicles available - show random item panel.
                _randomPanel.Show();

                // Check for TLM override.
                _randomLabel.text = Locale.Get(TLMActive && Singleton<BuildingManager>.instance.m_buildings.m_buffer[ParentPanel.ParentPanel.CurrentBuilding].Info.m_buildingAI is TransportStationAI ? "TLM_VEHICLE" : "ANY_VEHICLE");
            }

            // Set display list items, without changing the display.
            VehicleList.Data = new FastList<object>
            {
                m_buffer = items.OrderBy(x => x.Name).ToArray(),
                m_size = items.Count,
            };
        }
    }
}
