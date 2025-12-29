# Replace the entire MapToDto method in OfferService.cs
$filePath = "backend/InsuranceAPI/Services/OfferService.cs"
$content = Get-Content $filePath -Raw

# Find and replace the entire MapToDto method
$oldMethodStart = '        // Offer entity''sini OfferDto''ya dönüştür'
$oldMethodEnd = '        }'

# Split content into lines
$lines = $content -split "`n"
$startIndex = -1
$endIndex = -1

# Find the start of the MapToDto method
for ($i = 0; $i -lt $lines.Length; $i++) {
    if ($lines[$i] -match $oldMethodStart) {
        $startIndex = $i
        break
    }
}

# Find the end of the method (look for the closing brace)
if ($startIndex -ne -1) {
    for ($i = $startIndex; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match '^\s*}\s*$') {
            $endIndex = $i
            break
        }
    }
}

if ($startIndex -ne -1 -and $endIndex -ne -1) {
    Write-Host "Found MapToDto method from line $startIndex to $endIndex"
    
    # Create the new method
    $newMethod = @'
        // Offer entity'sini OfferDto'ya dönüştür
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
        }
'@

    # Replace the old method with the new one
    $newLines = $newMethod -split "`n"
    $lines[$startIndex..$endIndex] = $newLines
    
    # Join lines back together
    $content = $lines -join "`n"
    
    Set-Content $filePath $content
    Write-Host "Successfully replaced MapToDto method"
} else {
    Write-Host "Could not find MapToDto method to replace"
}
