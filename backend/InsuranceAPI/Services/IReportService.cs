using InsuranceAPI.DTOs;

namespace InsuranceAPI.Services
{
    public interface IReportService
    {
        Task<SalesReportDto> GenerateSalesReportAsync(ReportFilterDto filter);
        Task<ClaimsReportDto> GenerateClaimsReportAsync(ReportFilterDto filter);
        Task<CustomerReportDto> GenerateCustomerReportAsync(ReportFilterDto filter);
        Task<PaymentReportDto> GeneratePaymentReportAsync(ReportFilterDto filter);
        Task<byte[]> ExportReportToExcelAsync(object reportData, string reportType);
        Task<byte[]> ExportReportToPdfAsync(object reportData, string reportType);
        Task<Dictionary<string, object>> GetDashboardSummaryAsync();
        Task<List<object>> GetChartDataAsync(string chartType, ReportFilterDto filter);
    }
}
