using InsuranceAPI.DTOs;

namespace InsuranceAPI.Services
{
    public interface IOfferService
    {
        // Tüm teklifleri getir
        Task<List<OfferDto>> GetAllOffersAsync();
        
        // ID'ye göre teklif getir
        Task<OfferDto?> GetOfferByIdAsync(int id);
        
        // Yeni teklif oluştur
        Task<OfferDto?> CreateOfferAsync(CreateOfferDto createOfferDto);
        
        // Teklif güncelle
        Task<OfferDto?> UpdateOfferAsync(int id, UpdateOfferDto updateOfferDto);
        
        // Teklif sil
        Task<bool> DeleteOfferAsync(int id);
        
        // Müşteriye göre teklifleri getir
        Task<List<OfferDto>> GetOffersByCustomerAsync(int customerId);
        
        // Agent'a göre teklifleri getir
        Task<List<OfferDto>> GetOffersByAgentAsync(int agentId);
        
        // Teklif durumuna göre getir
        Task<List<OfferDto>> GetOffersByStatusAsync(string status);
        
        // Teklif arama
        Task<List<OfferDto>> SearchOffersAsync(string? insuranceType, string? status, decimal? minPrice, decimal? maxPrice);
        
        // Departman bazlı teklifleri getir (Agent için)
        Task<List<OfferDto>> GetOffersByDepartmentAsync(string department);
        
        // Agent'ın departmanına göre teklifleri getir
        Task<List<OfferDto>> GetOffersByAgentDepartmentAsync(int agentId);
        
        // Admin için tüm teklifleri getir (departman filtresi olmadan)
        Task<List<OfferDto>> GetAllOffersForAdminAsync();

        Task<List<OfferDto>> GetOffersByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<List<OfferDto>> GetOffersByCustomerAndStatusAsync(int customerId, string status);
        Task<List<OfferDto>> GetOffersByInsuranceTypeAsync(int insuranceTypeId);
        Task<List<OfferDto>> GetOffersByDateRangeAsync(DateTime startDate, DateTime endDate);
        
        // İndirim hesaplama metodları
        decimal CalculateFinalPrice(decimal basePrice, decimal discountRate, decimal coverageIncreaseRate = 0);
        Task<bool> UpdateOfferWithDiscountAsync(int offerId, decimal? discountRate, decimal? finalPrice = null);
    }
}
