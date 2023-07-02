using Klyte.TransportLinesManager.Data.Managers;
using Klyte.TransportLinesManager.WorldInfoPanels.Components;
using System.Collections.Generic;
using static Klyte.TransportLinesManager.Data.Managers.TLMTransportLineStatusesManager;

namespace Klyte.TransportLinesManager.WorldInfoPanels.ReportTabs
{

    internal class TLMLinePassengerAgeReportTab : BasicReportTab<TLMPassengerAgeReportLine, AgePassengerReport>
    {
        protected override string TitleLocaleID { get; } = "K45_TLM_PASSENGERS_AGE_LINE_REPORT";
        public override bool MayBeVisible() =>true;
        protected override List<AgePassengerReport> GetReportData(ushort lineId) => TLMTransportLineStatusesManager.instance.GetLineAgeReport(lineId);
        protected override void AddToTotalizer(ref AgePassengerReport totalizer, AgePassengerReport data)
        {
            totalizer.Child += data.Child;
            totalizer.Teen +=  data.Teen;
            totalizer.Young += data.Young;
            totalizer.Adult += data.Adult;
            totalizer.Elder += data.Elder;
        }


    }
}