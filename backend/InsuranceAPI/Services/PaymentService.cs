using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;

namespace InsuranceAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly InsuranceDbContext _context;
        private readonly Random _random = new Random();

        public PaymentService(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _context.Payments
                .Include(p => p.Policy)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Policy)
                .FirstOrDefaultAsync(p => p.Id == id);

            return payment != null ? MapToDto(payment) : null;
        }

        public async Task<List<PaymentDto>> GetPaymentsByPolicyAsync(int policyId)
        {
            var payments = await _context.Payments
                .Include(p => p.Policy)
                .Where(p => p.PolicyId == policyId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        public async Task<List<PaymentDto>> GetPaymentsByStatusAsync(string status)
        {
            if (Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            {
                var payments = await _context.Payments
                    .Include(p => p.Policy)
                    .Where(p => p.Status == paymentStatus)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                return payments.Select(MapToDto).ToList();
            }

            return new List<PaymentDto>();
        }

        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createDto)
        {
            // Poliçe var mı kontrol et
            var policy = await _context.Policies.FindAsync(createDto.PolicyId);
            if (policy == null)
                throw new ArgumentException("Poliçe bulunamadı");

            // Ödeme yöntemini parse et
            if (!Enum.TryParse<PaymentMethod>(createDto.Method, true, out var paymentMethod))
                throw new ArgumentException("Geçersiz ödeme yöntemi");

            var payment = new Payment
            {
                PolicyId = createDto.PolicyId,
                Amount = createDto.Amount,
                Method = paymentMethod,
                Status = PaymentStatus.Beklemede,
                Notes = createDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Poliçe ile birlikte getir
            await _context.Entry(payment).Reference(p => p.Policy).LoadAsync();
            return MapToDto(payment);
        }

        public async Task<PaymentDto> UpdatePaymentAsync(int id, UpdatePaymentDto updateDto)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                throw new ArgumentException("Ödeme bulunamadı");

            if (!string.IsNullOrEmpty(updateDto.Status))
            {
                if (Enum.TryParse<PaymentStatus>(updateDto.Status, true, out var status))
                    payment.Status = status;
                else
                    throw new ArgumentException("Geçersiz ödeme durumu");
            }

            payment.TransactionId = updateDto.TransactionId ?? payment.TransactionId;
            payment.Notes = updateDto.Notes ?? payment.Notes;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Poliçe ile birlikte getir
            await _context.Entry(payment).Reference(p => p.Policy).LoadAsync();
            return MapToDto(payment);
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaymentDto> ProcessPaymentAsync(int id, ProcessPaymentDto processDto)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                throw new ArgumentException("Ödeme bulunamadı");

            if (Enum.TryParse<PaymentStatus>(processDto.Status, true, out var status))
                payment.Status = status;
            else
                throw new ArgumentException("Geçersiz ödeme durumu");

            payment.TransactionId = processDto.TransactionId ?? payment.TransactionId;
            payment.Notes = processDto.Notes ?? payment.Notes;
            payment.UpdatedAt = DateTime.UtcNow;

            // Eğer ödeme başarılıysa, ödeme tarihini güncelle
            if (status == PaymentStatus.Basarili)
                payment.PaidAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Poliçe ile birlikte getir
            await _context.Entry(payment).Reference(p => p.Policy).LoadAsync();
            return MapToDto(payment);
        }

        public async Task<PaymentStatisticsDto> GetPaymentStatisticsAsync()
        {
            var payments = await _context.Payments.ToListAsync();

            var statistics = new PaymentStatisticsDto
            {
                TotalPayments = payments.Count,
                SuccessfulPayments = payments.Count(p => p.Status == PaymentStatus.Basarili),
                PendingPayments = payments.Count(p => p.Status == PaymentStatus.Beklemede),
                FailedPayments = payments.Count(p => p.Status == PaymentStatus.Basarisiz),
                TotalAmount = payments.Sum(p => p.Amount),
                SuccessfulAmount = payments.Where(p => p.Status == PaymentStatus.Basarili).Sum(p => p.Amount)
            };

            // Ödeme yöntemine göre grupla
            statistics.PaymentsByMethod = payments
                .GroupBy(p => p.Method.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Ödeme durumuna göre grupla
            statistics.PaymentsByStatus = payments
                .GroupBy(p => p.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // Aylık gelir
            statistics.RevenueByMonth = payments
                .Where(p => p.Status == PaymentStatus.Basarili)
                .GroupBy(p => p.PaidAt.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            return statistics;
        }

        public async Task<List<PaymentDto>> SearchPaymentsAsync(PaymentSearchDto searchDto)
        {
            var query = _context.Payments.Include(p => p.Policy).AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                if (Enum.TryParse<PaymentStatus>(searchDto.Status, true, out var status))
                    query = query.Where(p => p.Status == status);
            }

            if (!string.IsNullOrEmpty(searchDto.Method))
            {
                if (Enum.TryParse<PaymentMethod>(searchDto.Method, true, out var method))
                    query = query.Where(p => p.Method == method);
            }

            if (searchDto.PolicyId.HasValue)
                query = query.Where(p => p.PolicyId == searchDto.PolicyId.Value);

            if (searchDto.StartDate.HasValue)
                query = query.Where(p => p.CreatedAt >= searchDto.StartDate.Value);

            if (searchDto.EndDate.HasValue)
                query = query.Where(p => p.CreatedAt <= searchDto.EndDate.Value);

            if (searchDto.MinAmount.HasValue)
                query = query.Where(p => p.Amount >= searchDto.MinAmount.Value);

            if (searchDto.MaxAmount.HasValue)
                query = query.Where(p => p.Amount <= searchDto.MaxAmount.Value);

            var payments = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return payments.Select(MapToDto).ToList();
        }

        public async Task<PaymentDto> SimulatePaymentAsync(CreatePaymentDto createDto)
        {
            // Ödeme simülasyonu - gerçek banka entegrasyonu yok
            var payment = await CreatePaymentAsync(createDto);

            // Simüle edilmiş işlem ID'si
            payment.TransactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{_random.Next(1000, 9999)}";

            // %90 başarı oranı ile simüle et
            var isSuccessful = _random.Next(1, 101) <= 90;
            
            if (isSuccessful)
            {
                payment.Status = "Basarili";
                payment.PaidAt = DateTime.UtcNow;
            }
            else
            {
                payment.Status = "Basarisiz";
            }

            // Simüle edilmiş ödemeyi güncelle
            var updateDto = new UpdatePaymentDto
            {
                Status = payment.Status,
                TransactionId = payment.TransactionId,
                Notes = "Simüle edilmiş ödeme"
            };

            return await UpdatePaymentAsync(payment.Id, updateDto);
        }

        private static PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                PolicyId = payment.PolicyId,
                PolicyNumber = payment.Policy?.PolicyNumber ?? string.Empty,
                Amount = payment.Amount,
                PaidAt = payment.PaidAt,
                Method = payment.Method.ToString(),
                Status = payment.Status.ToString(),
                TransactionId = payment.TransactionId,
                Notes = payment.Notes,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt,
                Policy = payment.Policy != null ? new PolicyDto
                {
                    Id = payment.Policy.Id,
                    PolicyNumber = payment.Policy.PolicyNumber,
                    StartDate = payment.Policy.StartDate,
                    EndDate = payment.Policy.EndDate,
                    Status = payment.Policy.Status.ToString(),
                    TotalPremium = payment.Policy.TotalPremium
                } : null
            };
        }
    }
}
