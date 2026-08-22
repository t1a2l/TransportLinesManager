using ColossalFramework.UI;
using Commons.Utils;
using Commons.UI.Components;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    /// <summary>
    /// UIList row item for vehicle prefabs.
    /// </summary>
    public class VehicleSelectionRow : UIListRow
    {
        /// <summary>
        /// Row height.
        /// </summary>
        public const float VehicleRowHeight = 40f;

        // Layout constants - private.
        private const float VehicleSpriteSize = 40f;
        private const float SteamSpriteWidth = 26f;
        private const float SteamSpriteHeight = 16f;
        private const float ScrollMargin = 10f;

        // Vehicle name label.
        private UILabel m_vehicleNameLabel;

        // Preview image.
        private UISprite m_vehicleSprite;

        // Steam icon.
        private UISprite m_steamSprite;

        /// <summary>
        /// Vehicle prefab.
        /// </summary>
        private VehicleInfo m_info;

        /// <summary>
        /// Gets the height for this row.
        /// </summary>
        public override float RowHeight => VehicleRowHeight;

        /// <summary>
        /// Generates and displays a row.
        /// </summary>
        /// <param name="data">Object data to display.</param>
        /// <param name="rowIndex">Row index number (for background banding).</param>
        public override void Display(object data, int rowIndex)
        {
            // Perform initial setup for new rows.
            if (m_vehicleNameLabel == null)
            {
                // Add object name label.
                m_vehicleNameLabel = AddLabel(VehicleSpriteSize + Margin, width - Margin - VehicleSpriteSize - Margin - SteamSpriteWidth - ScrollMargin - Margin, wordWrap: true);

                // Add preview sprite image.
                m_vehicleSprite = AddUIComponent<UISprite>();
                m_vehicleSprite.height = VehicleSpriteSize;
                m_vehicleSprite.width = VehicleSpriteSize;
                m_vehicleSprite.relativePosition = Vector2.zero;

                // Add setam sprite.
                m_steamSprite = AddUIComponent<UISprite>();
                m_steamSprite.width = SteamSpriteWidth;
                m_steamSprite.height = SteamSpriteHeight;
                m_steamSprite.atlas = TextureAtlasUtils.DefaultTextureAtlas;
                m_steamSprite.spriteName = "SteamWorkshop";
                m_steamSprite.relativePosition = new Vector2(width - Margin - ScrollMargin - SteamSpriteWidth, (height - SteamSpriteHeight) / 2f);
            }

            // Get building ID and set name label.
            if (data is VehicleItem thisItem)
            {
                m_info = thisItem.Info;
                m_vehicleNameLabel.text = thisItem.Name;

                m_vehicleSprite.atlas = m_info?.m_Atlas;
                m_vehicleSprite.spriteName = m_info?.m_Thumbnail;

                m_steamSprite.isVisible = PrefabUtils.IsWorkshopAsset(m_info);
            }
            else
            {
                // Just in case (no valid vehicle record).
                m_vehicleNameLabel.text = string.Empty;
            }

            // Set initial background as deselected state.
            Deselect(rowIndex);
        }
    }
}
