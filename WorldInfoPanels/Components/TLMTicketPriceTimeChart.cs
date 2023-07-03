using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Commons.Utils;
using Commons.Utils.UtilitiesClasses;
using TransportLinesManager.Data.Base;
using TransportLinesManager.Data.Tsd;
using TransportLinesManager.Data.Extensions;
using TransportLinesManager.UI;
using TransportLinesManager.Utils;
using TransportLinesManager.WorldInfoPanels.Tabs;
using UnityEngine;

namespace TransportLinesManager.WorldInfoPanels.Components
{
    public class TLMTicketPriceTimeChart : TLMBaseTimeChart<TLMTicketConfigTab, TLMTicketPriceTimeChart, TLMTicketPriceEditorLine, TicketPriceEntryXml>
    {
        private UILabel m_effectiveLabel;

        public override TimeableList<TicketPriceEntryXml> GetCopyTarget()
        {
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineID, out bool fromBuilding);
            return !fromBuilding ? TLMLineUtils.GetEffectiveExtensionForLine(lineID).GetTicketPricesForLine(lineID) : null;
        }
        public override void SetPasteTarget(TimeableList<TicketPriceEntryXml> newVal)
        {
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineID, out bool fromBuilding);
            if (!fromBuilding)
            {
                TLMLineUtils.GetEffectiveExtensionForLine(lineID).SetTicketPricesForLine(lineID, newVal);
            }
        }
        public override void OnDeleteTarget()
        {
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineID, out bool fromBuilding);
            if (!fromBuilding)
            {
                TLMLineUtils.GetEffectiveExtensionForLine(lineID).ClearTicketPricesOfLine(lineID);
            }
        }

        public override void CreateLabels()
        {
            MonoUtils.CreateUIElement(out UILabel titleEffective, m_container.transform, "TitleEffective");
            titleEffective.width = 70;
            titleEffective.height = 30;
            MonoUtils.LimitWidthAndBox(titleEffective, 70, out UIPanel container, true);
            container.relativePosition = new Vector3(70, 0);
            titleEffective.textScale = 0.8f;
            titleEffective.color = Color.white;
            titleEffective.isLocalized = true;
            titleEffective.localeID = "TLM_EFFECTIVE_TICKET_PRICE_NOW";
            titleEffective.textAlignment = UIHorizontalAlignment.Center;

            MonoUtils.CreateUIElement(out UIPanel effectiveContainer, m_container.transform, "ValueEffectiveContainer");
            effectiveContainer.width = 70;
            effectiveContainer.height = 40;
            effectiveContainer.relativePosition = new Vector3(70, 30);
            effectiveContainer.color = Color.white;
            effectiveContainer.autoLayout = false;

            MonoUtils.CreateUIElement(out m_effectiveLabel, effectiveContainer.transform, "BarLabel");
            m_effectiveLabel.width = 70;
            m_effectiveLabel.height = 40;
            m_effectiveLabel.relativePosition = new Vector3(0, 0);
            m_effectiveLabel.color = Color.white;
            m_effectiveLabel.isLocalized = false;
            m_effectiveLabel.textAlignment = UIHorizontalAlignment.Center;
            m_effectiveLabel.verticalAlignment = UIVerticalAlignment.Middle;
            m_effectiveLabel.useOutline = true;
            m_effectiveLabel.backgroundSprite = "PlainWhite";
            m_effectiveLabel.padding.top = 3;
            MonoUtils.LimitWidthAndBox(m_effectiveLabel, 70, true);
        }

        public override void OnUpdate()
        {
            UVMPublicTransportWorldInfoPanel.GetLineID(out ushort lineID, out bool fromBuilding);
            if (!fromBuilding)
            {
                var tsd = TransportSystemDefinition.FromLineId(lineID, fromBuilding);
                Tuple<TicketPriceEntryXml, int> value = TLMLineUtils.GetTicketPriceForLine(tsd, lineID);
                m_effectiveLabel.color = value.Second < 0 ? Color.gray : TLMTicketConfigTab.Instance.ColorOrder[value.Second % TLMTicketConfigTab.Instance.ColorOrder.Count];
                m_effectiveLabel.text = (value.First.Value / 100f).ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
            }
        }


        public override string ClockTooltip { get; } = "TLM_TICKET_PRICE_CLOCK";

    }
}