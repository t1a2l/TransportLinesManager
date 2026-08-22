using System;
using System.Text;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Utils;
using Commons.UI.Components;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector
{
    internal class LinePanel : UIPanel
    {
        /// <summary>
        /// Layout margin.
        /// </summary>
        protected const float Margin = 5f;

        // Layout constants - private.
        private const float TitleHeight = 40f;
        private const float NameLabelY = TitleHeight + Margin;
        private const float NameLabelHeight = 30f;
        private const float AreaLabelHeight = 20f;
        private const float AreaLabel1Y = TitleHeight + NameLabelHeight;
        private const float AreaLabel2Y = AreaLabel1Y + AreaLabelHeight;
        private const float ListY = AreaLabel2Y + AreaLabelHeight + Margin;
        private const float VehicleSelectionHeight = VehicleSelection.PanelHeight + Margin;
        private const float NoPanelHeight = ListY + Margin;
        private const float IconButtonSize = 40f;
        private const float IconButtonY = ListY - IconButtonSize - Margin;
        private const float PasteButtonX = PanelWidth - IconButtonSize - Margin;
        private const float CopyButtonX = PasteButtonX - IconButtonSize - Margin;
        private const float CopyBuildingButtonX = CopyButtonX - IconButtonSize - Margin;
        private const float CopyDistrictButtonX = CopyBuildingButtonX - IconButtonSize - Margin;
        private const float PanelWidth = VehicleSelection.PanelWidth + Margin + Margin;

        // Panel components.
        private UILabel m_lineLabel;
        private UIButton m_copyButton;
        private UIButton m_pasteButton;
        private UIButton m_copyLineButton;

        // Sub-panels.
        private VehicleSelection m_vehicleSelection = new();

        // Status flag.
        private bool m_panelReady = false;

        // Current selections.
        private ushort m_currentLine;
        private TransportInfo m_thisLineInfo;

        // Event handling.
        private bool m_copyProcessing = false;
        private bool m_pasteProcessing = false;

        /// <summary>
        /// Gets the current line ID.
        /// </summary>
        internal ushort CurrentLine => m_currentLine;

        /// <summary>
        /// Gets or sets a value indicating whether this is an incoming (true) or outgoing (false) transfer.
        /// </summary>
        internal bool IsIncoming { get; set; }

        /// <summary>
        /// Gets or sets the current transfer reason.
        /// </summary>
        internal TransferManager.TransferReason TransferReason { get; set; }

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
                autoLayout = false;
                backgroundSprite = "UnlockingPanel2";
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = PanelWidth;
                height = NoPanelHeight;

                // Default position - centre in screen.
                relativePosition = new Vector2(Mathf.Floor((GetUIView().fixedWidth - PanelWidth) / 2), (GetUIView().fixedHeight - NoPanelHeight) / 2);

                // Title label.
                UILabel titleLabel = UILabels.AddLabel(this, 0f, 10f, Locale.Get("MOD_NAME"), PanelWidth, 1.2f);
                titleLabel.textAlignment = UIHorizontalAlignment.Center;

                // Building label.
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
                m_copyButton = UIButtons.AddIconButton(this, CopyButtonX, IconButtonY, IconButtonSize, TextureAtlasUtils.LoadQuadSpriteAtlas("__Copy"), Locale.Get("COPY_TIP"));
                m_copyButton.eventClicked += (c, p) => CopyPaste.Instance.Copy(m_currentLine);
                m_pasteButton = UIButtons.AddIconButton(this, PasteButtonX, IconButtonY, IconButtonSize, TextureAtlasUtils.LoadQuadSpriteAtlas("__Paste"), Locale.Get("PASTE_TIP"));
                m_pasteButton.eventClicked += (c, p) => Paste();

                m_vehicleSelection = AddUIComponent<VehicleSelection>();
                m_vehicleSelection.ParentPanel = this;
                m_vehicleSelection.relativePosition = new Vector2(Margin, ListY);

                // Enable events.
                m_panelReady = true;
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("exception setting up line panel " + e.Message);
            }
        }

        /// <summary>
        /// Sets/changes the currently selected line.
        /// </summary>
        /// <param name="lineId">New line ID.</param>
        internal virtual void SetTarget(ushort lineId)
        {
            // Local references.
            TransportManager transportManager = Singleton<TransportManager>.instance;

            // Update selected line ID.
            m_currentLine = lineId;
            m_thisLineInfo = transportManager.m_lines.m_buffer[m_currentLine].Info;

            m_vehicleSelection.SetTarget(lineId, _transfers[i].Title, _transfers[i].Reason);
            m_vehicleSelection.Show();


            // Set panel height.
            height = NoPanelHeight;

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

            // Update button states.
            m_pasteButton.isEnabled = CopyPaste.Instance.IsValidTarget(CurrentLine);

            // Make sure we're visible if we're not already.
            Show();
        }

        /// <summary>
        /// Paste data action.
        /// </summary>
        private void Paste()
        {
            // Paste data.
            CopyPaste.Instance.Paste(CurrentLine);

            // Update list.
            m_vehicleSelection.Refresh();
        }
    }
}
