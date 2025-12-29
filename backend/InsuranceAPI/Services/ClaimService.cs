using Microsoft.EntityFrameworkCore;
using InsuranceAPI.Data;
using InsuranceAPI.DTOs;
using InsuranceAPI.Models;

namespace InsuranceAPI.Services
{
    public interface IClaimService
    {
        Task<List<ClaimDto>> GetAllClaimsAsync();
        Task<ClaimDto?> GetClaimByIdAsync(int id);
        Task<List<ClaimDto>> GetClaimsByPolicyAsync(int policyId);
        Task<List<ClaimDto>> GetClaimsByUserAsync(int userId);
        Task<ClaimDto> CreateClaimAsync(CreateClaimDto createDto, int createdByUserId);
        Task<ClaimDto> UpdateClaimAsync(int id, UpdateClaimDto updateDto, int processedByUserId);
        Task<ClaimDto> UpdateMyClaimAsync(int id, UpdateClaimDto updateDto, int userId);
        Task<bool> DeleteClaimAsync(int id);
        Task<ClaimStatisticsDto> GetClaimStatisticsAsync();
        Task<List<ClaimDto>> SearchClaimsAsync(ClaimSearchDto searchDto);
        Task<ClaimDto> ProcessClaimAsync(int id, string status, string notes, int processedByUserId);
    }
    
    public class ClaimService : IClaimService
    {
        private readonly InsuranceDbContext _context;
        
        public ClaimService(InsuranceDbContext context)
        {
            _context = context;
        }
        
        // Tüm hasarları getir
        public async Task<List<ClaimDto>> GetAllClaimsAsync()
        {
            var claims = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Agent)
                .Include(c => c.ProcessedByUser)
                    .ThenInclude(u => u.Agent)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
                
            return claims.Select(MapToDto).ToList();
        }
        
        // ID'ye göre hasar getir
        public async Task<ClaimDto?> GetClaimByIdAsync(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Agent)
                .Include(c => c.ProcessedByUser)
                    .ThenInclude(u => u.Agent)
                .FirstOrDefaultAsync(c => c.ClaimId == id);
                
            return claim != null ? MapToDto(claim) : null;
        }
        
        // Poliçeye göre hasarları getir
        public async Task<List<ClaimDto>> GetClaimsByPolicyAsync(int policyId)
        {
            var claims = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Agent)
                .Include(c => c.ProcessedByUser)
                    .ThenInclude(u => u.Agent)
                .Where(c => c.PolicyId == policyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
                
            return claims.Select(MapToDto).ToList();
        }
        
        // Kullanıcıya göre hasarları getir
        public async Task<List<ClaimDto>> GetClaimsByUserAsync(int userId)
        {
            var claims = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Agent)
                .Include(c => c.ProcessedByUser)
                    .ThenInclude(u => u.Agent)
                .Where(c => c.CreatedByUserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
                
            return claims.Select(MapToDto).ToList();
        }
        
        // Yeni hasar oluştur
        public async Task<ClaimDto> CreateClaimAsync(CreateClaimDto createDto, int createdByUserId)
        {
            // Poliçe kontrolü
            var policy = await _context.Policies.FindAsync(createDto.PolicyId);
            if (policy == null)
            {
                throw new ArgumentException("Geçersiz poliçe ID");
            }
            
            var claim = new Claim
            {
                PolicyId = createDto.PolicyId,
                CreatedByUserId = createdByUserId,
                Description = createDto.Description,
                Type = createDto.Type,
                IncidentDate = createDto.IncidentDate ?? DateTime.UtcNow,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();
            
            // Oluşturulan hasarı getir
            var createdClaim = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Agent)
                .Include(c => c.ProcessedByUser)
                    .ThenInclude(u => u.Agent)
                .FirstOrDefaultAsync(c => c.ClaimId == claim.ClaimId);
                
            return MapToDto(createdClaim!);
        }
        
        // Hasar güncelle (admin ve agent için)
        public async Task<ClaimDto> UpdateClaimAsync(int id, UpdateClaimDto updateDto, int processedByUserId)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                throw new ArgumentException("Hasar bulunamadı");
            }
            
            // Sadece admin ve agent güncelleyebilir
            var user = await _context.Users.FindAsync(processedByUserId);
            if (user == null || (user.Role != "admin" && user.Role != "agent"))
            {
                throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok");
            }
            
            if (!string.IsNullOrEmpty(updateDto.Description))
                claim.Description = updateDto.Description;
                
            if (!string.IsNullOrEmpty(updateDto.Status))
            {
                claim.Status = updateDto.Status;
                if (updateDto.Status != "Pending")
                {
                    claim.ProcessedByUserId = processedByUserId;
                    claim.ProcessedAt = DateTime.UtcNow;
                }
            }
            
            if (!string.IsNullOrEmpty(updateDto.Type))
            {
                claim.Type = updateDto.Type;
            }
                
            if (updateDto.ApprovedAmount.HasValue)
                claim.ApprovedAmount = updateDto.ApprovedAmount.Value;
                
            if (!string.IsNullOrEmpty(updateDto.Notes))
                claim.Notes = updateDto.Notes;
            
            // UpdatedAt alanını güncelle
            claim.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            // Güncellenmiş hasarı getir
            var updatedClaim = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Agent)
                .Include(c => c.ProcessedByUser)
                    .ThenInclude(u => u.Agent)
                .FirstOrDefaultAsync(c => c.ClaimId == id);
                
            return MapToDto(updatedClaim!);
        }
        
        // Customer kendi pending claim'ini güncelle
        public async Task<ClaimDto> UpdateMyClaimAsync(int id, UpdateClaimDto updateDto, int userId)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                throw new ArgumentException("Hasar bulunamadı");
            }
            
            // Sadece kendi claim'ini güncelleyebilir
            if (claim.CreatedByUserId != userId)
            {
                throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok");
            }
            
            // Sadece Pending durumundaki claim'ler güncellenebilir
            if (claim.Status != "Pending")
            {
                throw new ArgumentException("Sadece beklemedeki hasar bildirimleri güncellenebilir");
            }
            
            // Customer sadece description güncelleyebilir
            if (!string.IsNullOrEmpty(updateDto.Description))
                claim.Description = updateDto.Description;
            
            // UpdatedAt alanını güncelle
            claim.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            // Güncellenmiş hasarı getir
            var updatedClaim = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Agent)
                .Include(c => c.ProcessedByUser)
                    .ThenInclude(u => u.Agent)
                .FirstOrDefaultAsync(c => c.ClaimId == id);
                
            return MapToDto(updatedClaim!);
        }
        
        // Hasar sil (soft delete)
        public async Task<bool> DeleteClaimAsync(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
                return false;
                
            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
            return true;
        }
        
        // Hasar istatistikleri
        public async Task<ClaimStatisticsDto> GetClaimStatisticsAsync()
        {
            var claims = await _context.Claims.ToListAsync();
            
            var statistics = new ClaimStatisticsDto
            {
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => c.Status == "Pending"),
                InReviewClaims = claims.Count(c => c.Status == "InReview"),
                ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                ResolvedClaims = claims.Count(c => c.Status == "Resolved"),
                ClosedClaims = claims.Count(c => c.Status == "Closed"),
                TotalApprovedAmount = claims.Where(c => c.ApprovedAmount.HasValue).Sum(c => c.ApprovedAmount!.Value)
            };
            
            // Hasar türüne göre sayılar
            statistics.ClaimsByType = claims
                .GroupBy(c => c.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
                
            // Aya göre sayılar
            statistics.ClaimsByMonth = claims
                .GroupBy(c => $"{c.CreatedAt.Year}-{c.CreatedAt.Month:D2}")
                .ToDictionary(g => g.Key, g => g.Count());
                
            return statistics;
        }
        
        // Hasar arama
        public async Task<List<ClaimDto>> SearchClaimsAsync(ClaimSearchDto searchDto)
        {
            var query = _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Agent)
                .Include(c => c.ProcessedByUser)
                    .ThenInclude(u => u.Agent)
                .AsQueryable();
                
            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                query = query.Where(c => c.Status == searchDto.Status);
            }
            
            if (!string.IsNullOrEmpty(searchDto.Type))
            {
                query = query.Where(c => c.Type == searchDto.Type);
            }
            
            if (searchDto.PolicyId.HasValue)
                query = query.Where(c => c.PolicyId == searchDto.PolicyId.Value);
                
            if (searchDto.CreatedByUserId.HasValue)
                query = query.Where(c => c.CreatedByUserId == searchDto.CreatedByUserId.Value);
                
            if (searchDto.StartDate.HasValue)
                query = query.Where(c => c.CreatedAt >= searchDto.StartDate.Value);
                
            if (searchDto.EndDate.HasValue)
                query = query.Where(c => c.CreatedAt <= searchDto.EndDate.Value);
                
            var claims = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return claims.Select(MapToDto).ToList();
        }
        
        // Hasar işleme
        public async Task<ClaimDto> ProcessClaimAsync(int id, string status, string notes, int processedByUserId)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                throw new ArgumentException("Hasar bulunamadı");
            }
            
            claim.Status = status;
            claim.ProcessedByUserId = processedByUserId;
            claim.ProcessedAt = DateTime.UtcNow;
            claim.Notes = notes;
            
            await _context.SaveChangesAsync();
            
            // İşlenmiş hasarı getir
            var processedClaim = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                    .ThenInclude(u => u.Agent)
                .Include(c => c.ProcessedByUser)
                    .ThenInclude(u => u.Agent)
                .FirstOrDefaultAsync(c => c.ClaimId == id);
                
            return MapToDto(processedClaim!);
        }
        
        // Claim entity'sini ClaimDto'ya dönüştür
        private static ClaimDto MapToDto(Claim claim)
        {
            return new ClaimDto
            {
                ClaimId = claim.ClaimId,
                PolicyId = claim.PolicyId,
                PolicyNumber = claim.Policy?.PolicyNumber ?? string.Empty,
                CreatedByUserId = claim.CreatedByUserId,
                CreatedByUserName = claim.CreatedByUser?.Name ?? string.Empty,
                CreatedByUserEmail = claim.CreatedByUser?.Email,
                ProcessedByUserId = claim.ProcessedByUserId,
                ProcessedByUserName = claim.ProcessedByUser?.Name ?? string.Empty,
                ProcessedByUserEmail = claim.ProcessedByUser?.Email,
                ProcessedByUserPhone = claim.ProcessedByUser?.Agent?.Phone ?? string.Empty,
                Description = claim.Description ?? string.Empty,
                Status = claim.Status,
                Type = claim.Type,
                ApprovedAmount = claim.ApprovedAmount,
                IncidentDate = claim.IncidentDate,
                CreatedAt = claim.CreatedAt,
                ProcessedAt = claim.ProcessedAt,
                Notes = claim.Notes,
                Policy = claim.Policy != null ? new PolicyDto
                {
                    PolicyId = claim.Policy.PolicyId,
                    PolicyNumber = claim.Policy.PolicyNumber,
                    StartDate = claim.Policy.StartDate,
                    EndDate = claim.Policy.EndDate,
                    TotalPremium = claim.Policy.TotalPremium,
                    Status = claim.Policy.Status
                } : null,
                CreatedByUser = claim.CreatedByUser != null ? new UserDto
                {
                    UserId = claim.CreatedByUser.UserId,
                    Name = claim.CreatedByUser.Name,
                    Email = claim.CreatedByUser.Email,
                    Role = claim.CreatedByUser.Role
                } : null,
                ProcessedByUser = claim.ProcessedByUser != null ? new UserDto
                {
                    UserId = claim.ProcessedByUser.UserId,
                    Name = claim.ProcessedByUser.Name,
                    Email = claim.ProcessedByUser.Email,
                    Role = claim.ProcessedByUser.Role
                } : null
            };
        }
    }
}
