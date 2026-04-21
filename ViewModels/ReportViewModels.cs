using DonorTrackingSystem.Models;

namespace DonorTrackingSystem.ViewModels
{
    // ViewModel for the forecasting report, showing YTD totals and comparisons
    public class ForecastingReportRow
    {
        public string DonorName { get; set; } = string.Empty;
        public decimal YtdTotal { get; set; }
        public decimal PriorYtdTotal { get; set; }
        public decimal PriorYearTotal { get; set; }
    }

    public class NonMemberReportRow
    {
        public string Name { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }
        public DateTime? LastDonationDate { get; set; }
    }

    public class FinancialReportRow
    {
        public string MonthName { get; set; } = string.Empty;
        public int Month { get; set; }
        public decimal CurrentYtd { get; set; }
        public decimal PriorYtd { get; set; }
    }

    public class FinancialReportViewModel
    {
        public List<FinancialReportRow> MonthlyRows { get; set; } = new();
        public decimal CurrentYearTotal { get; set; }
        public decimal PriorYearTotal { get; set; }
        public int CurrentYear { get; set; }
        public int PriorYear { get; set; }
    }
}
