namespace InsuranceAPI.Models
{
    public enum UserRole
    {
        Admin,
        Agent,
        Customer
    }
    
    public enum CustomerType
    {
        Bireysel,
        Kurumsal
    }
    
    public enum OfferStatus
    {
        Pending,
        Approved,
        Cancelled
    }
    
    public enum InsuranceCategory
    {
        Kasko,
        Trafik,
        Konut,
        Saglik,
        Hayat,
        Isyeri
    }
    
    public enum ClaimStatus
    {
        Pending,        // Beklemede
        InReview,       // İncelemede
        Approved,       // Onaylandı
        Rejected,       // Reddedildi
        Resolved,       // Çözüldü
        Closed          // Kapatıldı
    }
    
    public enum ClaimType
    {
        ArabaKazasi,    // Araba kazası
        Hirsizlik,      // Hırsızlık
        Yangin,         // Yangın
        SuBaskini,      // Su baskını
        Deprem,         // Deprem
        Saglik,         // Sağlık
        Diger           // Diğer
    }
    
    public enum ClaimPriority
    {
        Dusuk,          // Düşük
        Normal,         // Normal
        Yuksek,         // Yüksek
        Acil            // Acil
    }
    
    public enum PaymentMethod
    {
        Nakit,          // Nakit
        KrediKarti,     // Kredi kartı
        Havale,         // Havale/EFT
        Cek,            // Çek
        Senet           // Senet
    }
    
    public enum PaymentStatus
    {
        Beklemede,      // Beklemede
        Islemde,        // İşlemde
        Basarili,       // Başarılı
        Basarisiz,      // Başarısız
        Iptal,          // İptal
        Iade            // İade
    }
    
    public enum DocumentCategory
    {
        Kimlik,         // Kimlik belgesi
        Adres,          // Adres belgesi
        Gelir,          // Gelir belgesi
        Saglik,         // Sağlık raporu
        Hasar,          // Hasar belgesi
        Polis,          // Polis raporu
        Fatura,         // Fatura
        Sozlesme,       // Sözleşme
        Diger           // Diğer
    }
    
    public enum DocumentStatus
    {
        Aktif,          // Aktif
        Guncelleniyor,  // Güncelleniyor
        Arşivlendi,     // Arşivlendi
        Silindi         // Silindi
    }
} 