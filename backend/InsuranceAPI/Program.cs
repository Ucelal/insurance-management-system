using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using InsuranceAPI.Data;
using InsuranceAPI.Services;
using InsuranceAPI.Models;
using InsuranceAPI.Hubs;


var builder = WebApplication.CreateBuilder(args);

// Controller ve API explorer servislerini ekle
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // camelCase kullan
        options.JsonSerializerOptions.WriteIndented = true; // Okunabilir JSON
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // DateTime'ları ISO 8601 formatında gönder
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// WebSocket desteği ekle
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Insurance Management System API", 
        Version = "v1",
        Description = "API for Insurance Management System"
    });
    
    // JWT Authentication için Swagger UI'da Authorize butonu
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// CORS politikasını ekle - React uygulaması için
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5001", "http://localhost:5002")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Entity Framework veritabanı bağlantısını ekle
builder.Services.AddDbContext<InsuranceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication sistemi geri eklendi
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured")))
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));
    
    options.AddPolicy("AdminOrAgent", policy =>
        policy.RequireRole("admin", "agent"));
    
    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("customer"));
});

// Business logic servislerini kaydet
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOfferService, OfferService>(); // Teklif servisini kaydet
builder.Services.AddScoped<IPolicyService, PolicyService>(); // Poliçe servisini kaydet
builder.Services.AddScoped<IClaimService, ClaimService>(); // Hasar servisini kaydet
builder.Services.AddScoped<IPaymentService, PaymentService>(); // Ödeme servisini kaydet
builder.Services.AddScoped<IDocumentService, DocumentService>(); // Döküman servisini kaydet
builder.Services.AddScoped<IPdfService, PdfService>(); // PDF servisini kaydet
builder.Services.AddScoped<IAgentService, AgentService>(); // Acenta servisini kaydet
builder.Services.AddScoped<IFileUploadService, FileUploadService>(); // File Upload servisini kaydet
builder.Services.AddScoped<IReportService, ReportService>(); // Report servisini kaydet
builder.Services.AddScoped<IProfileService, ProfileService>(); // Profile servisini kaydet

// Background services
// builder.Services.AddHostedService<TokenCleanupService>(); // Geçici olarak devre dışı

var app = builder.Build();

// HTTP istek pipeline'ını yapılandır
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Insurance API V1");
    c.RoutePrefix = "swagger";
});

// WebSocket endpoint'i ekle
app.MapHub<NotificationHub>("/ws");

// HTTPS redirection - sadece production'da aktif
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowReactApp");

// Static files serving for PDFs and uploads
app.UseStaticFiles();

// Debug: Check if wwwroot exists
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (Directory.Exists(wwwrootPath))
{
    Console.WriteLine($"✅ wwwroot directory exists: {wwwrootPath}");
    var uploadsPath = Path.Combine(wwwrootPath, "uploads");
    if (Directory.Exists(uploadsPath))
    {
        Console.WriteLine($"✅ uploads directory exists: {uploadsPath}");
        var files = Directory.GetFiles(uploadsPath, "*", SearchOption.AllDirectories);
        Console.WriteLine($"📁 Found {files.Length} files in uploads directory");
    }
    else
    {
        Console.WriteLine($"❌ uploads directory not found: {uploadsPath}");
    }
}
else
{
    Console.WriteLine($"❌ wwwroot directory not found: {wwwrootPath}");
}

app.UseAuthentication();
app.UseAuthorization(); // Authorization geri eklendi
app.MapControllers();

// Seed data ekle - veritabanı oluşturulduktan sonra
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
    
    // Veritabanını oluştur (eğer yoksa)
    context.Database.EnsureCreated();
    Console.WriteLine("Veritabanı kontrol edildi ve gerekirse oluşturuldu.");
    
    // Sadece admin kullanıcısını ekle (eğer yoksa)
    User? adminUser = null;
    if (!context.Users.Any(u => u.Role == "admin"))
    {
        adminUser = new User
        {
            Name = "Admin User",
            Email = "admin@insurance.com",
            Role = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            CreatedAt = DateTime.UtcNow
        };
        
        try
        {
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            Console.WriteLine("Admin kullanıcısı başarıyla eklendi!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Admin kullanıcısı eklenirken hata oluştu: {ex.Message}");
            Console.WriteLine($"Hata detayı: {ex.InnerException?.Message}");
        }
    }
    else
    {
        // Mevcut admin user'ı al
        adminUser = await context.Users.FirstOrDefaultAsync(u => u.Role == "admin");
    }
    
    // Test customer kullanıcısını ekle (eğer yoksa)
    if (!context.Users.Any(u => u.Role == "customer"))
    {
        var customerUser = new User
        {
            Name = "Test Customer",
            Email = "customer@test.com",
            Role = "customer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
            CreatedAt = DateTime.UtcNow
        };
        
        try
        {
            context.Users.Add(customerUser);
            await context.SaveChangesAsync();
            
            // Customer entity'yi de ekle
            var customer = new Customer
            {
                UserId = customerUser.UserId,
                IdNo = "12345678901",
                Address = "Test Adres, Test Şehir",
                Phone = "05551234567"
            };
            
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            
            Console.WriteLine("Test customer kullanıcısı başarıyla eklendi!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test customer kullanıcısı eklenirken hata oluştu: {ex.Message}");
            Console.WriteLine($"Hata detayı: {ex.InnerException?.Message}");
        }
    }
    
    // 6 departman için agent'ları ekle (eğer yoksa)
    if (!context.Agents.Any() && !context.Users.Any(u => u.Role == "agent"))
    {
        var agentUsers = new[]
        {
            new { Name = "Konut Agent", Email = "konut@insurance.com", Department = "Konut Sigortası" },
            new { Name = "Seyahat Agent", Email = "seyahat@insurance.com", Department = "Seyahat Sigortası" },
            new { Name = "İş Yeri Agent", Email = "isyeri@insurance.com", Department = "İş Yeri Sigortası" },
            new { Name = "Trafik Agent", Email = "trafik@insurance.com", Department = "Trafik Sigortası" },
            new { Name = "Sağlık Agent", Email = "saglik@insurance.com", Department = "Sağlık Sigortası" },
            new { Name = "Hayat Agent", Email = "hayat@insurance.com", Department = "Hayat Sigortası" }
        };
        
        foreach (var agentInfo in agentUsers)
        {
            var agentUser = new User
            {
                Name = agentInfo.Name,
                Email = agentInfo.Email,
                Role = "agent",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent123!"),
                CreatedAt = DateTime.UtcNow
            };
            
            try
            {
                context.Users.Add(agentUser);
                await context.SaveChangesAsync();
                
                var agent = new Agent
                {
                    UserId = agentUser.UserId,
                    AgentCode = agentInfo.Department.Substring(0, 3).ToUpper(),
                    Department = agentInfo.Department,
                    Address = $"{agentInfo.Department} Departmanı, İstanbul",
                    Phone = "0555" + new Random().Next(1000000, 9999999).ToString()
                };
                
                context.Agents.Add(agent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Agent '{agentInfo.Name}' eklenirken hata oluştu: {ex.Message}");
                Console.WriteLine($"Hata detayı: {ex.InnerException?.Message}");
            }
        }
        
        try
        {
            await context.SaveChangesAsync();
            Console.WriteLine("6 departman için agent'lar başarıyla eklendi!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Agent'lar eklenirken hata oluştu: {ex.Message}");
            Console.WriteLine($"Hata detayı: {ex.InnerException?.Message}");
        }
    }
    
    // Sigorta türleri ve teminatları ekle (eğer yoksa) - SADECE 6 FARKLI SİGORTA HİZMETİ
    if (!context.InsuranceTypes.Any())
    {
        // 1. Konut Sigortası
        var konutInsurance = new InsuranceType
        {
            Name = "Konut Sigortası",
            Category = "Konut",
            Description = "Ev ve eşyaların çeşitli risklere karşı korunması",
            BasePrice = 1200.00m,
            ValidityPeriodDays = 365, // 1 yıl
            CoverageDetails = "Yangın, hırsızlık, su baskını, deprem",
            IsActive = true,
            UserId = adminUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        // 2. Seyahat Sigortası
        var seyahatInsurance = new InsuranceType
        {
            Name = "Seyahat Sigortası",
            Category = "Seyahat",
            Description = "Seyahat sırasında oluşabilecek risklere karşı koruma",
            BasePrice = 300.00m,
            ValidityPeriodDays = 30, // 1 ay
            CoverageDetails = "Sağlık, bagaj, iptal, kaza",
            IsActive = true,
            UserId = adminUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        // 3. İş Yeri Sigortası
        var isyeriInsurance = new InsuranceType
        {
            Name = "İş Yeri Sigortası",
            Category = "İşyeri",
            Description = "İşyeri ve işletme varlıklarının korunması",
            BasePrice = 2000.00m,
            ValidityPeriodDays = 365, // 1 yıl
            CoverageDetails = "Yangın, hırsızlık, iş kazası, sorumluluk",
            IsActive = true,
            UserId = adminUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        // 4. Trafik Sigortası
        var trafikInsurance = new InsuranceType
        {
            Name = "Trafik Sigortası",
            Category = "Araç",
            Description = "Zorunlu trafik sigortası - üçüncü şahıslara verilen zararları karşılar",
            BasePrice = 800.00m,
            ValidityPeriodDays = 365, // 1 yıl
            CoverageDetails = "Üçüncü şahıs maddi ve manevi tazminat",
            IsActive = true,
            UserId = adminUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        // 5. Sağlık Sigortası
        var saglikInsurance = new InsuranceType
        {
            Name = "Sağlık Sigortası",
            Category = "Sağlık",
            Description = "Sağlık giderlerinin karşılanması ve tedavi masrafları",
            BasePrice = 3000.00m,
            ValidityPeriodDays = 365, // 1 yıl
            CoverageDetails = "Hastane, ilaç, doktor, ameliyat",
            IsActive = true,
            UserId = adminUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        // 6. Hayat Sigortası
        var hayatInsurance = new InsuranceType
        {
            Name = "Hayat Sigortası",
            Category = "Hayat",
            Description = "Hayat riskine karşı koruma ve tasarruf imkanı",
            BasePrice = 5000.00m,
            ValidityPeriodDays = 365, // 1 yıl
            CoverageDetails = "Vefat, maluliyet, tasarruf",
            IsActive = true,
            UserId = adminUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        try
        {
            context.InsuranceTypes.AddRange(
                konutInsurance, seyahatInsurance, isyeriInsurance, trafikInsurance,
                saglikInsurance, hayatInsurance
            );
            await context.SaveChangesAsync();
            
            Console.WriteLine("6 farklı sigorta türü başarıyla eklendi!");
            
            // Şimdi her sigorta türü için teminatlar ekleyelim
            await AddCoveragesForInsuranceTypes(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Sigorta türleri eklenirken hata oluştu: {ex.Message}");
            Console.WriteLine($"Hata detayı: {ex.InnerException?.Message}");
        }
    }
    
    // Test müşterisi ve teklif ekle (eğer yoksa) - Sadece ilk kez çalıştırıldığında
    if (!context.Offers.Any() && !context.Users.Any(u => u.Email == "test@customer.com"))
    {
        // Test müşterisi ekle
        var testCustomerUser = new User
        {
            Name = "Test Müşteri",
            Email = "test@customer.com",
            Role = "customer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
            CreatedAt = DateTime.UtcNow
        };
        
                Customer testCustomer;
        try
        {
            context.Users.Add(testCustomerUser);
            await context.SaveChangesAsync();
            
            testCustomer = new Customer
            {
                UserId = testCustomerUser.UserId,
                IdNo = "98765432109",
                Address = "Test Müşteri Adresi, İstanbul",
                Phone = "05559876543"
            };
            
            context.Customers.Add(testCustomer);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test müşteri kullanıcısı eklenirken hata oluştu: {ex.Message}");
            Console.WriteLine($"Hata detayı: {ex.InnerException?.Message}");
            return; // Skip offer creation if customer creation fails
        }
        
        // Test teklifi ekle (Konut Sigortası departmanı için)
        var konutAgent = await context.Agents.FirstOrDefaultAsync(a => a.Department == "Konut Sigortası");
        var konutInsurance = await context.InsuranceTypes.FirstOrDefaultAsync(it => it.Name == "Konut Sigortası");
        
        if (konutAgent != null && konutInsurance != null)
        {
            var testOffer = new Offer
            {
                CustomerId = testCustomer.CustomerId,
                AgentId = konutAgent.AgentId,
                InsuranceTypeId = konutInsurance.InsuranceTypeId,
                Department = "Konut Sigortası",
                BasePrice = 1200.00m,
                DiscountRate = 0,
                FinalPrice = 1200.00m,
                Status = "pending",
                ValidUntil = CalculateValidityPeriod(konutInsurance),
                CustomerAdditionalInfo = "Test müşteri ek bilgisi",
                CoverageAmount = 200000.00m,
                RequestedStartDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            
            try
            {
                context.Offers.Add(testOffer);
                await context.SaveChangesAsync();
                Console.WriteLine("Test müşterisi ve teklifi başarıyla eklendi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test teklifi eklenirken hata oluştu: {ex.Message}");
                Console.WriteLine($"Hata detayı: {ex.InnerException?.Message}");
            }
        }
    }
}

// Teminatları ekleyen yardımcı metod
static async Task AddCoveragesForInsuranceTypes(InsuranceDbContext context)
{
    var insuranceTypes = await context.InsuranceTypes.ToListAsync();
    
    foreach (var insuranceType in insuranceTypes)
    {
        var coverages = new List<Coverage>();
        
        switch (insuranceType.Category)
        {
            case "Konut":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Yangın Teminatı", Description = "Yangın sonucu oluşan hasarlar", Limit = 200000, Premium = 400, IsOptional = false, Type = "Zorunlu", BasePremium = 400, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Hırsızlık Teminatı", Description = "Ev hırsızlığı ve eşya çalınması", Limit = 50000, Premium = 300, IsOptional = true, Type = "Opsiyonel", BasePremium = 300, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Su Baskını Teminatı", Description = "Su baskını ve su hasarı", Limit = 75000, Premium = 250, IsOptional = true, Type = "Opsiyonel", BasePremium = 250, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Deprem Teminatı", Description = "Deprem sonucu oluşan hasarlar", Limit = 150000, Premium = 500, IsOptional = true, Type = "Opsiyonel", BasePremium = 500, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "Seyahat":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Sağlık Teminatı", Description = "Seyahat sırasında sağlık giderleri", Limit = 50000, Premium = 150, IsOptional = false, Type = "Zorunlu", BasePremium = 150, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Bagaj Teminatı", Description = "Bagaj kaybı ve hasarı", Limit = 10000, Premium = 50, IsOptional = true, Type = "Opsiyonel", BasePremium = 50, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Seyahat İptali", Description = "Seyahat iptal giderleri", Limit = 15000, Premium = 100, IsOptional = true, Type = "Opsiyonel", BasePremium = 100, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "İşyeri":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Yangın Teminatı", Description = "İşyeri yangın hasarları", Limit = 300000, Premium = 600, IsOptional = false, Type = "Zorunlu", BasePremium = 600, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Hırsızlık Teminatı", Description = "İşyeri hırsızlık ve soygun", Limit = 100000, Premium = 400, IsOptional = true, Type = "Opsiyonel", BasePremium = 400, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "İş Kazası Teminatı", Description = "Çalışan iş kazası tazminatı", Limit = 200000, Premium = 800, IsOptional = true, Type = "Opsiyonel", BasePremium = 800, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Sorumluluk Teminatı", Description = "Üçüncü şahıs sorumluluğu", Limit = 150000, Premium = 500, IsOptional = true, Type = "Opsiyonel", BasePremium = 500, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "Araç":
                // Sadece Trafik Sigortası için
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Üçüncü Şahıs Maddi Tazminat", Description = "Üçüncü şahıslara verilen maddi zararlar", Limit = 100000, Premium = 500, IsOptional = false, Type = "Zorunlu", BasePremium = 500, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Üçüncü Şahıs Manevi Tazminat", Description = "Üçüncü şahıslara verilen manevi zararlar", Limit = 50000, Premium = 300, IsOptional = false, Type = "Zorunlu", BasePremium = 300, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "Sağlık":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Hastane Teminatı", Description = "Yataklı tedavi giderleri", Limit = 100000, Premium = 1200, IsOptional = false, Type = "Zorunlu", BasePremium = 1200, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "İlaç Teminatı", Description = "Reçeteli ilaç giderleri", Limit = 25000, Premium = 600, IsOptional = true, Type = "Opsiyonel", BasePremium = 600, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Doktor Muayene", Description = "Poliklinik ve muayene giderleri", Limit = 15000, Premium = 400, IsOptional = true, Type = "Opsiyonel", BasePremium = 400, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Ameliyat Teminatı", Description = "Cerrahi müdahale giderleri", Limit = 200000, Premium = 800, IsOptional = true, Type = "Opsiyonel", BasePremium = 800, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "Hayat":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Vefat Teminatı", Description = "Vefat durumunda ödenen tazminat", Limit = 500000, Premium = 2000, IsOptional = false, Type = "Zorunlu", BasePremium = 2000, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Maluliyet Teminatı", Description = "Sürekli maluliyet durumunda ödeme", Limit = 300000, Premium = 1500, IsOptional = true, Type = "Opsiyonel", BasePremium = 1500, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Tasarruf Teminatı", Description = "Belirli süre sonra ödenen tasarruf", Limit = 100000, Premium = 1000, IsOptional = true, Type = "Opsiyonel", BasePremium = 1000, InsuranceTypeId = insuranceType.InsuranceTypeId, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
        }
        
        if (coverages.Any())
        {
            context.Coverages.AddRange(coverages);
        }
    }
    
    try
    {
        await context.SaveChangesAsync();
        Console.WriteLine("Teminatlar başarıyla eklendi!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Teminatlar eklenirken hata oluştu: {ex.Message}");
        Console.WriteLine($"Hata detayı: {ex.InnerException?.Message}");
    }
}

// Geçerlilik süresi hesaplama metodu
static DateTime CalculateValidityPeriod(InsuranceType insuranceType)
{
    int validityDays = insuranceType.ValidityPeriodDays;
    
    // Sigorta türüne göre özel geçerlilik süreleri
    switch (insuranceType.Name.ToLower())
    {
        case "trafik sigortası":
        case "trafik":
            validityDays = 365; // 1 yıl
            break;
        case "konut sigortası":
        case "konut":
            validityDays = 365; // 1 yıl
            break;
        case "seyahat sigortası":
        case "seyahat":
            validityDays = 30; // 1 ay
            break;
        case "iş yeri sigortası":
        case "iş yeri":
        case "isyeri":
            validityDays = 365; // 1 yıl
            break;
        case "sağlık sigortası":
        case "saglik":
            validityDays = 365; // 1 yıl
            break;
        case "hayat sigortası":
        case "hayat":
            validityDays = 365; // 1 yıl
            break;
        default:
            validityDays = 30; // Varsayılan 1 ay
            break;
    }
    
    Console.WriteLine($"📅 Program: Calculated validity period for '{insuranceType.Name}': {validityDays} days");
    return DateTime.UtcNow.AddDays(validityDays);
}
app.Run();
