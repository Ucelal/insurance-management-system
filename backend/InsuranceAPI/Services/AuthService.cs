using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;
using System.Security.Claims;

namespace InsuranceAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly InsuranceDbContext _context;
        private readonly JwtService _jwtService; // JWT bağımlılığı geri eklendi
        
        // Auth service constructor - dependency injection
        public AuthService(InsuranceDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService; // JWT bağımlılığı geri eklendi
        }
        
        // Kullanıcı giriş işlemi - email ve şifre doğrulama
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                
            // Güvenlik kontrolü: PasswordHash null veya boş olmamalı
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                return null;
            }
            
            // BCrypt ile şifre doğrulama
            try
            {
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return null;
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Invalid salt"))
            {
                // Hash formatı geçersiz - kullanıcı giriş yapamaz
                return null;
            }
            
            // JWT token generation geri eklendi
            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    Customer = user.Customer != null ? new CustomerDto
                    {
                        Id = user.Customer.Id,
                        UserId = user.Customer.UserId,
                        Type = user.Customer.Type,
                        IdNo = user.Customer.IdNo,
                        Address = user.Customer.Address,
                        Phone = user.Customer.Phone
                    } : null
                }
            };
        }
        
        // Kullanıcı kayıt işlemi - yeni kullanıcı oluşturma
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return null;
            }
            
            // Create new user
            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Role = registerDto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Eğer kullanıcı customer rolü ile kayıt oluyorsa, Customers tablosuna da ekle
            if (registerDto.Role.ToLower() == "customer")
            {
                var customer = new Customer
                {
                    UserId = user.Id,
                    Type = "bireysel", // Varsayılan olarak bireysel
                    IdNo = $"CUST_{user.Id}_{DateTime.UtcNow:yyyyMMdd}", // Otomatik ID No oluştur
                    Address = "",
                    Phone = ""
                };
                
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }
            
            // Generate tokens
            // JWT token generation geri eklendi
            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        
        // Customer kayıt işlemi - özel validasyon ve iş mantığı
        public async Task<AuthResponseDto?> RegisterCustomerAsync(CustomerRegisterDto customerRegisterDto)
        {
            // Email kontrolü
            if (await _context.Users.AnyAsync(u => u.Email == customerRegisterDto.Email))
            {
                return null;
            }
            
            // TC No kontrolü
            if (await _context.Customers.AnyAsync(c => c.IdNo == customerRegisterDto.TcNo))
            {
                return null;
            }
            
            // Create new user with customer role
            var user = new User
            {
                Name = customerRegisterDto.Name,
                Email = customerRegisterDto.Email,
                Role = "customer",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(customerRegisterDto.Password),
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Create customer record
            var customer = new Customer
            {
                UserId = user.Id,
                Type = customerRegisterDto.CustomerType,
                IdNo = customerRegisterDto.TcNo,
                Address = customerRegisterDto.Address,
                Phone = customerRegisterDto.Phone
            };
            
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            
            // Generate tokens
            // JWT token generation geri eklendi
            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        
        // Agent kayıt işlemi - özel validasyon ve iş mantığı
        public async Task<AuthResponseDto?> RegisterAgentAsync(AgentRegisterDto agentRegisterDto)
        {
            // Email kontrolü
            if (await _context.Users.AnyAsync(u => u.Email == agentRegisterDto.Email))
            {
                return null;
            }
            
            // Agent Code kontrolü
            if (await _context.Agents.AnyAsync(a => a.AgentCode == agentRegisterDto.AgentCode))
            {
                return null;
            }
            
            // Create new user with agent role
            var user = new User
            {
                Name = agentRegisterDto.Name,
                Email = agentRegisterDto.Email,
                Role = "agent",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(agentRegisterDto.Password),
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            // Create agent record
            var agent = new Agent
            {
                UserId = user.Id,
                AgentCode = agentRegisterDto.AgentCode,
                Department = agentRegisterDto.Department,
                Address = agentRegisterDto.Address,
                Phone = agentRegisterDto.Phone
            };
            
            _context.Agents.Add(agent);
            await _context.SaveChangesAsync();
            
            // Generate tokens
            // JWT token generation geri eklendi
            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        
        public async Task<bool> ValidateTokenAsync(string token)
        {
            // JWT token validation geri eklendi
            var principal = _jwtService.ValidateToken(token);
            if (principal == null) return false;
            
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;
            
            return await _context.Users.AnyAsync(u => u.Id == int.Parse(userId));
        }
        
        public async Task<UserDto?> GetUserFromTokenAsync(string token)
        {
            // JWT token validation geri eklendi
            var principal = _jwtService.ValidateToken(token);
            if (principal == null) return null;
            
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;
            
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
                
            if (user == null) return null;
            
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                Customer = user.Customer != null ? new CustomerDto
                {
                    Id = user.Customer.Id,
                    UserId = user.Customer.UserId,
                    Type = user.Customer.Type,
                    IdNo = user.Customer.IdNo,
                    Address = user.Customer.Address,
                    Phone = user.Customer.Phone
                } : null
            };
        }
        
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
} 