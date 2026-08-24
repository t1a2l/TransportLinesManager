using System.Collections.Generic;
using System.Linq;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.UI.Components;
using Commons.Utils;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Utils;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    internal class SelectedVehiclePanel : VehicleSelectionPanel
    {
        // Panel to display when no item is selected.
        private UIPanel m_randomPanel;
        private UILabel m_randomLabel;
        private UISprite m_randomSprite;

        /// <summary>
        /// Gets or sets a value indicating whether the Transport Lines Manager mod is active.
        /// </summary>
        internal static bool TLMActive { get; set; } = false;

        /// <summary>
        /// Sets the currently selected vehicle.
        /// </summary>
        protected override VehicleInfo SelectedVehicle { set => ParentPanel.SelectedLineVehicle = value; }

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Panel setup.
            m_randomPanel = VehicleList.AddUIComponent<UIPanel>();
            m_randomPanel.width = VehicleList.width;
            m_randomPanel.height = VehicleList.height;
            m_randomPanel.relativePosition = new Vector2(0f, 0f);

            // Random sprite.
            m_randomSprite = m_randomPanel.AddUIComponent<UISprite>();
            m_randomSprite.atlas = TextureAtlasUtils.DefaultTextureAtlas;
            m_randomSprite.spriteName = "Random";
            // __RemoveIcon

            // Label.
            m_randomLabel = UILabels.AddLabel(m_randomPanel, 0f, 0f, Locale.Get("TLM_ANY_VEHICLE"), VehicleList.width, 0.8f);

            // Size is 56x33, so offset -8 from left and 3.5 from top to match normal row sizing.
            m_randomSprite.size = new Vector2(56f, 33f);
            m_randomSprite.relativePosition = new Vector2(-8, (40f - m_randomSprite.height) / 2f);
            m_randomLabel.relativePosition = new Vector2(48f, 12f);
        }

        /// <summary>
        /// Populates the list.
        /// </summary>
        protected override void PopulateList()
        {
            var items = new List<VehicleItem>();

            ushort lineId = ParentPanel.CurrentLine;

            var allowAutoSpawnAllVehicles = TLMBaseConfigXML.CurrentContextConfig.AllowAutoSpawnAllVehicles;

            if (lineId == 0)
            {
                m_randomPanel.Show();
                m_randomLabel.text = allowAutoSpawnAllVehicles? Locale.Get("TLM_ANY_COMPATIBLE_VEHICLE") : Locale.Get("TLM_NO_SELECTED_VEHICLES");
                m_randomSprite.name = allowAutoSpawnAllVehicles ? "Random" : "__RemoveIcon";

                VehicleList.Data = new FastList<object>
                {
                    m_buffer = [.. items],
                    m_size = items.Count,
                };

                return;
            }

            var selectedAssets = GetSelectedAssets();

            // Any selected vehicles?
            if (selectedAssets != null && selectedAssets.Count > 0)
            {
                // Yes - hide random panel.
                m_randomPanel.Hide();

                // Generate filtered display list.
                foreach (var asset in selectedAssets)
                {
                    VehicleInfo vehicle = PrefabCollection<VehicleInfo>.FindLoaded(asset.name);

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
            }
            else
            {
                // No selected vehicles available - show random item panel.
                m_randomPanel.Show();
                m_randomLabel.text = allowAutoSpawnAllVehicles ? Locale.Get("TLM_ANY_COMPATIBLE_VEHICLE") : Locale.Get("TLM_NO_SELECTED_VEHICLES");
            }

            // Set display list items, without changing the display.
            VehicleList.Data = new FastList<object>
            {
                m_buffer = [.. items.OrderBy(x => x.Name)],
                m_size = items.Count,
            };
        }

        private List<TransportAsset> GetSelectedAssets()
        {
            if (ParentPanel.FromBuilding)
            {
                return ParentPanel.TransportSystem.GetTransportExtension().GetAssetTransportListForLine(0) ?? [];
            }

            return TLMLineUtils.GetEffectiveExtensionForLine(ParentPanel.CurrentLine).GetAssetTransportListForLine(ParentPanel.CurrentLine) ?? [];
        }
    }
}
