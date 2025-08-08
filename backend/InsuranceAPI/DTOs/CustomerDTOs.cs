namespace InsuranceAPI.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string IdNo { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
    
    public class CreateCustomerDto
    {
        public string Type { get; set; } = string.Empty;
        public string IdNo { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
    
    public class UpdateCustomerDto
    {
        public string Type { get; set; } = string.Empty;
        public string IdNo { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
} 