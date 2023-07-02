using Klyte.TransportLinesManager.Data.Managers;
using Klyte.TransportLinesManager.WorldInfoPanels.Components;
using System.Collections.Generic;
using static Klyte.TransportLinesManager.Data.Managers.TLMTransportLineStatusesManager;

namespace Klyte.TransportLinesManager.WorldInfoPanels.ReportTabs
{

    internal class TLMLinePassengerGenderReportTab : BasicReportTab<TLMPassengerGenderReportLine, GenderPassengerReport>
    {
        protected override string TitleLocaleID { get; } = "K45_TLM_PASSENGERS_GENDER_LINE_REPORT";
        public override bool MayBeVisible() => true;
        protected override List<GenderPassengerReport> GetReportData(ushort lineId) => TLMTransportLineStatusesManager.instance.GetLineGenderReport(lineId);
        protected override void AddToTotalizer(ref GenderPassengerReport totalizer, GenderPassengerReport data)
        {
            totalizer.Male +=   data.Male;
            totalizer.Female += data.Female;
        }

    }
}