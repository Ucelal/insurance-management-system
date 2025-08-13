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
    
    public enum InsuranceType
    {
        Kasko,
        Trafik,
        Konut,
        Saglik,
        Hayat,
        Isyeri
    }
} 