using Klyte.TransportLinesManager.Data.Managers;
using Klyte.TransportLinesManager.WorldInfoPanels.Components;
using System.Collections.Generic;
using static Klyte.TransportLinesManager.Data.Managers.TLMTransportLineStatusesManager;

namespace Klyte.TransportLinesManager.WorldInfoPanels.ReportTabs
{

    internal class TLMLinePassengerWealthReportTab : BasicReportTab<TLMPassengerWealthReportLine, WealthPassengerReport>
    {
        protected override string TitleLocaleID { get; } = "K45_TLM_PASSENGERS_WEALTH_LINE_REPORT";
        public override bool MayBeVisible() => true;
        protected override List<WealthPassengerReport> GetReportData(ushort lineId) => TLMTransportLineStatusesManager.instance.GetLineWealthReport(lineId);
        protected override void AddToTotalizer(ref WealthPassengerReport totalizer, WealthPassengerReport data)
        {
            totalizer.Low +=    data.Low;
            totalizer.Medium += data.Medium;
            totalizer.High +=   data.High;
        }
    }
}