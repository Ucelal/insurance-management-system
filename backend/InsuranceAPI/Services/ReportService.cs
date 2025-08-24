using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAPI.Services
{
    public class ReportService : IReportService
    {
        private readonly InsuranceDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(InsuranceDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SalesReportDto> GenerateSalesReportAsync(ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.UtcNow.AddYears(-1);
                var endDate = filter.EndDate ?? DateTime.UtcNow;

                var policiesQuery = _context.Policies
                    .Include(p => p.Offer)
                    .ThenInclude(o => o.Customer)
                    .Include(p => p.Offer)
                    .ThenInclude(o => o.InsuranceType)
                    .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

                if (filter.CustomerId.HasValue)
                    policiesQuery = policiesQuery.Where(p => p.Offer.CustomerId == filter.CustomerId.Value);

                if (filter.AgentId.HasValue)
                    policiesQuery = policiesQuery.Where(p => p.Offer.AgentId == filter.AgentId.Value);

                if (!string.IsNullOrEmpty(filter.InsuranceType))
                    policiesQuery = policiesQuery.Where(p => p.Offer.InsuranceType.Name == filter.InsuranceType);

                var policies = await policiesQuery.ToListAsync();

                var totalPolicies = policies.Count;
                var totalPremium = policies.Sum(p => p.TotalPremium);
                var totalCustomers = policies.Select(p => p.Offer.CustomerId).Distinct().Count();
                var totalAgents = policies.Select(p => p.Offer.AgentId).Distinct().Count();

                // Sales by Agent
                var salesByAgent = await _context.Agents
                    .Include(a => a.User)
                    .Select(a => new SalesByAgentDto
                    {
                        AgentId = a.Id,
                        AgentName = a.User.Name,
                        AgentCode = a.AgentCode,
                        PoliciesSold = policies.Count(p => p.Offer.AgentId == a.Id),
                        TotalPremium = policies.Where(p => p.Offer.AgentId == a.Id).Sum(p => p.TotalPremium),
                        Commission = policies.Where(p => p.Offer.AgentId == a.Id).Sum(p => p.TotalPremium * 0.1m) // 10% commission
                    })
                    .Where(s => s.PoliciesSold > 0)
                    .OrderByDescending(s => s.TotalPremium)
                    .ToListAsync();

                // Sales by Month
                var salesByMonth = policies
                    .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                    .Select(g => new SalesByMonthDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                        PoliciesSold = g.Count(),
                        TotalPremium = g.Sum(p => p.TotalPremium)
                    })
                    .OrderBy(s => s.Year)
                    .ThenBy(s => s.Month)
                    .ToList();

                // Sales by Insurance Type
                var salesByInsuranceType = policies
                    .GroupBy(p => p.Offer.InsuranceType.Name)
                    .Select(g => new SalesByInsuranceTypeDto
                    {
                        InsuranceType = g.Key,
                        PoliciesSold = g.Count(),
                        TotalPremium = g.Sum(p => p.TotalPremium),
                        AveragePremium = g.Average(p => p.TotalPremium)
                    })
                    .OrderByDescending(s => s.TotalPremium)
                    .ToList();

                _logger.LogInformation("Sales report generated for period {StartDate} to {EndDate}", startDate, endDate);

                return new SalesReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPolicies = totalPolicies,
                    TotalPremium = totalPremium,
                    TotalCustomers = totalCustomers,
                    TotalAgents = totalAgents,
                    SalesByAgent = salesByAgent,
                    SalesByMonth = salesByMonth,
                    SalesByInsuranceType = salesByInsuranceType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating sales report");
                throw;
            }
        }

        public async Task<ClaimsReportDto> GenerateClaimsReportAsync(ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.UtcNow.AddYears(-1);
                var endDate = filter.EndDate ?? DateTime.UtcNow;

                var claimsQuery = _context.Claims
                    .Include(c => c.Policy)
                    .ThenInclude(p => p.Offer)
                    .ThenInclude(o => o.Customer)
                    .Include(c => c.Policy)
                    .ThenInclude(p => p.Offer)
                    .ThenInclude(o => o.Agent)
                    .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate);

                if (filter.CustomerId.HasValue)
                    claimsQuery = claimsQuery.Where(c => c.Policy.Offer.CustomerId == filter.CustomerId.Value);

                if (filter.AgentId.HasValue)
                    claimsQuery = claimsQuery.Where(c => c.Policy.Offer.AgentId == filter.AgentId.Value);

                if (!string.IsNullOrEmpty(filter.Status))
                    claimsQuery = claimsQuery.Where(c => c.Status.ToString() == filter.Status);

                var claims = await claimsQuery.ToListAsync();

                var totalClaims = claims.Count;
                var pendingClaims = claims.Count(c => c.Status == ClaimStatus.Pending);
                var approvedClaims = claims.Count(c => c.Status == ClaimStatus.Approved);
                var rejectedClaims = claims.Count(c => c.Status == ClaimStatus.Rejected);
                var resolvedClaims = claims.Count(c => c.Status == ClaimStatus.Resolved);
                var totalClaimAmount = claims.Sum(c => c.ClaimAmount ?? 0);
                var averageClaimAmount = totalClaims > 0 ? totalClaimAmount / totalClaims : 0;

                // Claims by Status
                var claimsByStatus = claims
                    .GroupBy(c => c.Status)
                    .Select(g => new ClaimsByStatusDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        TotalAmount = g.Sum(c => c.ClaimAmount ?? 0),
                        Percentage = totalClaims > 0 ? (decimal)g.Count() / totalClaims * 100 : 0
                    })
                    .OrderByDescending(c => c.Count)
                    .ToList();

                // Claims by Type
                var claimsByType = claims
                    .GroupBy(c => c.Type)
                    .Select(g => new ClaimsByTypeDto
                    {
                        ClaimType = g.Key.ToString(),
                        Count = g.Count(),
                        TotalAmount = g.Sum(c => c.ClaimAmount ?? 0),
                        AverageAmount = g.Average(c => c.ClaimAmount ?? 0)
                    })
                    .OrderByDescending(c => c.Count)
                    .ToList();

                // Claims by Month
                var claimsByMonth = claims
                    .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                    .Select(g => new ClaimsByMonthDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                        ClaimsCount = g.Count(),
                        TotalAmount = g.Sum(c => c.ClaimAmount ?? 0)
                    })
                    .OrderBy(c => c.Year)
                    .ThenBy(c => c.Month)
                    .ToList();

                _logger.LogInformation("Claims report generated for period {StartDate} to {EndDate}", startDate, endDate);

                return new ClaimsReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalClaims = totalClaims,
                    PendingClaims = pendingClaims,
                    ApprovedClaims = approvedClaims,
                    RejectedClaims = rejectedClaims,
                    ResolvedClaims = resolvedClaims,
                    TotalClaimAmount = totalClaimAmount,
                    AverageClaimAmount = averageClaimAmount,
                    ClaimsByStatus = claimsByStatus,
                    ClaimsByType = claimsByType,
                    ClaimsByMonth = claimsByMonth
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating claims report");
                throw;
            }
        }

        public async Task<CustomerReportDto> GenerateCustomerReportAsync(ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.UtcNow.AddYears(-1);
                var endDate = filter.EndDate ?? DateTime.UtcNow;

                var customersQuery = _context.Customers
                    .Include(c => c.User)
                    .Where(c => c.User.CreatedAt >= startDate && c.User.CreatedAt <= endDate);

                if (filter.CustomerId.HasValue)
                    customersQuery = customersQuery.Where(c => c.Id == filter.CustomerId.Value);

                var customers = await customersQuery.ToListAsync();

                var totalCustomers = customers.Count;
                var newCustomers = customers.Count(c => c.User.CreatedAt >= startDate && c.User.CreatedAt <= endDate);
                var activeCustomers = customers.Count(c => c.User.CreatedAt >= DateTime.UtcNow.AddMonths(-6));
                var inactiveCustomers = totalCustomers - activeCustomers;

                // Customers by Type
                var customersByType = customers
                    .GroupBy(c => c.Type)
                    .Select(g => new CustomersByTypeDto
                    {
                        CustomerType = g.Key,
                        Count = g.Count(),
                        Percentage = totalCustomers > 0 ? (decimal)g.Count() / totalCustomers * 100 : 0
                    })
                    .OrderByDescending(c => c.Count)
                    .ToList();

                // Customers by Region (simplified - using address)
                var customersByRegion = customers
                    .GroupBy(c => GetRegionFromAddress(c.Address ?? string.Empty))
                    .Select(g => new CustomersByRegionDto
                    {
                        Region = g.Key,
                        Count = g.Count(),
                        Percentage = totalCustomers > 0 ? (decimal)g.Count() / totalCustomers * 100 : 0
                    })
                    .OrderByDescending(c => c.Count)
                    .ToList();

                // Customer Retention (simplified)
                var customerRetention = new List<CustomerRetentionDto>
                {
                    new CustomerRetentionDto
                    {
                        Year = DateTime.UtcNow.Year,
                        RetainedCustomers = activeCustomers,
                        TotalCustomers = totalCustomers,
                        RetentionRate = totalCustomers > 0 ? (decimal)activeCustomers / totalCustomers * 100 : 0
                    }
                };

                _logger.LogInformation("Customer report generated for period {StartDate} to {EndDate}", startDate, endDate);

                return new CustomerReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalCustomers = totalCustomers,
                    NewCustomers = newCustomers,
                    ActiveCustomers = activeCustomers,
                    InactiveCustomers = inactiveCustomers,
                    CustomersByType = customersByType,
                    CustomersByRegion = customersByRegion,
                    CustomerRetention = customerRetention
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating customer report");
                throw;
            }
        }

        public async Task<PaymentReportDto> GeneratePaymentReportAsync(ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.UtcNow.AddYears(-1);
                var endDate = filter.EndDate ?? DateTime.UtcNow;

                var paymentsQuery = _context.Payments
                    .Include(p => p.Policy)
                    .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

                if (filter.CustomerId.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.Policy.Offer.CustomerId == filter.CustomerId.Value);

                if (filter.AgentId.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.Policy.Offer.AgentId == filter.AgentId.Value);

                if (!string.IsNullOrEmpty(filter.Status))
                    paymentsQuery = paymentsQuery.Where(p => p.Status.ToString() == filter.Status);

                var payments = await paymentsQuery.ToListAsync();

                var totalPayments = payments.Count;
                var successfulPayments = payments.Count(p => p.Status == PaymentStatus.Basarili);
                var failedPayments = payments.Count(p => p.Status == PaymentStatus.Basarisiz);
                var pendingPayments = payments.Count(p => p.Status == PaymentStatus.Beklemede);
                var totalAmount = payments.Sum(p => p.Amount);
                var successfulAmount = payments.Where(p => p.Status == PaymentStatus.Basarili).Sum(p => p.Amount);
                var failedAmount = payments.Where(p => p.Status == PaymentStatus.Basarisiz).Sum(p => p.Amount);

                // Payments by Method
                var paymentsByMethod = payments
                    .GroupBy(p => p.Method)
                    .Select(g => new PaymentsByMethodDto
                    {
                        PaymentMethod = g.Key.ToString(),
                        Count = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount),
                        Percentage = totalPayments > 0 ? (decimal)g.Count() / totalPayments * 100 : 0
                    })
                    .OrderByDescending(p => p.Count)
                    .ToList();

                // Payments by Month
                var paymentsByMonth = payments
                    .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                    .Select(g => new PaymentsByMonthDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                        PaymentsCount = g.Count(),
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .OrderBy(p => p.Year)
                    .ThenBy(p => p.Month)
                    .ToList();

                _logger.LogInformation("Payment report generated for period {StartDate} to {EndDate}", startDate, endDate);

                return new PaymentReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPayments = totalPayments,
                    SuccessfulPayments = successfulPayments,
                    FailedPayments = failedPayments,
                    PendingPayments = pendingPayments,
                    TotalAmount = totalAmount,
                    SuccessfulAmount = successfulAmount,
                    FailedAmount = failedAmount,
                    PaymentsByMethod = paymentsByMethod,
                    PaymentsByMonth = paymentsByMonth
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating payment report");
                throw;
            }
        }

        public async Task<byte[]> ExportReportToExcelAsync(object reportData, string reportType)
        {
            // Placeholder for Excel export functionality
            // In a real implementation, you would use a library like EPPlus or ClosedXML
            await Task.Delay(100); // Simulate async operation
            return new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // Placeholder Excel file header
        }

        public async Task<byte[]> ExportReportToPdfAsync(object reportData, string reportType)
        {
            // Placeholder for PDF export functionality
            // In a real implementation, you would use a library like iTextSharp or PdfSharp
            await Task.Delay(100); // Simulate async operation
            return new byte[] { 0x25, 0x50, 0x44, 0x46 }; // Placeholder PDF file header
        }

        public async Task<Dictionary<string, object>> GetDashboardSummaryAsync()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);

                var summary = new Dictionary<string, object>
                {
                    ["TotalCustomers"] = await _context.Customers.CountAsync(),
                    ["TotalPolicies"] = await _context.Policies.CountAsync(),
                    ["TotalClaims"] = await _context.Claims.CountAsync(),
                    ["TotalAgents"] = await _context.Agents.CountAsync(),
                    ["MonthlyPremium"] = await _context.Policies
                        .Where(p => p.CreatedAt >= thisMonth)
                        .SumAsync(p => p.TotalPremium),
                    ["MonthlyClaims"] = await _context.Claims
                        .Where(c => c.CreatedAt >= thisMonth)
                        .CountAsync(),
                    ["NewCustomersThisMonth"] = await _context.Customers
                        .Include(c => c.User)
                        .Where(c => c.User.CreatedAt >= thisMonth)
                        .CountAsync(),
                    ["PendingClaims"] = await _context.Claims
                        .Where(c => c.Status == ClaimStatus.Pending)
                        .CountAsync()
                };

                _logger.LogInformation("Dashboard summary generated successfully");

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating dashboard summary");
                throw;
            }
        }

        public async Task<List<object>> GetChartDataAsync(string chartType, ReportFilterDto filter)
        {
            try
            {
                var startDate = filter.StartDate ?? DateTime.UtcNow.AddMonths(-6);
                var endDate = filter.EndDate ?? DateTime.UtcNow;

                switch (chartType.ToLower())
                {
                    case "sales":
                        var salesData = await _context.Policies
                            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                            .Select(g => new
                            {
                                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                                Policies = g.Count(),
                                Premium = g.Sum(p => p.TotalPremium)
                            })
                            .OrderBy(x => x.Period)
                            .ToListAsync();
                        return salesData.Cast<object>().ToList();

                    case "claims":
                        var claimsData = await _context.Claims
                            .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                            .Select(g => new
                            {
                                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                                Claims = g.Count(),
                                Amount = g.Sum(c => c.ClaimAmount ?? 0)
                            })
                            .OrderBy(x => x.Period)
                            .ToListAsync();
                        return claimsData.Cast<object>().ToList();

                    default:
                        return new List<object>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating chart data for type {ChartType}", chartType);
                throw;
            }
        }

        private string GetRegionFromAddress(string address)
        {
            // Simplified region extraction from address
            if (string.IsNullOrEmpty(address))
                return "Unknown";

            var addressLower = address.ToLowerInvariant();
            if (addressLower.Contains("istanbul") || addressLower.Contains("ankara") || addressLower.Contains("izmir"))
                return "Metropolitan";
            else if (addressLower.Contains("antalya") || addressLower.Contains("bursa") || addressLower.Contains("adana"))
                return "Major Cities";
            else
                return "Other Regions";
        }
    }
}
