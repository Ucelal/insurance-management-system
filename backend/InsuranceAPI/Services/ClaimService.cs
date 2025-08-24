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
                .Include(c => c.ProcessedByUser)
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
                .Include(c => c.ProcessedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            return claim != null ? MapToDto(claim) : null;
        }
        
        // Poliçeye göre hasarları getir
        public async Task<List<ClaimDto>> GetClaimsByPolicyAsync(int policyId)
        {
            var claims = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                .Include(c => c.ProcessedByUser)
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
                .Include(c => c.ProcessedByUser)
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
            
            // Enum değerlerini parse et
            if (!Enum.TryParse<ClaimType>(createDto.Type, out var claimType))
            {
                throw new ArgumentException("Geçersiz hasar türü");
            }
            
            if (!Enum.TryParse<ClaimPriority>(createDto.Priority, out var claimPriority))
            {
                throw new ArgumentException("Geçersiz öncelik");
            }
            
            var claim = new Claim
            {
                PolicyId = createDto.PolicyId,
                CreatedByUserId = createdByUserId,
                Description = createDto.Description,
                Type = claimType,
                Priority = claimPriority,
                ClaimAmount = createDto.ClaimAmount,
                EstimatedResolutionDate = createDto.EstimatedResolutionDate,
                Status = ClaimStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();
            
            // Oluşturulan hasarı getir
            var createdClaim = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                .Include(c => c.ProcessedByUser)
                .FirstOrDefaultAsync(c => c.Id == claim.Id);
                
            return MapToDto(createdClaim!);
        }
        
        // Hasar güncelle
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
                if (Enum.TryParse<ClaimStatus>(updateDto.Status, out var status))
                {
                    claim.Status = status;
                    if (status != ClaimStatus.Pending)
                    {
                        claim.ProcessedByUserId = processedByUserId;
                        claim.ProcessedAt = DateTime.UtcNow;
                    }
                }
            }
            
            if (!string.IsNullOrEmpty(updateDto.Type))
            {
                if (Enum.TryParse<ClaimType>(updateDto.Type, out var type))
                    claim.Type = type;
            }
            
            if (!string.IsNullOrEmpty(updateDto.Priority))
            {
                if (Enum.TryParse<ClaimPriority>(updateDto.Priority, out var priority))
                    claim.Priority = priority;
            }
            
            if (updateDto.ClaimAmount.HasValue)
                claim.ClaimAmount = updateDto.ClaimAmount.Value;
                
            if (updateDto.ApprovedAmount.HasValue)
                claim.ApprovedAmount = updateDto.ApprovedAmount.Value;
                
            if (updateDto.EstimatedResolutionDate.HasValue)
                claim.EstimatedResolutionDate = updateDto.EstimatedResolutionDate.Value;
                
            if (!string.IsNullOrEmpty(updateDto.Notes))
                claim.Notes = updateDto.Notes;
            
            await _context.SaveChangesAsync();
            
            // Güncellenmiş hasarı getir
            var updatedClaim = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                .Include(c => c.ProcessedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);
                
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
                PendingClaims = claims.Count(c => c.Status == ClaimStatus.Pending),
                InReviewClaims = claims.Count(c => c.Status == ClaimStatus.InReview),
                ApprovedClaims = claims.Count(c => c.Status == ClaimStatus.Approved),
                RejectedClaims = claims.Count(c => c.Status == ClaimStatus.Rejected),
                ResolvedClaims = claims.Count(c => c.Status == ClaimStatus.Resolved),
                ClosedClaims = claims.Count(c => c.Status == ClaimStatus.Closed),
                TotalClaimAmount = claims.Where(c => c.ClaimAmount.HasValue).Sum(c => c.ClaimAmount!.Value),
                TotalApprovedAmount = claims.Where(c => c.ApprovedAmount.HasValue).Sum(c => c.ApprovedAmount!.Value)
            };
            
            // Hasar türüne göre sayılar
            statistics.ClaimsByType = claims
                .GroupBy(c => c.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
                
            // Önceliğe göre sayılar
            statistics.ClaimsByPriority = claims
                .GroupBy(c => c.Priority.ToString())
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
                .Include(c => c.ProcessedByUser)
                .AsQueryable();
                
            if (!string.IsNullOrEmpty(searchDto.Status))
            {
                if (Enum.TryParse<ClaimStatus>(searchDto.Status, out var status))
                    query = query.Where(c => c.Status == status);
            }
            
            if (!string.IsNullOrEmpty(searchDto.Type))
            {
                if (Enum.TryParse<ClaimType>(searchDto.Type, out var type))
                    query = query.Where(c => c.Type == type);
            }
            
            if (!string.IsNullOrEmpty(searchDto.Priority))
            {
                if (Enum.TryParse<ClaimPriority>(searchDto.Priority, out var priority))
                    query = query.Where(c => c.Priority == priority);
            }
            
            if (searchDto.PolicyId.HasValue)
                query = query.Where(c => c.PolicyId == searchDto.PolicyId.Value);
                
            if (searchDto.CreatedByUserId.HasValue)
                query = query.Where(c => c.CreatedByUserId == searchDto.CreatedByUserId.Value);
                
            if (searchDto.StartDate.HasValue)
                query = query.Where(c => c.CreatedAt >= searchDto.StartDate.Value);
                
            if (searchDto.EndDate.HasValue)
                query = query.Where(c => c.CreatedAt <= searchDto.EndDate.Value);
                
            if (searchDto.MinAmount.HasValue)
                query = query.Where(c => c.ClaimAmount >= searchDto.MinAmount.Value);
                
            if (searchDto.MaxAmount.HasValue)
                query = query.Where(c => c.ClaimAmount <= searchDto.MaxAmount.Value);
                
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
            
            if (Enum.TryParse<ClaimStatus>(status, out var claimStatus))
            {
                claim.Status = claimStatus;
                claim.ProcessedByUserId = processedByUserId;
                claim.ProcessedAt = DateTime.UtcNow;
                claim.Notes = notes;
                
                await _context.SaveChangesAsync();
            }
            
            // İşlenmiş hasarı getir
            var processedClaim = await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.CreatedByUser)
                .Include(c => c.ProcessedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            return MapToDto(processedClaim!);
        }
        
        // Claim entity'sini ClaimDto'ya dönüştür
        private static ClaimDto MapToDto(Claim claim)
        {
            return new ClaimDto
            {
                Id = claim.Id,
                PolicyId = claim.PolicyId,
                PolicyNumber = claim.Policy?.PolicyNumber ?? string.Empty,
                CreatedByUserId = claim.CreatedByUserId,
                CreatedByUserName = claim.CreatedByUser?.Name ?? string.Empty,
                ProcessedByUserId = claim.ProcessedByUserId,
                ProcessedByUserName = claim.ProcessedByUser?.Name ?? string.Empty,
                Description = claim.Description ?? string.Empty,
                Status = claim.Status.ToString(),
                Type = claim.Type.ToString(),
                Priority = claim.Priority.ToString(),
                ClaimAmount = claim.ClaimAmount,
                ApprovedAmount = claim.ApprovedAmount,
                CreatedAt = claim.CreatedAt,
                ProcessedAt = claim.ProcessedAt,
                EstimatedResolutionDate = claim.EstimatedResolutionDate,
                Notes = claim.Notes,
                Policy = claim.Policy != null ? new PolicyDto
                {
                    Id = claim.Policy.Id,
                    PolicyNumber = claim.Policy.PolicyNumber,
                    StartDate = claim.Policy.StartDate,
                    EndDate = claim.Policy.EndDate,
                    TotalPremium = claim.Policy.TotalPremium,
                    Status = claim.Policy.Status
                } : null,
                CreatedByUser = claim.CreatedByUser != null ? new UserDto
                {
                    Id = claim.CreatedByUser.Id,
                    Name = claim.CreatedByUser.Name,
                    Email = claim.CreatedByUser.Email,
                    Role = claim.CreatedByUser.Role
                } : null,
                ProcessedByUser = claim.ProcessedByUser != null ? new UserDto
                {
                    Id = claim.ProcessedByUser.Id,
                    Name = claim.ProcessedByUser.Name,
                    Email = claim.ProcessedByUser.Email,
                    Role = claim.ProcessedByUser.Role
                } : null
            };
        }
    }
}
