# Fix remaining issues in OfferController.cs
$filePath = "backend/InsuranceAPI/Controllers/OfferController.cs"
$content = Get-Content $filePath -Raw

# Fix all remaining issues
# 1. Fix InsuranceTypeId = insuranceType.UserId to insuranceType.InsuranceTypeId
$content = $content -replace 'InsuranceTypeId = insuranceType\.UserId', 'InsuranceTypeId = insuranceType.InsuranceTypeId'

# 2. Fix all occurrences of offer.UserId to offer.OfferId
$content = $content -replace 'offer\.UserId', 'offer.OfferId'

# 3. Fix FirstOrDefaultAsync(o => o.UserId == id) to FirstOrDefaultAsync(o => o.OfferId == id)
$content = $content -replace 'FirstOrDefaultAsync\(o => o\.UserId == id\)', 'FirstOrDefaultAsync(o => o.OfferId == id)'

# 4. Fix FirstOrDefaultAsync(o => o.UserId == id) in other contexts
$content = $content -replace 'FirstOrDefaultAsync\(o => o\.UserId == id\)', 'FirstOrDefaultAsync(o => o.OfferId == id)'

Set-Content $filePath $content
Write-Host "Fixed remaining OfferController.cs issues"
