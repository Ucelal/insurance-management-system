# Fix remaining OfferService.cs issues
$filePath = "backend/InsuranceAPI/Services/OfferService.cs"
$content = Get-Content $filePath -Raw

# Fix decimal ?? int issue
$content = $content -replace '(\w+) \?\? 0', '$1 ?? 0m'

# Fix all remaining .Id references to their new PK names
$content = $content -replace '\.Id(?=\s*==)', '.OfferId'
$content = $content -replace '\.Id(?=\s*\.)', '.OfferId'
$content = $content -replace 'offer\.Id', 'offer.OfferId'
$content = $content -replace 'o\.Id(?=\s*==)', 'o.OfferId'
$content = $content -replace 'o\.Id(?=\s*\.)', 'o.OfferId'
$content = $content -replace '\.OrderBy\(o => o\.Id\)', '.OrderBy(o => o.OfferId)'

# Fix Customer.Id references
$content = $content -replace 'offer\.Customer\.Id', 'offer.Customer.CustomerId'

# Fix Agent.Id references
$content = $content -replace 'offer\.Agent\.Id', 'offer.Agent.AgentId'

# Fix InsuranceType.Id references
$content = $content -replace 'offer\.InsuranceType\.Id', 'offer.InsuranceType.InsuranceTypeId'

# Fix User.Id references
$content = $content -replace 'offer\.Customer\.User\.Id', 'offer.Customer.User.UserId'
$content = $content -replace 'offer\.Agent\.User\.Id', 'offer.Agent.User.UserId'

# Fix SelectedCoverage.Id references
$content = $content -replace 'sc\.Id', 'sc.SelectedCoverageId'

# Fix the MapToDto method Id references
$content = $content -replace 'Id = offer\.Id', 'Id = offer.OfferId'
$content = $content -replace 'Id = offer\.Customer\.Id', 'Id = offer.Customer.CustomerId'
$content = $content -replace 'Id = offer\.Agent\.Id', 'Id = offer.Agent.AgentId'
$content = $content -replace 'Id = offer\.InsuranceType\.Id', 'Id = offer.InsuranceType.InsuranceTypeId'
$content = $content -replace 'Id = offer\.Customer\.User\.Id', 'Id = offer.Customer.User.UserId'
$content = $content -replace 'Id = offer\.Agent\.User\.Id', 'Id = offer.Agent.User.UserId'
$content = $content -replace 'Id = sc\.Id', 'Id = sc.SelectedCoverageId'

Set-Content $filePath $content
Write-Host "Fixed remaining OfferService.cs issues"
