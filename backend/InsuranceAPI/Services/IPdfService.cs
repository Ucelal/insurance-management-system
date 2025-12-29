using InsuranceAPI.DTOs;

namespace InsuranceAPI.Services
{
    public interface IPdfService
    {
        // Poliçe dokümanını PDF olarak oluştur
        Task<byte[]> CreatePolicyPdfAsync(PolicyDto policy);
        
        // Ödeme makbuzunu PDF olarak oluştur ve URL döndür
        Task<string> CreatePaymentReceiptPdfAsync(object receipt);
        
        // PDF dosyasını kaydet ve URL döndür
        Task<string> SavePdfAsync(byte[] pdfBytes, string fileName, string category = "Policy", string description = null, int? customerId = null, int? userId = null);
    }
}



