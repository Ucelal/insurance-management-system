using InsuranceAPI.DTOs;

namespace InsuranceAPI.Services
{
    public interface IPolicyService
    {
        // Tüm poliçeleri getir
        Task<List<PolicyDto>> GetAllPoliciesAsync();
        
        // ID'ye göre poliçe getir
        Task<PolicyDto?> GetPolicyByIdAsync(int id);
        
        // Yeni poliçe oluştur
        Task<PolicyDto?> CreatePolicyAsync(CreatePolicyDto createPolicyDto);
        
        // Poliçe güncelle
        Task<PolicyDto?> UpdatePolicyAsync(int id, UpdatePolicyDto updatePolicyDto);
        
        // Poliçe sil
        Task<bool> DeletePolicyAsync(int id);
        
        // Teklif ID'sine göre poliçe getir
        Task<PolicyDto?> GetPolicyByOfferAsync(int offerId);
        
        // Poliçe numarasına göre getir
        Task<PolicyDto?> GetPolicyByNumberAsync(string policyNumber);
        
        // Poliçe arama
        Task<List<PolicyDto>> SearchPoliciesAsync(string? policyNumber, DateTime? startDate, DateTime? endDate);
    }
}
