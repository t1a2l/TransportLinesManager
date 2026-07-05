using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Components;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;
using TransportLinesManager.Interfaces;
using Commons.Utils;
using ColossalFramework.Globalization;
using Commons.Extensions.UI;
using static TransportLinesManager.Data.Extensions.ExtensionStaticExtensionMethods;

namespace TransportLinesManager.WorldInfoPanels.Tabs
{
    public class TLMTicketConfigTab : TLMBaseTimedConfigTab<TLMTicketConfigTab, TLMTicketPriceTimeChart, TLMTicketPriceEditorLine, TicketPriceEntryXml>
    {
        public override string GetTitleLocale() => "TLM_PER_HOUR_TICKET_PRICE_TITLE";

        public override string GetValueColumnLocale() => "TLM_TICKET_PRICE";

        public ProfileTarget CurrentProfileTarget => m_editingWeekendTicketPrice ? ProfileTarget.Weekend : ProfileTarget.Weekday;

        private UIPanel m_ticketPriceProfilePanel;

        private UILabel m_ticketPriceProfileLabel;

        private UIDropDown m_ticketPriceProfileDropdown;

        private bool m_editingWeekendTicketPrice;

        public override void ExtraAwake()
        {
            m_uiHelper.AddSpace(20);
            CreateWeekendTicketPriceControls(this.component);
        }

        public override float GetMaxSliderValue()
        {
            if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && !fromBuilding)
            {
                var tsd = TransportSystemDefinition.GetDefinitionForLine(ref TransportManager.instance.m_lines.m_buffer[lineId]);
                return TLMLineUtils.GetTicketPriceForLine(tsd, 0, CurrentProfileTarget).First.Value * 5;
            }
            return 0;
        }

        internal override List<Color> ColorOrder { get; } =
        [
            Color.Lerp(Color.red,Color.magenta,0.5f),
            Color.magenta,
            Color.Lerp(Color.blue,Color.magenta,0.5f),
            Color.blue,
            Color.Lerp(Color.blue,Color.cyan,0.5f),
            Color.cyan,
            Color.green,
            Color.yellow,
            Color.Lerp(Color.red,Color.yellow,0.5f),
            Color.red,
        ];

        protected override TimeableList<TicketPriceEntryXml> Config
        {
            get
            {
                if (!TryGetCurrentLineConfig(out ushort _, out ITicketPriceStorage cfg))
                {
                    return null;
                }

                return CurrentProfileTarget == ProfileTarget.Weekend ? (cfg.WeekendTicketPriceEntries ?? []) : cfg.TicketPriceEntries;
            }
        }

        protected override TicketPriceEntryXml DefaultEntry() => new()
        {
            HourOfDay = 0,
            Value = 0
        };

        public override string GetTemplateName() => TLMTicketPriceEditorLine.TICKET_PRICE_LINE_TEMPLATE;

        public override void EnsureTemplate() => TLMTicketPriceEditorLine.EnsureTemplate();

        private void CreateWeekendTicketPriceControls(UIComponent parent)
        {
            MonoUtils.CreateUIElement(out m_ticketPriceProfilePanel, m_uiHelper.Self.transform);
            m_ticketPriceProfilePanel.name = "TicketPriceProfilePanel";
            m_ticketPriceProfilePanel.width = 140f;
            m_ticketPriceProfilePanel.height = 40f;
            m_ticketPriceProfilePanel.autoLayout = false;

            MonoUtils.CreateUIElement(out m_ticketPriceProfileLabel, m_ticketPriceProfilePanel.transform);
            m_ticketPriceProfileLabel.name = "TicketPriceProfileLabel";
            m_ticketPriceProfileLabel.text = Locale.Get("TLM_TICKET_PRICE_PROFILE");
            m_ticketPriceProfileLabel.textScale = 0.9f;
            m_ticketPriceProfileLabel.autoSize = false;
            m_ticketPriceProfileLabel.width = 120f;
            m_ticketPriceProfileLabel.height = 22f;
            m_ticketPriceProfileLabel.relativePosition = new Vector3(0f, 22f);

            var ddGo = Instantiate(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate).GetComponent<UIPanel>().Find<UIDropDown>("Dropdown").gameObject, m_ticketPriceProfilePanel.transform);

            m_ticketPriceProfileDropdown = ddGo.GetComponent<UIDropDown>();
            m_ticketPriceProfileDropdown.name = "TicketPriceProfileDropdown";
            m_ticketPriceProfileDropdown.width = 140f;
            m_ticketPriceProfileDropdown.height = 24f;
            m_ticketPriceProfileDropdown.textScale = 0.85f;
            m_ticketPriceProfileDropdown.itemHeight = 24;
            m_ticketPriceProfileDropdown.textFieldPadding = new RectOffset(6, 6, 4, 4);
            m_ticketPriceProfileDropdown.itemPadding = new RectOffset(6, 6, 2, 2);
            m_ticketPriceProfileDropdown.normalBgSprite = "OptionsDropboxListbox";
            m_ticketPriceProfileDropdown.items =
            [
                Locale.Get("TLM_PROFILE_WEEKDAY"),
                Locale.Get("TLM_PROFILE_WEEKEND")
            ];
            m_ticketPriceProfileDropdown.selectedIndex = 0;
            m_ticketPriceProfileDropdown.relativePosition = new Vector3(130f, 18f);
            m_ticketPriceProfileDropdown.eventSelectedIndexChanged += OnTicketPriceProfileChanged;
            UpdateWeekendTicketPriceUIState();
        }

        private void OnTicketPriceProfileChanged(UIComponent component, int value)
        {
            m_editingWeekendTicketPrice = value == 1;
            ReloadTicketPriceListFromCurrentProfile();
            UpdateWeekendTicketPriceUIState();
        }

        private void UpdateWeekendTicketPriceUIState()
        {
            bool enabled = false;

            if (TryGetCurrentLineConfig(out ushort _, out ITicketPriceStorage cfg))
            {
                enabled = cfg?.UseSeparateWeekendProfile == true;
            }

            m_ticketPriceProfilePanel?.isVisible = enabled;
            m_ticketPriceProfileLabel?.isVisible = enabled;
            m_ticketPriceProfileDropdown?.isVisible = enabled;

            m_ticketPriceProfileLabel.relativePosition = new Vector3(0f, 22f);
            m_ticketPriceProfileDropdown?.relativePosition = new Vector3(130f, 18f);
        }

        private void ReloadTicketPriceListFromCurrentProfile()
        {
            RebuildList();
        }

        private bool TryGetCurrentLineConfig(out ushort lineId, out ITicketPriceStorage cfg)
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
