using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using InsuranceAPI.DTOs;
using InsuranceAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAPI.Services
{
    public class PdfService : IPdfService
    {
        private readonly InsuranceDbContext _context;
        private readonly string _pdfStoragePath;

        public PdfService(InsuranceDbContext context)
        {
            _context = context;
            _pdfStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", "pdfs");
            
            // PDF klasörünü oluştur
            if (!Directory.Exists(_pdfStoragePath))
            {
                Directory.CreateDirectory(_pdfStoragePath);
            }
        }

        public async Task<byte[]> CreatePolicyPdfAsync(PolicyDto policy)
        {
            try
            {
                Console.WriteLine($"🔍 PdfService: Starting PDF creation for policy: {policy.PolicyNumber}");
                
                using var memoryStream = new MemoryStream();
                using var writer = new PdfWriter(memoryStream);
                using var pdf = new PdfDocument(writer);
                using var document = new Document(pdf);

                // Fontlar - iText7 ile (StandardFonts kullanarak)
                var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var sectionHeaderFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var smallFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var totalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                
                Console.WriteLine($"🔍 PdfService: Fonts created successfully");
                
                // HEADER - Başlık ve poliçe bilgileri
                var title = new Paragraph("Sigorta Poliçesi")
                    .SetFont(titleFont)
                    .SetFontSize(20)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(15);
                document.Add(title);
                
                // Poliçe numarası ve tarih
                var policyInfo = new Paragraph($"Poliçe No: {policy.PolicyNumber}")
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(5);
                document.Add(policyInfo);
                
                var dateInfo = new Paragraph($"Oluşturulma Tarihi: {policy.CreatedAt:dd.MM.yyyy HH:mm}")
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(15);
                document.Add(dateInfo);
                
                // Alt çizgi
                var line = new Paragraph("─────────────────────────────────────────────")
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(line);
                
                // POLİÇE BİLGİLERİ Bölümü
                var policyHeader = new Paragraph("Poliçe Bilgileri")
                    .SetFont(sectionHeaderFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10);
                document.Add(policyHeader);
                
                // Basit paragraf formatında bilgiler
                var statusInfo = new Paragraph($"Durum: {policy.Status}")
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetMarginBottom(5);
                document.Add(statusInfo);
                
                var startDateInfo = new Paragraph($"Başlangıç Tarihi: {policy.StartDate:dd.MM.yyyy}")
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetMarginBottom(5);
                document.Add(startDateInfo);
                
                var endDateInfo = new Paragraph($"Bitiş Tarihi: {policy.EndDate:dd.MM.yyyy}")
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetMarginBottom(5);
                document.Add(endDateInfo);
                
                var premiumInfo = new Paragraph($"Toplam Prim: ₺{policy.TotalPremium:N2}")
                    .SetFont(totalFont)
                    .SetFontSize(11)
                    .SetMarginBottom(15);
                document.Add(premiumInfo);
                
                // SİGORTA BİLGİLERİ Bölümü
                var insuranceHeader = new Paragraph("Sigorta Bilgileri")
                    .SetFont(sectionHeaderFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10);
                document.Add(insuranceHeader);
                
                var insuranceTypeInfo = new Paragraph($"Sigorta Türü: {policy.Offer?.InsuranceTypeName ?? "Belirtilmemiş"}")
                    .SetFont(normalFont)
                    .SetFontSize(11)
                    .SetMarginBottom(15);
                document.Add(insuranceTypeInfo);
                
                // TEKLİF BİLGİLERİ Bölümü (eğer varsa)
                if (policy.Offer != null)
                {
                    var offerHeader = new Paragraph("Teklif Bilgileri")
                        .SetFont(sectionHeaderFont)
                        .SetFontSize(14)
                        .SetMarginBottom(10);
                    document.Add(offerHeader);
                    
                    var offerIdInfo = new Paragraph($"Teklif No: #{policy.Offer.OfferId}")
                        .SetFont(normalFont)
                        .SetFontSize(11)
                        .SetMarginBottom(5);
                    document.Add(offerIdInfo);
                    
                    var basePriceInfo = new Paragraph($"Temel Fiyat: ₺{policy.Offer.BasePrice:N2}")
                        .SetFont(normalFont)
                        .SetFontSize(11)
                        .SetMarginBottom(5);
                    document.Add(basePriceInfo);
                    
                    var discountInfo = new Paragraph($"İndirim Oranı: %{policy.Offer.DiscountRate:N2}")
                        .SetFont(normalFont)
                        .SetFontSize(11)
                        .SetMarginBottom(5);
                    document.Add(discountInfo);
                    
                    var finalPriceInfo = new Paragraph($"Final Fiyat: ₺{policy.Offer.FinalPrice:N2}")
                        .SetFont(totalFont)
                        .SetFontSize(11)
                        .SetMarginBottom(5);
                    document.Add(finalPriceInfo);
                    
                    var coverageInfo = new Paragraph($"Teminat Tutarı: ₺{policy.Offer.CoverageAmount:N2}")
                        .SetFont(normalFont)
                        .SetFontSize(11)
                        .SetMarginBottom(15);
                    document.Add(coverageInfo);
                }
                
                // NOTLAR Bölümü (eğer varsa)
                if (!string.IsNullOrEmpty(policy.Notes))
                {
                    var notesHeader = new Paragraph("Notlar")
                        .SetFont(sectionHeaderFont)
                        .SetFontSize(14)
                        .SetMarginBottom(10);
                    document.Add(notesHeader);
                    
                    var notesParagraph = new Paragraph(policy.Notes)
                        .SetFont(normalFont)
                        .SetFontSize(11)
                        .SetMarginBottom(15);
                    document.Add(notesParagraph);
                }
                
                // TEKLİF FORMUNU ONAYLAYAN YETKİLİ BİLGİLERİ Bölümü
                if (!string.IsNullOrEmpty(policy.ApprovedByAgentName))
                {
                    var approvedByHeader = new Paragraph("Onaylayan Yetkili")
                        .SetFont(sectionHeaderFont)
                        .SetFontSize(14)
                        .SetMarginBottom(10);
                    document.Add(approvedByHeader);
                    
                    var agentNameInfo = new Paragraph($"Ad Soyad: {policy.ApprovedByAgentName}")
                        .SetFont(normalFont)
                        .SetFontSize(11)
                        .SetMarginBottom(5);
                    document.Add(agentNameInfo);
                    
                    if (!string.IsNullOrEmpty(policy.ApprovedByAgentPhone))
                    {
                        var phoneInfo = new Paragraph($"Telefon: {policy.ApprovedByAgentPhone}")
                            .SetFont(normalFont)
                            .SetFontSize(11)
                            .SetMarginBottom(5);
                        document.Add(phoneInfo);
                    }
                    
                    if (!string.IsNullOrEmpty(policy.ApprovedByAgentEmail))
                    {
                        var emailInfo = new Paragraph($"E-posta: {policy.ApprovedByAgentEmail}")
                            .SetFont(normalFont)
                            .SetFontSize(11)
                            .SetMarginBottom(15);
                        document.Add(emailInfo);
                    }
                }
                
                // FOOTER - Alt bilgi
                var footer = new Paragraph("Bu poliçe elektronik ortamda oluşturulmuştur ve geçerlidir.\nSigorta şirketi tarafından düzenlenmiştir.")
                    .SetFont(smallFont)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(30);
                document.Add(footer);

                document.Close();
                
                var pdfBytes = memoryStream.ToArray();
                Console.WriteLine($"✅ PdfService: PDF created successfully, size: {pdfBytes.Length} bytes");
                
                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating policy PDF: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                Console.WriteLine($"❌ Inner exception: {ex.InnerException?.Message}");
                throw new Exception($"PDF oluşturma hatası: {ex.Message}", ex);
            }
        }


        public async Task<string> SavePdfAsync(byte[] pdfBytes, string fileName, string category = "Policy", string description = null, int? customerId = null, int? userId = null)
        {
            try
            {
                var filePath = Path.Combine(_pdfStoragePath, fileName);
                Console.WriteLine($"🔍 PdfService: Saving PDF to: {filePath}");
                Console.WriteLine($"🔍 PdfService: PDF size: {pdfBytes.Length} bytes");
                Console.WriteLine($"🔍 PdfService: Directory exists: {Directory.Exists(_pdfStoragePath)}");
                
                await File.WriteAllBytesAsync(filePath, pdfBytes);
                
                var fileUrl = $"/documents/pdfs/{fileName}";
                Console.WriteLine($"✅ PdfService: PDF saved successfully: {filePath}");
                Console.WriteLine($"✅ PdfService: File URL: {fileUrl}");
                
                // PDF'i veritabanında sakla
                try
                {
                    var dbDocument = new Models.Document
                    {
                        FileName = fileName,
                        FileUrl = fileUrl,
                        FileType = "application/pdf",
                        FileSize = pdfBytes.Length,
                        Category = category,
                        Description = description ?? $"{category} PDF dokümanı",
                        Status = "Active",
                        UploadedAt = DateTime.UtcNow,
                        CustomerId = customerId,
                        UserId = userId
                    };
                    
                    _context.Documents.Add(dbDocument);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"✅ {category} PDF saved to database with ID: {dbDocument.DocumentId}");
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"❌ Error saving {category} PDF to database: {dbEx.Message}");
                    // Veritabanı hatası PDF oluşturmayı engellemez
                }
                
                return fileUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PdfService: Error saving PDF: {ex.Message}");
                Console.WriteLine($"❌ PdfService: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<string> CreatePaymentReceiptPdfAsync(object receipt)
        {
            try
            {
                // Basit implementasyon - şimdilik boş döndür
                await Task.Delay(1); // Async metod olduğu için
                return "/documents/pdfs/payment-receipt.pdf";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PdfService: Error creating payment receipt PDF: {ex.Message}");
                throw;
            }
        }
    }
}

