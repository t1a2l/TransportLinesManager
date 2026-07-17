using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Components;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.UI;
using TransportLinesManager.Interfaces;
using Commons.Utils;
using ColossalFramework.Globalization;
using Commons.Extensions.UI;
using System.Linq;
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

        public override void ExtraOnSetTarget(ushort lineID)
        {
            m_ticketPriceProfileLabel?.relativePosition = new Vector3(0f, 0f);
            m_ticketPriceProfileDropdown?.relativePosition = new Vector3(90f, 0f);
        }

        public override float GetMaxSliderValue()
        {
            if (UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineId, out bool fromBuilding) && !fromBuilding)
            {
                return TLMLineUtils.GetDefaultTicketPrice(lineId) * 5;
            }
            return 0;
        }

        public override string GetComponentName() => "TicketPriceConfigTabLabel";

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

        protected override TicketPriceEntryXml DefaultEntry(ushort lineId)
        {
            // find first hour not already used
            var usedHours = new HashSet<int>(Config.Select(x => x.HourOfDay ?? -1));
            int availableHour = Enumerable.Range(0, 24).FirstOrDefault(h => !usedHours.Contains(h));

            return new()
            {
                HourOfDay = availableHour >= 0 ? availableHour : 0,
                Value = TLMLineUtils.GetDefaultTicketPrice(lineId)
            };
        }

        public override string GetTemplateName() => TLMTicketPriceEditorLine.TICKET_PRICE_LINE_TEMPLATE;

        public override void EnsureTemplate() => TLMTicketPriceEditorLine.EnsureTemplate();

        public void UpdateWeekendTicketPriceUIState()
        {
            bool enabled = false;

            if (TryGetCurrentLineConfig(out ushort _, out ITicketPriceStorage cfg))
            {
                enabled = cfg?.UseSeparateWeekendProfile == true;
            }

            m_ticketPriceProfilePanel?.isVisible = enabled;
            m_ticketPriceProfileLabel?.isVisible = enabled;
            m_ticketPriceProfileDropdown?.isVisible = enabled;

            m_ticketPriceProfileLabel?.relativePosition = new Vector3(0f, 0f);
            m_ticketPriceProfileDropdown?.relativePosition = new Vector3(90f, 0f);
        }

        public TimeableList<TicketPriceEntryXml> CloneTicketPriceEntries(TimeableList<TicketPriceEntryXml> src, ushort lineId)
        {
            var result = new TimeableList<TicketPriceEntryXml>();

            if (src == null || src.Count == 0)
            {
                result.Add(new TicketPriceEntryXml
                {
                    HourOfDay = 0,
                    Value = TLMLineUtils.GetDefaultTicketPrice(lineId)
                });
                return result;
            }

            for (int i = 0; i < src.Count; i++)
            {
                result.Add(new TicketPriceEntryXml
                {
                    HourOfDay = src[i].HourOfDay,
                    Value = src[i].Value
                });
            }

            return result;
        }

        private void CreateWeekendTicketPriceControls(UIComponent parent)
        {
            MonoUtils.CreateUIElement(out m_ticketPriceProfilePanel, parent.transform);
            m_ticketPriceProfilePanel.name = "TicketPriceProfilePanel";
            m_ticketPriceProfilePanel.width = 370f;
            m_ticketPriceProfilePanel.height = 24f;
            m_ticketPriceProfilePanel.autoLayout = false;

            MonoUtils.CreateUIElement(out m_ticketPriceProfileLabel, m_ticketPriceProfilePanel.transform);
            m_ticketPriceProfileLabel.name = "TicketPriceProfileLabel";
            m_ticketPriceProfileLabel.text = Locale.Get("TLM_PROFILE");
            m_ticketPriceProfileLabel.textScale = 0.9f;
            m_ticketPriceProfileLabel.autoSize = false;
            m_ticketPriceProfileLabel.width = 90f;
            m_ticketPriceProfileLabel.height = 24f;
            m_ticketPriceProfileLabel.relativePosition = new Vector3(0f, 0f);
            m_ticketPriceProfileLabel.verticalAlignment = UIVerticalAlignment.Middle;

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
            m_ticketPriceProfileDropdown.relativePosition = new Vector3(90f, 0f);
            m_ticketPriceProfileDropdown.eventSelectedIndexChanged += OnTicketPriceProfileChanged;

            UpdateWeekendTicketPriceUIState();
        }

        private void OnTicketPriceProfileChanged(UIComponent component, int value)
        {
            m_editingWeekendTicketPrice = value == 1;
            ReloadTicketPriceListFromCurrentProfile();
            UpdateWeekendTicketPriceUIState();
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
