using InsuranceAPI.DTOs;

namespace InsuranceAPI.Services
{
    public interface ICustomerService
    {
        // Tüm müşterileri getir
        Task<List<CustomerDto>> GetAllCustomersAsync();
        
        // ID'ye göre müşteri getir
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        
        // Yeni müşteri oluştur
        Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        
        // Müşteri güncelle
        Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto);
        
        // Müşteri sil
        Task<bool> DeleteCustomerAsync(int id);
        
        // Müşteri arama
        Task<List<CustomerDto>> SearchCustomersAsync(string? name, string? type, string? idNo);
        
        // ID No'ya göre müşteri kontrol et
        Task<bool> IsIdNoExistsAsync(string idNo);
        
        // Departman bazlı müşterileri getir (Agent için)
        Task<List<CustomerDto>> GetCustomersByDepartmentAsync(string department);
        
        // Agent'ın departmanına göre müşterileri getir
        Task<List<CustomerDto>> GetCustomersByAgentDepartmentAsync(int agentId);
        
        // User ID'ye göre müşteri getir
        Task<CustomerDto?> GetCustomerByUserIdAsync(int userId);
        
        // Yeni gelişmiş metodlar
        Task<CustomerStatisticsDto> GetCustomerStatisticsAsync();
        Task<List<CustomerActivityDto>> GetCustomerActivityAsync(int customerId);
        Task<object> BulkUpdateCustomersAsync(List<BulkUpdateCustomerDto> updates);
        Task<string> ExportCustomersAsync(string? format);
        Task<object> ImportCustomersAsync(IFormFile file);
    }
}
