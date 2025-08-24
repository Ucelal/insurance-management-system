using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsuranceAPI.DTOs;
using InsuranceAPI.Services;
using System.Security.Claims;

namespace InsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Satış raporu oluştur
        /// </summary>
        [HttpPost("sales")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<SalesReportDto>> GenerateSalesReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GenerateSalesReportAsync(filter);
                
                _logger.LogInformation("Sales report generated successfully for period {StartDate} to {EndDate}", 
                    filter.StartDate, filter.EndDate);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating sales report");
                return StatusCode(500, "Satış raporu oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Hasar raporu oluştur
        /// </summary>
        [HttpPost("claims")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<ClaimsReportDto>> GenerateClaimsReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GenerateClaimsReportAsync(filter);
                
                _logger.LogInformation("Claims report generated successfully for period {StartDate} to {EndDate}", 
                    filter.StartDate, filter.EndDate);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating claims report");
                return StatusCode(500, "Hasar raporu oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Müşteri raporu oluştur
        /// </summary>
        [HttpPost("customers")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<CustomerReportDto>> GenerateCustomerReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GenerateCustomerReportAsync(filter);
                
                _logger.LogInformation("Customer report generated successfully for period {StartDate} to {EndDate}", 
                    filter.StartDate, filter.EndDate);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating customer report");
                return StatusCode(500, "Müşteri raporu oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Ödeme raporu oluştur
        /// </summary>
        [HttpPost("payments")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<PaymentReportDto>> GeneratePaymentReport([FromBody] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GeneratePaymentReportAsync(filter);
                
                _logger.LogInformation("Payment report generated successfully for period {StartDate} to {EndDate}", 
                    filter.StartDate, filter.EndDate);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating payment report");
                return StatusCode(500, "Ödeme raporu oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Dashboard özet bilgileri
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<Dictionary<string, object>>> GetDashboardSummary()
        {
            try
            {
                var result = await _reportService.GetDashboardSummaryAsync();
                
                _logger.LogInformation("Dashboard summary generated successfully");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating dashboard summary");
                return StatusCode(500, "Dashboard özeti oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Grafik verisi getir
        /// </summary>
        [HttpGet("charts/{chartType}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<List<object>>> GetChartData(string chartType, [FromQuery] ReportFilterDto filter)
        {
            try
            {
                var result = await _reportService.GetChartDataAsync(chartType, filter);
                
                _logger.LogInformation("Chart data generated successfully for type {ChartType}", chartType);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating chart data for type {ChartType}", chartType);
                return StatusCode(500, "Grafik verisi oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Raporu Excel formatında export et
        /// </summary>
        [HttpPost("export/excel/{reportType}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<IActionResult> ExportToExcel(string reportType, [FromBody] ReportFilterDto filter)
        {
            try
            {
                object reportData;
                
                // Rapor türüne göre veri oluştur
                switch (reportType.ToLower())
                {
                    case "sales":
                        reportData = await _reportService.GenerateSalesReportAsync(filter);
                        break;
                    case "claims":
                        reportData = await _reportService.GenerateClaimsReportAsync(filter);
                        break;
                    case "customers":
                        reportData = await _reportService.GenerateCustomerReportAsync(filter);
                        break;
                    case "payments":
                        reportData = await _reportService.GeneratePaymentReportAsync(filter);
                        break;
                    default:
                        return BadRequest($"Desteklenmeyen rapor türü: {reportType}");
                }

                var excelData = await _reportService.ExportReportToExcelAsync(reportData, reportType);
                
                _logger.LogInformation("Excel export completed successfully for report type {ReportType}", reportType);
                
                var fileName = $"{reportType}_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting to Excel for report type {ReportType}", reportType);
                return StatusCode(500, "Excel export sırasında bir hata oluştu");
            }
        }

        /// <summary>
        /// Raporu PDF formatında export et
        /// </summary>
        [HttpPost("export/pdf/{reportType}")]
        [Authorize(Roles = "admin,agent")]
        public async Task<IActionResult> ExportToPdf(string reportType, [FromBody] ReportFilterDto filter)
        {
            try
            {
                object reportData;
                
                // Rapor türüne göre veri oluştur
                switch (reportType.ToLower())
                {
                    case "sales":
                        reportData = await _reportService.GenerateSalesReportAsync(filter);
                        break;
                    case "claims":
                        reportData = await _reportService.GenerateClaimsReportAsync(filter);
                        break;
                    case "customers":
                        reportData = await _reportService.GenerateCustomerReportAsync(filter);
                        break;
                    case "payments":
                        reportData = await _reportService.GeneratePaymentReportAsync(filter);
                        break;
                    default:
                        return BadRequest($"Desteklenmeyen rapor türü: {reportType}");
                }

                var pdfData = await _reportService.ExportReportToPdfAsync(reportData, reportType);
                
                _logger.LogInformation("PDF export completed successfully for report type {ReportType}", reportType);
                
                var fileName = $"{reportType}_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
                
                return File(pdfData, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting to PDF for report type {ReportType}", reportType);
                return StatusCode(500, "PDF export sırasında bir hata oluştu");
            }
        }

        /// <summary>
        /// Hızlı satış raporu (son 30 gün)
        /// </summary>
        [HttpGet("sales/quick")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<SalesReportDto>> GetQuickSalesReport()
        {
            try
            {
                var filter = new ReportFilterDto
                {
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow
                };

                var result = await _reportService.GenerateSalesReportAsync(filter);
                
                _logger.LogInformation("Quick sales report generated successfully");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating quick sales report");
                return StatusCode(500, "Hızlı satış raporu oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Hızlı hasar raporu (son 30 gün)
        /// </summary>
        [HttpGet("claims/quick")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<ClaimsReportDto>> GetQuickClaimsReport()
        {
            try
            {
                var filter = new ReportFilterDto
                {
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow
                };

                var result = await _reportService.GenerateClaimsReportAsync(filter);
                
                _logger.LogInformation("Quick claims report generated successfully");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating quick claims report");
                return StatusCode(500, "Hızlı hasar raporu oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Acente performans raporu
        /// </summary>
        [HttpGet("agent-performance/{agentId}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<object>> GetAgentPerformanceReport(int agentId, [FromQuery] ReportFilterDto filter)
        {
            try
            {
                filter.AgentId = agentId;
                
                var salesReport = await _reportService.GenerateSalesReportAsync(filter);
                var claimsReport = await _reportService.GenerateClaimsReportAsync(filter);
                
                var performanceReport = new
                {
                    AgentId = agentId,
                    Period = new { StartDate = filter.StartDate, EndDate = filter.EndDate },
                    Sales = new
                    {
                        TotalPolicies = salesReport.TotalPolicies,
                        TotalPremium = salesReport.TotalPremium,
                        AveragePremium = salesReport.TotalPolicies > 0 ? salesReport.TotalPremium / salesReport.TotalPolicies : 0
                    },
                    Claims = new
                    {
                        TotalClaims = claimsReport.TotalClaims,
                        TotalAmount = claimsReport.TotalClaimAmount,
                        AverageAmount = claimsReport.AverageClaimAmount,
                        PendingClaims = claimsReport.PendingClaims
                    },
                    Performance = new
                    {
                        ConversionRate = salesReport.TotalPolicies > 0 ? (decimal)claimsReport.TotalClaims / salesReport.TotalPolicies * 100 : 0,
                        Efficiency = salesReport.TotalPremium > 0 ? (salesReport.TotalPremium - claimsReport.TotalClaimAmount) / salesReport.TotalPremium * 100 : 0
                    }
                };
                
                _logger.LogInformation("Agent performance report generated successfully for agent {AgentId}", agentId);
                
                return Ok(performanceReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating agent performance report for agent {AgentId}", agentId);
                return StatusCode(500, "Acente performans raporu oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Müşteri segmentasyon raporu
        /// </summary>
        [HttpGet("customer-segmentation")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<object>> GetCustomerSegmentationReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var customerReport = await _reportService.GenerateCustomerReportAsync(filter);
                var salesReport = await _reportService.GenerateSalesReportAsync(filter);
                
                var segmentationReport = new
                {
                    Period = new { StartDate = filter.StartDate, EndDate = filter.EndDate },
                    TotalCustomers = customerReport.TotalCustomers,
                    Segmentation = new
                    {
                        ByType = customerReport.CustomersByType,
                        ByRegion = customerReport.CustomersByRegion,
                        ByActivity = new
                        {
                            Active = customerReport.ActiveCustomers,
                            Inactive = customerReport.InactiveCustomers,
                            New = customerReport.NewCustomers
                        }
                    },
                    Revenue = new
                    {
                        TotalPremium = salesReport.TotalPremium,
                        AveragePremiumPerCustomer = customerReport.TotalCustomers > 0 ? salesReport.TotalPremium / customerReport.TotalCustomers : 0,
                        PremiumByCustomerType = salesReport.SalesByInsuranceType
                    }
                };
                
                _logger.LogInformation("Customer segmentation report generated successfully");
                
                return Ok(segmentationReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating customer segmentation report");
                return StatusCode(500, "Müşteri segmentasyon raporu oluşturulurken bir hata oluştu");
            }
        }

        /// <summary>
        /// Trend analizi raporu
        /// </summary>
        [HttpGet("trend-analysis")]
        [Authorize(Roles = "admin,agent")]
        public async Task<ActionResult<object>> GetTrendAnalysisReport([FromQuery] ReportFilterDto filter)
        {
            try
            {
                var salesReport = await _reportService.GenerateSalesReportAsync(filter);
                var claimsReport = await _reportService.GenerateClaimsReportAsync(filter);
                
                var trendReport = new
                {
                    Period = new { StartDate = filter.StartDate, EndDate = filter.EndDate },
                    SalesTrends = new
                    {
                        MonthlyData = salesReport.SalesByMonth,
                        InsuranceTypeTrends = salesReport.SalesByInsuranceType,
                        AgentPerformance = salesReport.SalesByAgent
                    },
                    ClaimsTrends = new
                    {
                        MonthlyData = claimsReport.ClaimsByMonth,
                        StatusDistribution = claimsReport.ClaimsByStatus,
                        TypeDistribution = claimsReport.ClaimsByType
                    },
                    Insights = new
                    {
                        GrowthRate = CalculateGrowthRate(salesReport.SalesByMonth),
                        ClaimsRatio = salesReport.TotalPremium > 0 ? claimsReport.TotalClaimAmount / salesReport.TotalPremium * 100 : 0,
                        PeakMonths = GetPeakMonths(salesReport.SalesByMonth)
                    }
                };
                
                _logger.LogInformation("Trend analysis report generated successfully");
                
                return Ok(trendReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating trend analysis report");
                return StatusCode(500, "Trend analizi raporu oluşturulurken bir hata oluştu");
            }
        }

        #region Helper Methods

        private decimal CalculateGrowthRate(List<SalesByMonthDto> monthlyData)
        {
            if (monthlyData.Count < 2) return 0;
            
            var firstMonth = monthlyData.First();
            var lastMonth = monthlyData.Last();
            
            if (firstMonth.TotalPremium == 0) return 0;
            
            return ((lastMonth.TotalPremium - firstMonth.TotalPremium) / firstMonth.TotalPremium) * 100;
        }

        private List<string> GetPeakMonths(List<SalesByMonthDto> monthlyData)
        {
            if (monthlyData.Count == 0) return new List<string>();
            
            var averagePremium = monthlyData.Average(m => m.TotalPremium);
            var peakMonths = monthlyData
                .Where(m => m.TotalPremium > averagePremium * 1.2m) // %20 üzeri peak olarak kabul et
                .Select(m => m.MonthName)
                .ToList();
            
            return peakMonths;
        }

        #endregion
    }
}
