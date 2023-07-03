using TransportLinesManager.Data.Managers;
using TransportLinesManager.WorldInfoPanels.Components;
using System.Collections.Generic;
using static TransportLinesManager.Data.Managers.TLMTransportLineStatusesManager;

namespace TransportLinesManager.WorldInfoPanels.ReportTabs
{

    internal class TLMLineFinanceReportTab : BasicReportTab<TLMFinanceReportLine, IncomeExpenseReport>
    {
        protected override string TitleLocaleID { get; } = "K45_TLM_FINANCIAL_REPORT";
        public override bool MayBeVisible() => UVMPublicTransportWorldInfoPanel.GetCurrentTSD().HasVehicles();
        protected override List<IncomeExpenseReport> GetReportData(ushort lineId) => TLMTransportLineStatusesManager.instance.GetLineFinanceReport(lineId);
        protected override void AddToTotalizer(ref IncomeExpenseReport totalizer, IncomeExpenseReport data)
        {
            totalizer.Income += data.Income;
            totalizer.Expense += data.Expense;
        }
    }
}