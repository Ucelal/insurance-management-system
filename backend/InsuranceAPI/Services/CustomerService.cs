using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;
using System.Text.Json;

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
        
        // Müşteri istatistikleri
        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync()
        {
            var totalCustomers = await _context.Customers.CountAsync();
            var individualCustomers = await _context.Customers.CountAsync(c => c.Type == "bireysel");
            var corporateCustomers = await _context.Customers.CountAsync(c => c.Type == "kurumsal");
            
            var customersByType = await _context.Customers
                .GroupBy(c => c.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);
                
            var customersByMonth = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.User != null)
                .GroupBy(c => new { Month = c.User!.CreatedAt.Month, Year = c.User!.CreatedAt.Year })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();
            
            // Client-side'da string formatlamayı yap
            var customersByMonthDict = customersByMonth.ToDictionary(
                x => $"{x.Year}-{x.Month:00}", 
                x => x.Count
            );
            
            return new CustomerStatisticsDto
            {
                TotalCustomers = totalCustomers,
                IndividualCustomers = individualCustomers,
                CorporateCustomers = corporateCustomers,
                ActiveCustomers = totalCustomers, // Basit implementasyon
                InactiveCustomers = 0,
                CustomersByType = customersByType,
                CustomersByMonth = customersByMonthDict
            };
        }
        
        // Müşterileri gruplandır
        public async Task<object> GetCustomersGroupedAsync()
        {
            var grouped = await _context.Customers
                .Include(c => c.User)
                .GroupBy(c => c.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Customers = g.Select(c => MapToDto(c)).ToList()
                })
                .ToListAsync();
                
            return grouped;
        }
        
        // Müşteri aktivite geçmişi (basit implementasyon)
        public async Task<List<CustomerActivityDto>> GetCustomerActivityAsync(int customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == customerId);
                
            if (customer == null)
            {
                return new List<CustomerActivityDto>();
            }
            
            var activities = new List<CustomerActivityDto>
            {
                new CustomerActivityDto
                {
                    Id = 1,
                    Action = "Kayıt",
                    Description = $"Müşteri {customer.User?.Name} sisteme kayıt oldu",
                    Timestamp = customer.User?.CreatedAt ?? DateTime.UtcNow,
                    UserName = customer.User?.Name
                }
            };
            
            return activities;
        }
        
        // Toplu müşteri güncelleme
        public async Task<object> BulkUpdateCustomersAsync(List<BulkUpdateCustomerDto> updates)
        {
            var results = new List<object>();
            
            foreach (var update in updates)
            {
                var customer = await _context.Customers.FindAsync(update.Id);
                if (customer != null)
                {
                    if (!string.IsNullOrEmpty(update.Type)) customer.Type = update.Type;
                    if (!string.IsNullOrEmpty(update.Address)) customer.Address = update.Address;
                    if (!string.IsNullOrEmpty(update.Phone)) customer.Phone = update.Phone;
                    
                    results.Add(new { Id = update.Id, Status = "Updated", Customer = MapToDto(customer) });
                }
                else
                {
                    results.Add(new { Id = update.Id, Status = "Not Found" });
                }
            }
            
            await _context.SaveChangesAsync();
            return new { UpdatedCount = results.Count(r => r.GetType().GetProperty("Status")?.GetValue(r)?.ToString() == "Updated"), Results = results };
        }
        
        // Müşteri export
        public async Task<string> ExportCustomersAsync(string? format)
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .ToListAsync();
                
            if (format?.ToLower() == "csv")
            {
                var csv = "Id,UserId,Name,Email,Type,IdNo,Address,Phone,CreatedAt\n";
                
                foreach (var customer in customers)
                {
                    csv += $"{customer.Id},{customer.UserId}," +
                           $"\"{customer.User?.Name?.Replace("\"", "\"\"")}\"," +
                           $"\"{customer.User?.Email}\"," +
                           $"\"{customer.Type}\"," +
                           $"\"{customer.IdNo}\"," +
                           $"\"{customer.Address}\"," +
                           $"\"{customer.Phone}\"," +
                           $"{customer.User?.CreatedAt:yyyy-MM-dd}\n";
                }
                
                return csv;
            }
            
            return System.Text.Json.JsonSerializer.Serialize(customers.Select(MapToDto));
        }
        
        // Müşteri import
        public async Task<object> ImportCustomersAsync(IFormFile file)
        {
            var results = new List<object>();
            var successCount = 0;
            var errorCount = 0;
            
            using var reader = new StreamReader(file.OpenReadStream());
            var csv = await reader.ReadToEndAsync();
            var lines = csv.Split('\n').Skip(1); // Header'ı atla
            
            foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                try
                {
                    var values = line.Split(',');
                    if (values.Length >= 4)
                    {
                        var customer = new Customer
                        {
                            Type = values[0].Trim('"'),
                            IdNo = values[1].Trim('"'),
                            Address = values[2].Trim('"'),
                            Phone = values[3].Trim('"')
                        };
                        
                        _context.Customers.Add(customer);
                        successCount++;
                        results.Add(new { Line = line, Status = "Success" });
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    results.Add(new { Line = line, Status = "Error", Message = ex.Message });
                }
            }
            
            await _context.SaveChangesAsync();
            
            return new
            {
                TotalProcessed = results.Count,
                SuccessCount = successCount,
                ErrorCount = errorCount,
                Results = results
            };
        }
    }
}
