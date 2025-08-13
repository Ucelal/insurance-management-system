using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;

namespace InsuranceAPI.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly InsuranceDbContext _context;
        
        // Customer service constructor - dependency injection
        public CustomerService(InsuranceDbContext context)
        {
            _context = context;
        }
        
        // Tüm müşterileri getir
        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .OrderBy(c => c.Id)
                .ToListAsync();
                
            return customers.Select(MapToDto).ToList();
        }
        
        // ID'ye göre müşteri getir
        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            return customer != null ? MapToDto(customer) : null;
        }
        
        // Yeni müşteri oluştur
        public async Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            // ID No kontrolü
            var idNoExists = await IsIdNoExistsAsync(createCustomerDto.IdNo);
            if (idNoExists)
            {
                Console.WriteLine($"ID No {createCustomerDto.IdNo} zaten mevcut!");
                return null;
            }
            
            var customer = new Customer
            {
                Type = createCustomerDto.Type,
                IdNo = createCustomerDto.IdNo,
                Address = createCustomerDto.Address,
                Phone = createCustomerDto.Phone,
                UserId = createCustomerDto.UserId
            };
            
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            
            // Oluşturulan müşteriyi User bilgisiyle birlikte getir
            var createdCustomer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == customer.Id);
                
            return createdCustomer != null ? MapToDto(createdCustomer) : null;
        }
        
        // Müşteri güncelle
        public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto)
        {
            var customer = await _context.Customers.FindAsync(id);
            
            if (customer == null)
            {
                return null;
            }
            
            // ID No değişmişse kontrol et
            if (customer.IdNo != updateCustomerDto.IdNo && await IsIdNoExistsAsync(updateCustomerDto.IdNo))
            {
                return null;
            }
            
            customer.Type = updateCustomerDto.Type;
            customer.IdNo = updateCustomerDto.IdNo;
            customer.Address = updateCustomerDto.Address;
            customer.Phone = updateCustomerDto.Phone;
            customer.UserId = updateCustomerDto.UserId;
            
            await _context.SaveChangesAsync();
            
            // Güncellenmiş müşteriyi User bilgisiyle birlikte getir
            var updatedCustomer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            return updatedCustomer != null ? MapToDto(updatedCustomer) : null;
        }
        
        // Müşteri sil
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            
            if (customer == null)
            {
                return false;
            }
            
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        // Müşteri arama
        public async Task<List<CustomerDto>> SearchCustomersAsync(string? name, string? type, string? idNo)
        {
            var query = _context.Customers.Include(c => c.User).AsQueryable();
            
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.User != null && c.User.Name.Contains(name));
            }
            
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(c => c.Type == type);
            }
            
            if (!string.IsNullOrEmpty(idNo))
            {
                query = query.Where(c => c.IdNo.Contains(idNo));
            }
            
            var customers = await query.OrderBy(c => c.Id).ToListAsync();
            
            return customers.Select(MapToDto).ToList();
        }
        
        // ID No'ya göre müşteri kontrol et
        public async Task<bool> IsIdNoExistsAsync(string idNo)
        {
            return await _context.Customers.AnyAsync(c => c.IdNo == idNo);
        }
        
        // Customer entity'sini CustomerDto'ya dönüştür
        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                UserId = customer.UserId,
                Type = customer.Type,
                IdNo = customer.IdNo,
                Address = customer.Address,
                Phone = customer.Phone,
                User = customer.User != null ? new UserDto
                {
                    Id = customer.User.Id,
                    Name = customer.User.Name,
                    Email = customer.User.Email,
                    Role = customer.User.Role,
                    CreatedAt = customer.User.CreatedAt
                } : null
            };
        }
    }
}
