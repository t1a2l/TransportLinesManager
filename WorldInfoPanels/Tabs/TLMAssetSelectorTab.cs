using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Extensions.UI;
using Commons.UI;
using Commons.UI.SpriteNames;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base;
using TransportLinesManager.Data.Base.ConfigurationContainers;
using TransportLinesManager.Data.DataContainers;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Interfaces;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Components;
using TransportLinesManager.WorldInfoPanels.Components.TLMVehicleSelector;
using UnityEngine;
using static TransportLinesManager.Data.Extensions.ExtensionStaticExtensionMethods;

namespace TransportLinesManager.WorldInfoPanels.Tabs
{
    internal class TLMAssetSelectorTab : UICustomControl, IUVMPTWIPChild
    {
        private UILabel m_title;
        private Color m_lastColor = Color.clear;
        private static bool m_isDirty = false;
        private bool m_pendingAssetUsageRefresh;
        private ushort m_pendingAssetUsageLineId;
        private int m_pendingAssetUsageSlotIndex = -1;

        internal static TLMAssetSelectorTab Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
            CreateWindow();
            TLMLineUtils.EventAssetUsedCountChanged += OnAssetUsedCountChanged;
        }

        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            TLMLineUtils.EventAssetUsedCountChanged -= OnAssetUsedCountChanged;
        }

        private void OnAssetUsedCountChanged(ushort lineId, int slotIndex)
        {
            m_pendingAssetUsageLineId = lineId;
            m_pendingAssetUsageSlotIndex = slotIndex;
            m_pendingAssetUsageRefresh = true;
        }

        public void Update()
        {
            if (!m_pendingAssetUsageRefresh)
            {
                return;
            }

            m_pendingAssetUsageRefresh = false;

            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort currentLine, out bool fromBuilding);
            if (fromBuilding || currentLine == 0 || currentLine != m_pendingAssetUsageLineId)
            {
                return;
            }

            RefreshAssetRows(m_pendingAssetUsageSlotIndex);
        }

        public UIPanel MainPanel { get; private set; }

        private UIScrollablePanel m_scrollablePanel;
        private AVOPreviewRenderer m_previewRenderer;
        private UITextureSprite m_preview;
        private UIPanel m_previewPanel;
        private VehicleInfo m_lastInfo;
        private UITemplateList<UIPanel> m_checkboxTemplateList;
        private UITextField m_nameFilter;
        private readonly Dictionary<TransportSystemDefinition, string> m_clipboard = [];

        private UIButton m_copyButton;
        private UIButton m_pasteButton;
        private UIButton m_eraseButton;

        private UIButton m_vehicleManagementButton;

        private UILabel m_capacityColumnHeader;
        private UILabel m_weightColumnHeader;
        private UILabel m_usedCountColumnHeader;

        private UILabel m_vehicleCountLabel;

        private UISprite m_timeBudgetSelectLabelSprite;
        private static UIDropDown m_timeBudgetSelect;

        private UIPanel m_budgetProfilePanel;
        private UILabel m_budgetProfileLabel;
        private UIDropDown m_budgetProfileDropdown;

        private TransportSystemDefinition TransportSystem => UVMPublicTransportWorldInfoPanel.GetCurrentTSD();

        private static readonly List<BudgetEntryXml> m_budgetEntriesInUiOrder = [];

        public ProfileTarget CurrentProfileTarget => m_budgetProfileDropdown?.selectedIndex == 1 ? ProfileTarget.Weekend : ProfileTarget.Weekday;

        internal static bool HasAssetClipboard => Instance != null && Instance.m_clipboard.ContainsKey(Instance.TransportSystem);

        public static ushort GetLineID()
        {
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
            return !fromBuilding ? lineId : (ushort)0;
        }

        public static void RefreshIndicatorForBudgetIndex(ushort lineId, int budgetIndex)
        {
            Instance?.UpdateModeIndicator(lineId, budgetIndex);
        }

        public static void MarkDirty() => m_isDirty = true;

        public void OnEnable()
        { }

        public void OnDisable()
        {
            OnDestroy();
        }

        public void OnGotFocus()
        { }

        public bool MayBeVisible() => TransportSystem?.HasVehicles() ?? false;

        public void Hide() => MainPanel.isVisible = false;

        public void RefreshSelectedAssetRows()
        {
            var config = TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem);
            UpdateAssetList(config);
        }

        public static int GetBudgetSelectedIndex()
        {
            if (!UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) || fromBuilding || lineId == 0)
            {
                return 0;
            }

            int uiIndex = m_timeBudgetSelect.selectedIndex;
            if (uiIndex < 0 || uiIndex >= m_budgetEntriesInUiOrder.Count)
            {
                return 0;
            }

            return GetBudgetEntryBackingIndex(lineId, m_budgetEntriesInUiOrder[uiIndex]);
        }

        public static int GetBudgetEntryBackingIndex(ushort lineId, BudgetEntryXml target)
        {
            var budgetEntries = TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetBudgetsMultiplierForLine(lineId, Instance.CurrentProfileTarget);
            for (int i = 0; i < budgetEntries.Count; i++)
            {
                if (ReferenceEquals(budgetEntries[i], target))
                {
                    return i;
                }
            }
            return -1;
        }

        internal void UpdateWeekendBudgetUIState()
        {
            bool enabled = false;

            if (TryGetCurrentLineConfig(out ushort lineId, out IBudgetStorage cfg))
            {
                enabled = cfg?.UseSeparateWeekendProfile == true;
            }

            m_budgetProfilePanel?.isVisible = enabled;
            m_budgetProfileLabel?.isVisible = enabled;
            m_budgetProfileDropdown?.isVisible = enabled;

            m_budgetProfileLabel?.relativePosition = new Vector3(5f, 0f);
            m_budgetProfileDropdown?.relativePosition = new Vector3(95f, 0f);

            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, TransportSystem);
            UpdateAssetList(config);
        }

        internal static void CopyAssetConfiguration(ushort sourceLineId)
        {
            if (Instance == null || sourceLineId == 0)
            {
                return;
            }

            var sourceTsd = TransportSystemDefinition.FromLineId(sourceLineId, false);

            var config = TLMLineUtils.GetEffectiveExtensionForLine(sourceLineId, sourceTsd);

            var assets = config.GetAssetTransportListForLine(sourceLineId);
          
            Instance.m_clipboard[sourceTsd] = XmlUtils.DefaultXmlSerialize(assets);
        }

        internal static bool PasteAssetConfiguration(ushort targetLineId)
        {
            if (Instance == null || targetLineId == 0)
            {
                return false;
            }

            var targetTsd = TransportSystemDefinition.FromLineId(targetLineId, false);

            if (!Instance.m_clipboard.TryGetValue(targetTsd, out string data))
            {
                return false;
            }

            var copiedAssets = XmlUtils.DefaultXmlDeserialize<SimpleXmlList<TransportAsset>>(data)?.ToList();

            if (copiedAssets == null)
            {
                return false;
            }

            var targetConfig = TLMLineUtils.GetEffectiveExtensionForLine(targetLineId, targetTsd);

            targetConfig.SetAssetTransportListForLine(targetLineId, copiedAssets);

            MarkDirty();

            return true;
        }

        private void CreateWindow()
        {
            CreateMainPanel();

            MonoUtils.CreateUIElement(out m_nameFilter, MainPanel.transform);
            MonoUtils.UiTextFieldDefaults(m_nameFilter);
            MonoUtils.InitButtonFull(m_nameFilter, false, "OptionsDropboxListbox");
            m_nameFilter.tooltip = Locale.Get("TLM_ASSET_FILTERBY");
            m_nameFilter.relativePosition = new Vector3(5f, 75f);
            m_nameFilter.height = 23;
            m_nameFilter.width = 170f;
            m_nameFilter.eventKeyUp += (x, y) => UpdateAssetList(TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem));
            m_nameFilter.eventTextSubmitted += (x, y) => UpdateAssetList(TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem));
            m_nameFilter.eventTextCancelled += (x, y) => UpdateAssetList(TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem));
            m_nameFilter.horizontalAlignment = UIHorizontalAlignment.Left;
            m_nameFilter.padding = new RectOffset(2, 2, 4, 2);

            MonoUtils.CreateUIElement(out m_timeBudgetSelectLabelSprite, MainPanel.transform);
            m_timeBudgetSelectLabelSprite.spriteName = "InfoPanelIconCurrency";
            m_timeBudgetSelectLabelSprite.size = new Vector2(35, 35);
            m_timeBudgetSelectLabelSprite.relativePosition = new Vector3(MainPanel.width - 140f, 50f);
            m_timeBudgetSelectLabelSprite.tooltipLocaleID = "TLM_START_HOUR";

            m_timeBudgetSelect = UIHelperExtension.CloneBasicDropDownNoLabel([], ChangeBudgetTime, MainPanel);
            m_timeBudgetSelect.tooltipLocaleID = "TLM_TIME_PERCENT_LABEL";
            m_timeBudgetSelect.relativePosition = new Vector3(MainPanel.width - 100f, 55f);
            m_timeBudgetSelect.height = 24f;
            m_timeBudgetSelect.width = 80f;
            m_timeBudgetSelect.horizontalAlignment = UIHorizontalAlignment.Left;
            m_timeBudgetSelect.listPosition = UIDropDown.PopupListPosition.Automatic;
            m_timeBudgetSelect.textScale = 0.90f;
            m_timeBudgetSelect.eventSelectedIndexChanged += TimeBudgetSelect_eventSelectedIndexChanged;

            MonoUtils.CreateUIElement(out m_budgetProfilePanel, MainPanel.transform);
            m_budgetProfilePanel.name = "BudgetProfilePanel";
            m_budgetProfilePanel.width = 140f;
            m_budgetProfilePanel.height = 24f;
            m_budgetProfilePanel.autoLayout = false;
            m_budgetProfilePanel.relativePosition = new Vector3(5f, 46f);

            MonoUtils.CreateUIElement(out m_budgetProfileLabel, m_budgetProfilePanel.transform);
            m_budgetProfileLabel.name = "BudgetProfileLabel";
            m_budgetProfileLabel.text = Locale.Get("TLM_PROFILE");
            m_budgetProfileLabel.textScale = 0.9f;
            m_budgetProfileLabel.autoSize = false;
            m_budgetProfileLabel.width = 120f;
            m_budgetProfileLabel.height = 24f;
            m_budgetProfileLabel.relativePosition = new Vector3(5f, 0f);
            m_budgetProfileLabel.verticalAlignment = UIVerticalAlignment.Middle;

            var ddGo = Instantiate(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel>().Find<UIDropDown>("Dropdown").gameObject, m_budgetProfilePanel.transform);

            m_budgetProfileDropdown = ddGo.GetComponent<UIDropDown>();
            m_budgetProfileDropdown.name = "BudgetProfileDropdown";
            m_budgetProfileDropdown.width = 140f;
            m_budgetProfileDropdown.height = 24f;
            m_budgetProfileDropdown.textScale = 0.85f;
            m_budgetProfileDropdown.itemHeight = 24;
            m_budgetProfileDropdown.textFieldPadding = new RectOffset(6, 6, 4, 4);
            m_budgetProfileDropdown.itemPadding = new RectOffset(6, 6, 2, 2);
            m_budgetProfileDropdown.normalBgSprite = "OptionsDropboxListbox";
            m_budgetProfileDropdown.items =
            [
                Locale.Get("TLM_PROFILE_WEEKDAY"),
                Locale.Get("TLM_PROFILE_WEEKEND")
            ];
            m_budgetProfileDropdown.selectedIndex = 0;
            m_budgetProfileDropdown.relativePosition = new Vector3(95f, 0f);
            m_budgetProfileDropdown.eventSelectedIndexChanged += OnBudgetProfileChanged;

            CreateScrollPanel();

            SetPreviewWindow();

            CreateButtons();

            CreateTemplateList();

            MonoUtils.CreateUIElement(out m_vehicleCountLabel, MainPanel.transform);
            m_vehicleCountLabel.autoSize = false;
            m_vehicleCountLabel.width = 50f;
            m_vehicleCountLabel.height = 20f;
            m_vehicleCountLabel.relativePosition = new Vector3(MainPanel.width - 210f, 85f);
            m_vehicleCountLabel.textScale = 0.65f;
            m_vehicleCountLabel.textAlignment = UIHorizontalAlignment.Center;
            m_vehicleCountLabel.isVisible = false;

            MonoUtils.CreateUIElement(out m_capacityColumnHeader, MainPanel.transform);
            m_capacityColumnHeader.autoSize = false;
            m_capacityColumnHeader.width = 50f;
            m_capacityColumnHeader.height = 20f;
            m_capacityColumnHeader.relativePosition = new Vector3(MainPanel.width - 170f, 85f);
            m_capacityColumnHeader.textScale = 0.65f;
            m_capacityColumnHeader.textAlignment = UIHorizontalAlignment.Center;
            m_capacityColumnHeader.localeID = "TLM_ASSET_CAPACITY_FIELD_HEADER";

            MonoUtils.CreateUIElement(out m_weightColumnHeader, MainPanel.transform);
            m_weightColumnHeader.autoSize = false;
            m_weightColumnHeader.width = 50f;
            m_weightColumnHeader.height = 20f;
            m_weightColumnHeader.relativePosition = new Vector3(MainPanel.width - 120f, 85f);
            m_weightColumnHeader.textScale = 0.65f;
            m_weightColumnHeader.textAlignment = UIHorizontalAlignment.Center;
            // text set dynamically in UpdateModeIndicator()

            MonoUtils.CreateUIElement(out m_usedCountColumnHeader, MainPanel.transform);
            m_usedCountColumnHeader.autoSize = false;
            m_usedCountColumnHeader.width = 50f;
            m_usedCountColumnHeader.height = 20f;
            m_usedCountColumnHeader.relativePosition = new Vector3(MainPanel.width - 70f, 85f);
            m_usedCountColumnHeader.textScale = 0.65f;
            m_usedCountColumnHeader.textAlignment = UIHorizontalAlignment.Center;
        }

        private void TimeBudgetSelect_eventSelectedIndexChanged(UIComponent component, int value)
        {
            ChangeBudgetTime(value);
        }

        private void OnBudgetProfileChanged(UIComponent component, int value)
        {
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem);
            UpdateAssetList(config);
        }

        private void CreateTemplateList()
        {
            TLMAssetItemLine.EnsureTemplate();
            m_checkboxTemplateList = new UITemplateList<UIPanel>(m_scrollablePanel, TLMAssetItemLine.TEMPLATE_NAME);
        }

        private void CreateButtons()
        {
            var removeUndesired = ConfigureActionButton(MainPanel, CommonsSpriteNames.RemoveUnwantedIcon);
            removeUndesired.eventClick += (component, eventParam) => TLMVehicleUtils.RemoveAllUnwantedVehicles();

            m_copyButton = ConfigureActionButton(MainPanel, CommonsSpriteNames.Copy);
            m_copyButton.eventClick += (x, y) => ActionCopy();
            m_pasteButton = ConfigureActionButton(MainPanel, CommonsSpriteNames.Paste);
            m_pasteButton.eventClick += (x, y) => ActionPaste();
            m_eraseButton = ConfigureActionButton(MainPanel, CommonsSpriteNames.Delete);
            m_eraseButton.eventClick += (x, y) => ActionDelete();
            m_eraseButton.color = Color.red;

            m_vehicleManagementButton = ConfigureActionButton(MainPanel, CommonsSpriteNames.VehicleManagement);
            m_vehicleManagementButton.eventClick += (x, y) => ActionOpenManagementPanel();
            m_vehicleManagementButton.width = 40;
            m_vehicleManagementButton.height = 40;

            removeUndesired.tooltip = Locale.Get("TLM_REMOVE_UNWANTED_TOOLTIP");
            m_copyButton.tooltip = Locale.Get("TLM_COPY_CURRENT_LIST_CLIPBOARD");
            m_pasteButton.tooltip = Locale.Get("TLM_PASTE_CLIPBOARD_TO_CURRENT_LIST");
            m_eraseButton.tooltip = Locale.Get("TLM_DELETE_CURRENT_LIST");

            removeUndesired.relativePosition = new Vector3(MainPanel.width - 50, 0f);
            m_copyButton.relativePosition = new Vector3(MainPanel.width - 75f, 0f);
            m_pasteButton.relativePosition = new Vector3(MainPanel.width - 100f, 0f);
            m_eraseButton.relativePosition = new Vector3(MainPanel.width - 25f, 0f);
            m_vehicleManagementButton.relativePosition = new Vector3(5f, 5f);

            m_copyButton.Hide();
            m_pasteButton.Hide();
            m_eraseButton.Hide();
        }

        private void ActionCopy()
        {
            var lineId = GetLineID();
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, TransportSystem);
            m_clipboard[TransportSystem] = XmlUtils.DefaultXmlSerialize(config.GetAssetTransportListForLine(lineId));
            m_pasteButton.isVisible = m_clipboard.ContainsKey(TransportSystem);
        }

        private void ActionPaste()
        {
            if (!m_clipboard.TryGetValue(TransportSystem, out string data))
            {
                return;
            }

            var lineId = GetLineID();

            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, TransportSystem);

            var assets = XmlUtils.DefaultXmlDeserialize<List<TransportAsset>>(data);

            config.SetAssetTransportListForLine(lineId, assets);

            RefreshSelectedAssetRows();
        }

        private void ActionDelete()
        {
            var lineId = GetLineID();
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, TransportSystem);
            config.SetAssetTransportListForLine(lineId, []);
            RefreshSelectedAssetRows();
        }

        private void ActionOpenManagementPanel()
        {
            var lineId = GetLineID();
            LinePanelManager.SetTarget(lineId);
        }

        protected static UIButton ConfigureActionButton(UIComponent parent, CommonsSpriteNames spriteName)
        {
            MonoUtils.CreateUIElement(out UIButton actionButton, parent.transform, "Btn");
            MonoUtils.InitButton(actionButton, false, "OptionBase");
            actionButton.focusedBgSprite = "";
            actionButton.autoSize = false;
            actionButton.width = 20;
            actionButton.height = 20;
            actionButton.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            actionButton.normalFgSprite = ResourceLoader.GetDefaultSpriteNameFor(spriteName);
            actionButton.canFocus = false;
            return actionButton;
        }

        private void CreateMainPanel()
        {
            MainPanel = GetComponent<UIPanel>();
            MainPanel.relativePosition = new Vector3(510f, 0.0f);
            MainPanel.width = 350;
            MainPanel.height = GetComponentInParent<UIComponent>().height;
            MainPanel.zOrder = 50;
            MainPanel.color = new Color32(255, 255, 255, 255);
            MainPanel.name = "AssetSelectorWindow";
            MainPanel.autoLayoutPadding = new RectOffset(5, 5, 10, 10);
            MainPanel.autoLayout = false;
            MainPanel.useCenter = true;
            MainPanel.wrapLayout = false;
            MainPanel.canFocus = true;

            MonoUtils.CreateUIElement(out m_title, MainPanel.transform);
            m_title.textAlignment = UIHorizontalAlignment.Center;
            m_title.autoSize = false;
            m_title.autoHeight = false;
            m_title.wordWrap = false;
            m_title.width = MainPanel.width - 55f;
            m_title.height = 45f; 
            m_title.relativePosition = new Vector3(5, 10);
            m_title.textScale = 0.9f;
            m_title.localeID = "TLM_ASSETS_FOR_PREFIX";
        }

        private void CreateScrollPanel()
        {
            MonoUtils.CreateScrollPanel(MainPanel, out m_scrollablePanel, out _, MainPanel.width - 25f, MainPanel.height - 245f, new Vector3(5, 100));
            m_scrollablePanel.backgroundSprite = "ScrollbarTrack";
            m_scrollablePanel.scrollPadding.top = 10;
            m_scrollablePanel.scrollPadding.bottom = 10;
            m_scrollablePanel.scrollPadding.left = 6;
            m_scrollablePanel.scrollPadding.right = 8;
            m_scrollablePanel.eventMouseLeave += (x, u) => m_lastInfo = default;
        }

        public void OnSetTarget(Type source)
        {
            if (source == GetType())
            {
                return;
            }

            TransportSystemDefinition tsd = TransportSystem;
            if (!tsd.HasVehicles())
            {
                MainPanel.isVisible = false;
                return;
            }

            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
            m_timeBudgetSelect.isVisible = !fromBuilding;
            m_weightColumnHeader.isVisible = !fromBuilding;
            m_usedCountColumnHeader.isVisible = !fromBuilding;

            var lineExt = TLMTransportLineExtension.Instance;
            bool isAbsolute = lineExt.IsUsingCustomConfig(lineId) && lineExt.IsDisplayAbsoluteValues(lineId);

            if(isAbsolute)
            { 
                m_vehicleCountLabel.isVisible = true;
            }
            else
            {
                m_vehicleCountLabel.isVisible = false;
            }

            LogUtils.DoLog("tsd = {0}", tsd);
            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, tsd);

            UpdateAssetList(config);

            if (lineId == 0)
            {
                m_title.text = Locale.Get("TLM_ASSET_SELECT_WINDOW_TITLE_OUTSIDECONNECTION");
            }
            else if (config is TLMTransportLineConfiguration)
            {
                m_title.text = string.Format(Locale.Get("TLM_ASSET_SELECT_WINDOW_TITLE"), TLMLineUtils.GetLineStringId(GetLineID(), false));
            }
            else
            {
                int prefix = (int)TLMPrefixesUtils.GetPrefix(GetLineID());
                m_title.text = string.Format(Locale.Get("TLM_ASSET_SELECT_WINDOW_TITLE_PREFIX"), prefix > 0 ? NumberingUtils.GetStringFromNumber(TLMPrefixesUtils.GetStringOptionsForPrefix(tsd), prefix + 1) : Locale.Get("TLM_UNPREFIXED"), tsd.GetTransportName());
            }

            bool enabled = false;

            var cfg = TLMLineUtils.GetEffectiveConfigForLine(lineId);

            if (cfg != null)
            {
                enabled = cfg?.UseSeparateWeekendProfile == true;
            }

            m_budgetProfileLabel?.isVisible = enabled;
            m_budgetProfileDropdown?.isVisible = enabled;

            m_budgetProfileLabel?.relativePosition = new Vector3(5f, 0f);
            m_budgetProfileDropdown?.relativePosition = new Vector3(95f, 0f);
        }

        private void UpdateAssetList(IBasicExtension config)
        {
            var lineId = GetLineID();

            if (lineId == 0)
            {
                return;
            }

            var selectedAssets = config.GetAssetTransportListForLine(lineId) ?? [];

            m_lastInfo = default;

            var budgetEntries = config.GetBudgetsMultiplierForLine(lineId, CurrentProfileTarget);

            if (lineId != 0 && (budgetEntries == null || budgetEntries.Count == 0))
            {
                TLMLineUtils.GetBudgetMultiplierLineWithIndexes(lineId); // triggers lazy init
                config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, TransportSystem); // re-fetch

                selectedAssets = config.GetAssetTransportListForLine(lineId) ?? [];

                budgetEntries = config.GetBudgetsMultiplierForLine(lineId, CurrentProfileTarget);
            }

            var slotIndex = GetBudgetSelectedIndex();


            UIPanel[] assetsCheck = m_checkboxTemplateList.SetItemCount(selectedAssets.Count);

            for (int i = 0; i < assetsCheck.Length; i++)
            {
                var asset = selectedAssets[i];
                var controller = assetsCheck[i].GetComponent<TLMAssetItemLine>();
                controller.SetAsset(asset, lineId, slotIndex);
                controller.OnMouseEnter = () =>
                {
                    m_lastInfo = PrefabCollection<VehicleInfo>.FindLoaded(asset.name);
                    RedrawModel();
                };
            }

            UpdateModeIndicator(lineId, slotIndex);
        }

        private void ChangeBudgetTime(int idxSel)
        {
            if (idxSel < 0 || idxSel >= m_timeBudgetSelect.items.Length)
            {
                return;
            }
            m_timeBudgetSelect.selectedIndex = idxSel;

            var lineId = GetLineID();

            int selectedBudgetIndex = GetBudgetSelectedIndex();

            IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(lineId, TransportSystem);
            List<TransportAsset> selectedAssets = config.GetAssetTransportListForLine(lineId);

            UIPanel[] assetsCheck = m_checkboxTemplateList.SetItemCount(selectedAssets.Count);
                  
            for (int i = 0; i < assetsCheck.Length; i++)
            {
                var asset = selectedAssets[i];
                var controller = assetsCheck[i].GetComponent<TLMAssetItemLine>();
                controller.SetAsset(asset, lineId, selectedBudgetIndex);
                controller.OnMouseEnter = () =>
                {
                    m_lastInfo = PrefabCollection<VehicleInfo>.FindLoaded(asset.name);
                    RedrawModel();
                };
            }

            UpdateModeIndicator(lineId, selectedBudgetIndex);
        }

        private void SetPreviewWindow()
        {
            MonoUtils.CreateUIElement(out m_previewPanel, MainPanel.transform);
            m_previewPanel.backgroundSprite = "GenericPanel";
            m_previewPanel.width = MainPanel.width - 15;
            m_previewPanel.height = 140;
            m_previewPanel.relativePosition = new Vector3(7.5f, MainPanel.height - 142);
            MonoUtils.CreateUIElement(out m_preview, m_previewPanel.transform);
            m_preview.size = m_previewPanel.size;
            m_preview.relativePosition = Vector3.zero;
            MonoUtils.CreateElement(out m_previewRenderer, MainPanel.transform);
            m_previewRenderer.Size = m_preview.size * 2f;
            m_preview.texture = m_previewRenderer.Texture;
            m_previewRenderer.Zoom = 3;
            m_previewRenderer.CameraRotation = 40;
        }

        public void UpdateBindings()
        {
            if (m_isDirty)
            {
                m_isDirty = false;
                IBasicExtension config = TLMLineUtils.GetEffectiveExtensionForLine(GetLineID(), TransportSystem);
                UpdateAssetList(config);
            }

            if (GetComponentInParent<UIComponent>().isVisible)
            {
                if (m_lastInfo is null)
                {
                    m_preview.isVisible = false;
                    return;
                }
                else
                {
                    m_preview.isVisible = true;
                    m_previewRenderer.CameraRotation -= 1;
                    RedrawModel();
                }
            }
        }

        private void UpdateModeIndicator(ushort lineId, int budgetIndex)
        {
            m_usedCountColumnHeader.text = Locale.Get("TLM_ASSET_USED_HEADER");
            m_usedCountColumnHeader.isVisible = true;

            var lineExt = TLMTransportLineExtension.Instance;
            bool isAbsolute = lineExt.IsUsingCustomConfig(lineId) && lineExt.IsDisplayAbsoluteValues(lineId);

            if (isAbsolute)
            {
                m_weightColumnHeader.text = Locale.Get("TLM_ASSET_COUNT_HEADER"); // e.g. "Count"
                IBasicExtensionStorage currentConfig = TLMLineUtils.GetEffectiveConfigForLine(lineId);
                var budgetEntries = TLMLineUtils.GetEffectiveExtensionForLine(lineId).GetBudgetsMultiplierForLine(lineId, CurrentProfileTarget);

                if (budgetIndex >= 0 && budgetIndex < budgetEntries.Count)
                {
                    float budgetPercent = budgetEntries[budgetIndex].Value / 100f;
                    float lineLength = TransportManager.instance.m_lines.m_buffer[lineId].m_totalLength;
                    TransportInfo info = TransportManager.instance.m_lines.m_buffer[lineId].Info;
                    int maxVehicles = TLMLineUtils.ProjectTargetVehicleCount(info, lineLength, budgetPercent);
                    IBasicExtension ext = TLMLineUtils.GetEffectiveExtensionForLine(lineId);
                    List<TransportAsset> assets = ext.GetAssetTransportListForLine(lineId);
                    int unassigned = TLMCountModeUtils.GetUnassignedCount(assets, budgetIndex.ToString(), maxVehicles);

                    bool allZero = assets.Count > 0 && assets.All(a => !a.count.TryGetValue(budgetIndex.ToString(), out var ce) || ce.TotalCount == 0);
                    string text = "";
                    string tooltip = "";
                    Color32 color = new(255, 255, 255, 255); // white

                    // e.g. "7 (2)" - this is a warning state that not all vehicles are assigned, even though there is a budget
                    if (unassigned > 0)
                    {
                        text = string.Format(Locale.Get("TLM_ASSET_MAX_VEHICLES_WITH_UNASSIGNED"), maxVehicles, unassigned);
                        tooltip = string.Format(Locale.Get("TLM_ASSET_MAX_VEHICLES_WITH_UNASSIGNED_TOOLTIP"), maxVehicles, unassigned);
                    }
                    // e.g. "⚠" - none assigned, No assets assigned for this time slot
                    else if (allZero && maxVehicles > 0)
                    {
                        text = string.Format(Locale.Get("TLM_NO_VEHICLES_ASSIGNED_WARNING"), maxVehicles, unassigned);
                        tooltip = string.Format(Locale.Get("TLM_NO_VEHICLES_ASSIGNED_WARNING_TOOLTIP"), maxVehicles, unassigned);
                        color = new Color32(255, 200, 0, 255); // yellow
                    }
                    // e.g. "7" - everything is assigned, so no warnings
                    else
                    {
                        text = string.Format(Locale.Get("TLM_ASSET_MAX_VEHICLES"), maxVehicles, unassigned);
                        tooltip = string.Format(Locale.Get("TLM_ASSET_MAX_VEHICLES_TOOLTIP"), maxVehicles, unassigned);
                    }
                    m_vehicleCountLabel.text = text;
                    m_vehicleCountLabel.tooltip = tooltip;
                    m_vehicleCountLabel.color = color;
                    m_vehicleCountLabel.isVisible = true;
                }
            }
            else
            {
                m_weightColumnHeader.text = Locale.Get("TLM_ASSET_PERCENT_HEADER"); // e.g. "%"
                m_vehicleCountLabel.isVisible = false;
            }
        }

        private void RefreshAssetRows(int slotIndex)
        {
            if (slotIndex < 0)
            {
                return;
            }

            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding);
            if (fromBuilding || lineId == 0)
            {
                return;
            }

            foreach (var row in GetComponentsInChildren<TLMAssetItemLine>(true))
            {
                row.RefreshUsageDisplay(lineId, slotIndex);
            }
        }

        private void RedrawModel() => m_previewRenderer.RenderVehicle(m_lastInfo, m_lastColor == Color.clear ? Color.HSVToRGB(Math.Abs(m_previewRenderer.CameraRotation) / 360f, .5f, .5f) : m_lastColor, true);

        private bool TryGetCurrentLineConfig(out ushort lineId, out IBudgetStorage cfg)
        {
            cfg = null;
            if (!UVMPublicTransportWorldInfoPanel.GetLineID(out lineId, out bool fromBuilding) || fromBuilding)
            {
                return false;
            }

            cfg = TLMLineUtils.GetEffectiveConfigForLine(lineId);
            return cfg != null;
        } 
    }
}
