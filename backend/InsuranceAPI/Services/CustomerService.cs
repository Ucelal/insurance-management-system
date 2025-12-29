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
                .OrderBy(c => c.CustomerId)
                .ToListAsync();
                
            return customers.Select(MapToDto).ToList();
        }
        
        // ID'ye göre müşteri getir
        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
                
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
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);
                
            return createdCustomer != null ? MapToDto(createdCustomer) : null;
        }
        
        // Müşteri güncelle
        public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
            
            if (customer == null)
            {
                return null;
            }
            
            // Update User fields if provided
            if (customer.User != null)
            {
                if (!string.IsNullOrEmpty(updateCustomerDto.Name))
                {
                    customer.User.Name = updateCustomerDto.Name;
                    Console.WriteLine($"Updated customer user name to '{updateCustomerDto.Name}'");
                }
                
                if (!string.IsNullOrEmpty(updateCustomerDto.Email))
                {
                    // Only check if email is unique if it's actually changing
                    if (customer.User.Email != updateCustomerDto.Email)
                    {
                        var emailExists = await _context.Users
                            .AnyAsync(u => u.Email == updateCustomerDto.Email && u.UserId != customer.UserId);
                        if (emailExists)
                        {
                            throw new InvalidOperationException($"Email '{updateCustomerDto.Email}' is already in use.");
                        }
                        customer.User.Email = updateCustomerDto.Email;
                        Console.WriteLine($"Updated customer user email to '{updateCustomerDto.Email}'");
                    }
                }
                
                if (!string.IsNullOrEmpty(updateCustomerDto.Password))
                {
                    customer.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateCustomerDto.Password);
                    Console.WriteLine("Updated customer user password");
                }
            }
            
            // Update Customer fields if provided
            if (!string.IsNullOrEmpty(updateCustomerDto.IdNo))
            {
            // ID No değişmişse kontrol et
            if (customer.IdNo != updateCustomerDto.IdNo && await IsIdNoExistsAsync(updateCustomerDto.IdNo))
            {
                    throw new InvalidOperationException($"ID No '{updateCustomerDto.IdNo}' is already in use.");
                }
                customer.IdNo = updateCustomerDto.IdNo;
            }
            
            if (!string.IsNullOrEmpty(updateCustomerDto.Address))
            customer.Address = updateCustomerDto.Address;
                
            if (!string.IsNullOrEmpty(updateCustomerDto.Phone))
            customer.Phone = updateCustomerDto.Phone;
            
            await _context.SaveChangesAsync();
            
            // Güncellenmiş müşteriyi User bilgisiyle birlikte getir
            var updatedCustomer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
                
            return updatedCustomer != null ? MapToDto(updatedCustomer) : null;
        }
        
        // Müşteri sil
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
            
            if (customer == null)
            {
                return false;
            }
            
            // Store the user ID before deleting the customer
            var userId = customer.UserId;
            
            // Delete the customer first
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            
            // Delete the associated user if exists
            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
            
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
            
            if (!string.IsNullOrEmpty(idNo))
            {
                query = query.Where(c => c.IdNo.Contains(idNo));
            }
            
            var customers = await query.OrderBy(c => c.CustomerId).ToListAsync();
            
            return customers.Select(MapToDto).ToList();
        }
        
        // ID No'ya göre müşteri kontrol et
        public async Task<bool> IsIdNoExistsAsync(string idNo)
        {
            return await _context.Customers.AnyAsync(c => c.IdNo == idNo);
        }
        
        // Departman bazlı müşterileri getir (Agent için)
        public async Task<List<CustomerDto>> GetCustomersByDepartmentAsync(string department)
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Offers)
                    .ThenInclude(o => o.Agent)
                .Where(c => c.Offers != null && c.Offers.Any(o => o.Agent != null && o.Agent.Department == department))
                .OrderBy(c => c.CustomerId)
                .ToListAsync();
                
            return customers.Select(MapToDto).ToList();
        }
        
        // Agent'ın departmanına göre müşterileri getir
        public async Task<List<CustomerDto>> GetCustomersByAgentDepartmentAsync(int agentId)
        {
            try
            {
                Console.WriteLine($"CustomerService.GetCustomersByAgentDepartmentAsync - AgentId: {agentId}");
                
                var agent = await _context.Agents.FindAsync(agentId);
                if (agent == null)
                {
                    Console.WriteLine($"Agent bulunamadı: {agentId}");
                    return new List<CustomerDto>();
                }
                
                Console.WriteLine($"Agent bulundu: {agent.User?.Name ?? "İsimsiz"}, Departman: {agent.Department}");
                
                var customers = await _context.Customers
                    .Include(c => c.User)
                    .Include(c => c.Offers)
                        .ThenInclude(o => o.Agent)
                            .ThenInclude(a => a.User)
                    .Where(c => c.Offers != null && c.Offers.Any(o => o.Agent != null && o.Agent.Department == agent.Department))
                    .OrderBy(c => c.CustomerId)
                    .ToListAsync();
                
                Console.WriteLine($"Departman '{agent.Department}' için {customers.Count} müşteri bulundu");
                
                var result = customers.Select(MapToDto).ToList();
                Console.WriteLine($"DTO'ya dönüştürüldü: {result.Count} müşteri");
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomerService.GetCustomersByAgentDepartmentAsync - Hata: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
        
        // User ID'ye göre müşteri getir
        public async Task<CustomerDto?> GetCustomerByUserIdAsync(int userId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);
                
            return customer != null ? MapToDto(customer) : null;
        }
        
        // Customer entity'sini CustomerDto'ya dönüştür
        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                UserId = customer.UserId,
                IdNo = customer.IdNo,
                Address = customer.Address,
                Phone = customer.Phone,
                User = customer.User != null ? new UserDto
                {
                    Id = customer.User.UserId,
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
                
            var customersByMonth = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.User != null)
                .GroupBy(c => new { Month = c.User.CreatedAt.Month, Year = c.User.CreatedAt.Year })
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
                ActiveCustomers = totalCustomers, // Basit implementasyon
                InactiveCustomers = 0,
                CustomersByMonth = customersByMonthDict
            };
        }
        
        
        // Müşteri aktivite geçmişi (basit implementasyon)
        public async Task<List<CustomerActivityDto>> GetCustomerActivityAsync(int customerId)
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
                
            if (customer == null)
            {
                return new List<CustomerActivityDto>();
            }
            
            var activities = new List<CustomerActivityDto>
            {
                new CustomerActivityDto
                {
                    CustomerId = 1,
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
                var customer = await _context.Customers.FindAsync(update.CustomerId);
                if (customer != null)
                {
                    if (!string.IsNullOrEmpty(update.Phone)) customer.Phone = update.Phone;
                    if (!string.IsNullOrEmpty(update.Address)) customer.Address = update.Address;
                    
                    results.Add(new { Id = update.CustomerId, Status = "Updated", Customer = MapToDto(customer) });
                }
                else
                {
                    results.Add(new { Id = update.CustomerId, Status = "Not Found" });
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
                var csv = "Id,UserId,Name,Email,IdNo,Address,Phone,CreatedAt\n";
                
                foreach (var customer in customers)
                {
                    csv += $"{customer.CustomerId},{customer.UserId}," +
                           $"\"{customer.User?.Name?.Replace("\"", "\"\"")}\"," +
                           $"\"{customer.User?.Email}\"," +
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
                    if (values.Length >= 3)
                    {
                        var customer = new Customer
                        {
                            IdNo = values[0].Trim('"'),
                            Address = values[1].Trim('"'),
                            Phone = values[2].Trim('"')
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
