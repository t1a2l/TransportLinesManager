using System;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.UI.Components;
using Commons.Utils;
using TransportLinesManager.Data.Base.ConfigurationContainers;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Tabs;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    internal class LinePanel : UIPanel
    {
        protected const float Margin = 5f;

        private const float TitleHeight = 40f;
        private const float NameLabelY = TitleHeight + Margin + 10f;
        private const float NameLabelHeight = 30f;
        private const float AreaLabelHeight = 20f;
        private const float AreaLabel1Y = TitleHeight + NameLabelHeight;
        private const float AreaLabel2Y = AreaLabel1Y + AreaLabelHeight;
        private const float ListY = AreaLabel2Y + AreaLabelHeight + Margin;
        private const float NoPanelHeight = ListY + Margin;
        private const float IconButtonSize = 40f;
        private const float IconButtonY = ListY - IconButtonSize - Margin;
        private const float PasteButtonX = PanelWidth - IconButtonSize - Margin;
        private const float CopyButtonX = PasteButtonX - IconButtonSize - Margin;

        private const float PanelWidth = VehicleSelection.PanelWidth + Margin + Margin;

        private UILabel m_titleLabel;
        private UILabel m_lineLabel;
        private UIButton m_copyButton;
        private UIButton m_pasteButton;

        private VehicleSelection m_vehicleSelection = new();

        private ushort m_currentLine;

        public override void Awake()
        {
            base.Awake();

            try
            {
                // Basic setup.
                autoLayout = false;
                backgroundSprite = "UnlockingPanel2";
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = PanelWidth;
                height = 400;

                // Default position - centre in screen.
                relativePosition = new Vector2(Mathf.Floor((GetUIView().fixedWidth - PanelWidth) / 2), (GetUIView().fixedHeight - NoPanelHeight) / 2);

                // Title label.
                m_titleLabel = UILabels.AddLabel(this, 0f, 10f, "temp", PanelWidth, 1.2f);
                m_titleLabel.textAlignment = UIHorizontalAlignment.Center;

                // Line label.
                m_lineLabel = UILabels.AddLabel(this, 0f, NameLabelY, string.Empty, PanelWidth);
                m_lineLabel.textAlignment = UIHorizontalAlignment.Center;

                // Drag handle.
                UIDragHandle dragHandle = this.AddUIComponent<UIDragHandle>();
                dragHandle.relativePosition = Vector3.zero;
                dragHandle.width = PanelWidth - 35f;
                dragHandle.height = TitleHeight;

                // Close button.
                UIButton closeButton = AddUIComponent<UIButton>();
                closeButton.relativePosition = new Vector2(width - 35f, 2f);
                closeButton.normalBgSprite = "buttonclose";
                closeButton.hoveredBgSprite = "buttonclosehover";
                closeButton.pressedBgSprite = "buttonclosepressed";

                // Close button event handler.
                closeButton.eventClick += (component, clickEvent) =>
                {
                    LinePanelManager.Close();
                };

                // Copy/paste buttons.
                m_copyButton = UIButtons.AddIconButton(this, CopyButtonX, IconButtonY, IconButtonSize, TextureAtlasUtils.LoadQuadSpriteAtlas("__Copy"), Locale.Get("TLM_COPY_CURRENT_LIST_CLIPBOARD"));
                m_copyButton.eventClicked += (c, p) =>
                {
                    TLMAssetSelectorTab.CopyAssetConfiguration(m_currentLine);
                    m_pasteButton.isEnabled = TLMAssetSelectorTab.HasAssetClipboard;
                };
                m_pasteButton = UIButtons.AddIconButton(this, PasteButtonX, IconButtonY, IconButtonSize, TextureAtlasUtils.LoadQuadSpriteAtlas("__Paste"), Locale.Get("TLM_PASTE_CLIPBOARD_TO_CURRENT_LIST"));
                m_pasteButton.eventClicked += (c, p) => Paste();

                m_vehicleSelection = AddUIComponent<VehicleSelection>();
                m_vehicleSelection.ParentPanel = this;
                m_vehicleSelection.relativePosition = new Vector2(Margin, ListY);
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("exception setting up line panel " + e.Message);
            }
        }

        internal virtual void SetTarget(ushort lineId)
        {
            // Update selected line ID.
            m_currentLine = lineId;

            m_vehicleSelection.SetTarget(lineId);
            m_vehicleSelection.Show();

            // Set panel height.
            height = 400;

            // Set name.
            m_lineLabel.text = Singleton<TransportManager>.instance.GetLineName(lineId);

            // Make sure we're fully visible on-screen.
            if (absolutePosition.y + height > Screen.height - 120)
            {
                absolutePosition = new Vector2(absolutePosition.x, Screen.height - 120 - height);
            }

            if (absolutePosition.x + width > Screen.width - 20)
            {
                absolutePosition = new Vector2(Screen.width - 20 - width, absolutePosition.y);
            }

            if (absolutePosition.y < 20f)
            {
                absolutePosition = new Vector2(absolutePosition.x, 20f);
            }

            if (absolutePosition.x < 20f)
            {
                absolutePosition = new Vector2(20f, absolutePosition.y);
            }

            var tsd = UVMPublicTransportWorldInfoPanel.GetCurrentTSD();
            var config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, tsd);

            if (lineId == 0)
            {
                m_titleLabel.text = Locale.Get("TLM_ASSET_SELECT_WINDOW_TITLE_OUTSIDECONNECTION");
            }
            else if (config is TLMTransportLineConfiguration)
            {
                m_titleLabel.text = string.Format(Locale.Get("TLM_ASSET_SELECT_WINDOW_TITLE"), TLMLineUtils.GetLineStringId(lineId, false));
            }
            else
            {
                int prefix = (int)TLMPrefixesUtils.GetPrefix(lineId);
                m_titleLabel.text = string.Format(Locale.Get("TLM_ASSET_SELECT_WINDOW_TITLE_PREFIX"), prefix > 0 ? NumberingUtils.GetStringFromNumber(TLMPrefixesUtils.GetStringOptionsForPrefix(tsd), prefix + 1) : Locale.Get("TLM_UNPREFIXED"), tsd.GetTransportName());
            }

            // Update button states.
            m_pasteButton.isEnabled = TLMAssetSelectorTab.HasAssetClipboard;

            // Make sure we're visible if we're not already.
            Show();
        }

        private void Paste()
        {
            if (!TLMAssetSelectorTab.PasteAssetConfiguration(m_currentLine))
            {
                return;
            }

            m_vehicleSelection.Refresh();
            m_pasteButton.isEnabled = TLMAssetSelectorTab.HasAssetClipboard;
        }
    }
}
