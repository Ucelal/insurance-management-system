using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace InsuranceAPI.Attributes
{
    public class PhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            
            var phoneNumber = value.ToString();
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return ValidationResult.Success;
            }
            
            // Türkiye telefon numarası formatı: 5XX XXX XXXX veya 0XXX XXX XXXX
            var phonePattern = @"^(5\d{2}|0\d{3})\s?\d{3}\s?\d{4}$";
            
            if (!Regex.IsMatch(phoneNumber, phonePattern))
            {
                return new ValidationResult("Geçerli bir telefon numarası giriniz (örn: 555 123 4567 veya 0555 123 4567)");
            }
            
            return ValidationResult.Success;
        }
    }
}
