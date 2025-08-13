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
        private readonly JwtService _jwtService;
        
        // Auth service constructor - dependency injection
        public AuthService(InsuranceDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }
        
        // Kullanıcı giriş işlemi - email ve şifre doğrulama
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }
            
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
            var principal = _jwtService.ValidateToken(token);
            if (principal == null) return false;
            
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;
            
            return await _context.Users.AnyAsync(u => u.Id == int.Parse(userId));
        }
        
        public async Task<UserDto?> GetUserFromTokenAsync(string token)
        {
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