using InsuranceAPI.DTOs;

namespace InsuranceAPI.Services
{
    public interface IPaymentService
    {
        Task<List<PaymentDto>> GetAllPaymentsAsync();
        Task<PaymentDto?> GetPaymentByIdAsync(int id);
        Task<List<PaymentDto>> GetPaymentsByPolicyAsync(int policyId);
        Task<List<PaymentDto>> GetPaymentsByStatusAsync(string status);
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createDto);
        Task<PaymentDto> UpdatePaymentAsync(int id, UpdatePaymentDto updateDto);
        Task<bool> DeletePaymentAsync(int id);
        Task<PaymentDto> ProcessPaymentAsync(int id, ProcessPaymentDto processDto);
        Task<PaymentStatisticsDto> GetPaymentStatisticsAsync();
        Task<List<PaymentDto>> SearchPaymentsAsync(PaymentSearchDto searchDto);
        Task<PaymentDto> SimulatePaymentAsync(CreatePaymentDto createDto);
    }
}
