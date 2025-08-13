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
        
        // Teklif durumuna göre getir
        Task<List<OfferDto>> GetOffersByStatusAsync(string status);
        
        // Teklif arama
        Task<List<OfferDto>> SearchOffersAsync(string? insuranceType, string? status, decimal? minPrice, decimal? maxPrice);
    }
}
