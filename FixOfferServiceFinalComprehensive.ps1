# Comprehensive fix for OfferService.cs MapToDto method
$filePath = "backend/InsuranceAPI/Services/OfferService.cs"
$content = Get-Content $filePath -Raw

# Define the old method pattern to find
$oldMethodPattern = '        // Offer entity''sini OfferDto''ya dönüştür
        private static OfferDto MapToDto\(Offer offer\)
        \{
            return new OfferDto
            \{
                OfferId = offer\.OfferId,
                CustomerId = offer\.CustomerId,
                AgentId = offer\.AgentId,
                InsuranceTypeId = offer\.InsuranceTypeId,
                Description = offer\.Description,
                BasePrice = offer\.BasePrice,
                DiscountRate = offer\.DiscountRate,
                FinalPrice = offer\.FinalPrice,
                Status = offer\.Status,
                ValidUntil = offer\.ValidUntil,
                CreatedAt = offer\.CreatedAt,
                UpdatedAt = offer\.UpdatedAt,
                
                // Navigation properties
                Customer = offer\.Customer != null \? new CustomerDto
                \{
                    Id = offer\.Customer\.CustomerId,
                    UserId = offer\.Customer\.UserId,
                    Type = offer\.Customer\.Type,
                    IdNo = offer\.Customer\.CustomerIdNo,
                    Address = offer\.Customer\.Address,
                    Phone = offer\.Customer\.Phone,
                    User = offer\.Customer\.User != null \? new UserDto
                    \{
                        Id = offer\.Customer\.User\.UserId,
                        Name = offer\.Customer\.User\.Name,
                        Email = offer\.Customer\.User\.Email,
                        Role = offer\.Customer\.User\.Role,
                        CreatedAt = offer\.Customer\.User\.CreatedAt
                    \} : null
                \} : null,
                
                Agent = offer\.Agent != null \? new AgentDto
                \{
                    Id = offer\.Agent\.AgentId,
                    UserId = offer\.Agent\.UserId,
                    AgentCode = offer\.Agent\.AgentCode,
                    Department = offer\.Agent\.Department,
                    Address = offer\.Agent\.Address,
                    Phone = offer\.Agent\.Phone,
                    User = offer\.Agent\.User != null \? new UserDto
                    \{
                        Id = offer\.Agent\.User\.UserId,
                        Name = offer\.Agent\.User\.Name,
                        Email = offer\.Agent\.User\.Email,
                        Role = offer\.Agent\.User\.Role,
                        CreatedAt = offer\.Agent\.User\.CreatedAt
                    \} : null
                \} : null,
                
                InsuranceType = offer\.InsuranceType != null \? new InsuranceTypeDto
                \{
                    Id = offer\.InsuranceType\.InsuranceTypeId,
                    Name = offer\.InsuranceType\.Name,
                    Category = offer\.InsuranceType\.Category,
                    Description = offer\.InsuranceType\.Description,
                    IsActive = offer\.InsuranceType\.IsActive,
                    BasePrice = offer\.InsuranceType\.BasePrice,
                    CoverageDetails = offer\.InsuranceType\.CoverageDetails,
                    CreatedAt = offer\.InsuranceType\.CreatedAt,
                    UpdatedAt = offer\.InsuranceType\.UpdatedAt
                \} : null,
                
                SelectedCoverages = offer\.SelectedCoverages\?\\.Select\(sc => new SelectedCoverageDto
                \{
                    Id = sc\.SelectedCoverageId,
                    OfferId = sc\.OfferId,
                    CoverageId = sc\.CoverageId,
                    SelectedLimit = sc\.SelectedLimit,
                    Premium = sc\.Premium,
                    IsSelected = sc\.IsSelected,
                    CreatedAt = sc\.CreatedAt
                \}\)\.ToList\(\) \?\? new List<SelectedCoverageDto>\(\)
            \};
        \}'

# Define the new method
$newMethod = '        // Offer entity''sini OfferDto''ya dönüştür
        private static OfferDto MapToDto(Offer offer)
        {
            return new OfferDto
            {
                OfferId = offer.OfferId,
                CustomerId = offer.CustomerId,
                AgentId = offer.AgentId,
                InsuranceTypeId = offer.InsuranceTypeId,
                Description = offer.Description,
                BasePrice = offer.BasePrice,
                DiscountRate = offer.DiscountRate,
                FinalPrice = offer.FinalPrice,
                Status = offer.Status,
                ValidUntil = offer.ValidUntil,
                CreatedAt = offer.CreatedAt,
                UpdatedAt = offer.UpdatedAt,
                
                // Flat properties for easier access
                CustomerName = offer.Customer?.User?.Name ?? string.Empty,
                CustomerEmail = offer.Customer?.User?.Email ?? string.Empty,
                CustomerPhone = offer.Customer?.Phone ?? string.Empty,
                CustomerAddress = offer.Customer?.Address ?? string.Empty,
                CustomerType = offer.Customer?.Type ?? string.Empty,
                IdNo = offer.Customer?.IdNo ?? string.Empty,
                UserId = offer.Customer?.UserId ?? 0,
                AgentName = offer.Agent?.User?.Name ?? string.Empty,
                AgentEmail = offer.Agent?.User?.Email ?? string.Empty,
                AgentPhone = offer.Agent?.Phone ?? string.Empty,
                AgentAddress = offer.Agent?.Address ?? string.Empty,
                AgentCode = offer.Agent?.AgentCode ?? string.Empty,
                AgentUserId = offer.Agent?.UserId ?? 0,
                InsuranceTypeName = offer.InsuranceType?.Name ?? string.Empty,
                InsuranceTypeCategory = offer.InsuranceType?.Category ?? string.Empty,
                
                // Additional properties
                IsCustomerApproved = offer.IsCustomerApproved,
                CustomerApprovedAt = offer.CustomerApprovedAt,
                AgentNotes = offer.AgentNotes,
                ReviewedAt = offer.ReviewedAt,
                ReviewedByAgentId = offer.ReviewedByAgentId,
                CustomerAdditionalInfo = offer.CustomerAdditionalInfo,
                CoverageAmount = offer.CoverageAmount,
                RequestedStartDate = offer.RequestedStartDate,
                Department = offer.Department,
                
                SelectedCoverages = offer.SelectedCoverages?.Select(sc => new SelectedCoverageDto
                {
                    SelectedCoverageId = sc.SelectedCoverageId,
                    OfferId = sc.OfferId,
                    CoverageId = sc.CoverageId,
                    Premium = sc.Premium,
                    Notes = sc.Notes
                }).ToList() ?? new List<SelectedCoverageDto>()
            };
        }'

# Replace the old method with the new one
$content = $content -replace $oldMethodPattern, $newMethod

Set-Content $filePath $content
Write-Host "Successfully replaced MapToDto method with comprehensive approach"
