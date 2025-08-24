using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using InsuranceAPI.Data;
using InsuranceAPI.Services;
using InsuranceAPI.Models;


var builder = WebApplication.CreateBuilder(args);

// Controller ve API explorer servislerini ekle
builder.Services.AddControllers();
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
            policy.WithOrigins("http://localhost:3000")
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
});

// Business logic servislerini kaydet
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<IOfferService, OfferService>(); // Teklif servisini kaydet
        builder.Services.AddScoped<IPolicyService, PolicyService>(); // Poliçe servisini kaydet
        builder.Services.AddScoped<IClaimService, ClaimService>(); // Hasar servisini kaydet
        builder.Services.AddScoped<IPaymentService, PaymentService>(); // Ödeme servisini kaydet
        builder.Services.AddScoped<IDocumentService, DocumentService>(); // Döküman servisini kaydet
        builder.Services.AddScoped<IAgentService, AgentService>(); // Acenta servisini kaydet
        builder.Services.AddScoped<IFileUploadService, FileUploadService>(); // File Upload servisini kaydet
        builder.Services.AddScoped<IReportService, ReportService>(); // Report servisini kaydet

var app = builder.Build();

// HTTP istek pipeline'ını yapılandır
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Insurance API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization(); // Authorization geri eklendi
app.MapControllers();

// Seed data ekle - veritabanı oluşturulduktan sonra
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
    
    // Veritabanını oluştur
    context.Database.EnsureCreated();
    
    // Test kullanıcıları ekle (eğer yoksa)
    if (!context.Users.Any())
    {
        var adminUser = new User
        {
            Name = "Admin User",
            Email = "admin@insurance.com",
            Role = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            CreatedAt = DateTime.UtcNow
        };
        
        var testCustomer = new User
        {
            Name = "Test Customer",
            Email = "test@customer.com",
            Role = "customer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            CreatedAt = DateTime.UtcNow
        };
        
        var testAgent = new User
        {
            Name = "Test Agent",
            Email = "agent@insurance.com",
            Role = "agent",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.AddRange(adminUser, testCustomer, testAgent);
        await context.SaveChangesAsync();
        
        // Customer bilgilerini ekle
        var customer = new Customer
        {
            UserId = testCustomer.Id,
            Type = "bireysel",
            IdNo = $"CUST_{testCustomer.Id}_{DateTime.UtcNow:yyyyMMdd}",
            Address = "Test Address",
            Phone = "555 123 4567"
        };
        
        // Agent bilgilerini ekle
        var agent = new Agent
        {
            UserId = testAgent.Id,
            AgentCode = "AG001",
            Department = "Satış",
            Address = "Test Agent Address",
            Phone = "555 999 8888"
        };
        
        context.Customers.Add(customer);
        context.Agents.Add(agent);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Test kullanıcıları başarıyla eklendi!");
        
        // Test poliçesi ve ödemesi ekle
        var testOffer = new Offer
        {
            CustomerId = customer.Id,
            AgentId = agent.Id,
            InsuranceTypeId = 1, // Kasko Sigortası
            Description = "Test Kasko Teklifi",
            BasePrice = 2500.00m,
            DiscountRate = 10.0m,
            FinalPrice = 2250.00m,
            ValidUntil = DateTime.UtcNow.AddDays(30),
            Status = "Approved",
            CreatedAt = DateTime.UtcNow
        };
        
        context.Offers.Add(testOffer);
        await context.SaveChangesAsync();
        
        var testPolicy = new Policy
        {
            OfferId = testOffer.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            PolicyNumber = $"POL_{DateTime.UtcNow:yyyyMMdd}_{testOffer.Id}",
            TotalPremium = 2250.00m,
            Status = "Active",
            PaymentMethod = "KrediKarti",
            PaymentStatus = "Paid",
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Policies.Add(testPolicy);
        await context.SaveChangesAsync();
        
        var testPayment = new Payment
        {
            PolicyId = testPolicy.Id,
            Amount = 2250.00m,
            Method = PaymentMethod.KrediKarti,
            Status = PaymentStatus.Basarili,
            TransactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_001",
            Notes = "Test ödemesi",
            CreatedAt = DateTime.UtcNow
        };
        
        context.Payments.Add(testPayment);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Test poliçesi ve ödemesi başarıyla eklendi!");
    }
    
    // Sigorta türleri ve teminatları ekle (eğer yoksa) - GEÇİCİ OLARAK DEVRE DIŞI
    /*
    if (!context.InsuranceTypes.Any())
    {
        // 1. Kasko Sigortası
        var kaskoInsurance = new InsuranceType
        {
            Name = "Kasko Sigortası",
            Category = "Araç",
            Description = "Araç sahibinin kendi aracına verebileceği zararları karşılayan kapsamlı sigorta",
            BasePrice = 2500.00m,
            CoverageDetails = "Çarpışma, hırsızlık, doğal afetler, vandalizm",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // 2. Trafik Sigortası
        var trafikInsurance = new InsuranceType
        {
            Name = "Trafik Sigortası",
            Category = "Araç",
            Description = "Zorunlu trafik sigortası - üçüncü şahıslara verilen zararları karşılar",
            BasePrice = 800.00m,
            CoverageDetails = "Üçüncü şahıs maddi ve manevi tazminat",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // 3. Konut Sigortası
        var konutInsurance = new InsuranceType
        {
            Name = "Konut Sigortası",
            Category = "Konut",
            Description = "Ev ve eşyaların çeşitli risklere karşı korunması",
            BasePrice = 1200.00m,
            CoverageDetails = "Yangın, hırsızlık, su baskını, deprem",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // 4. Sağlık Sigortası
        var saglikInsurance = new InsuranceType
        {
            Name = "Sağlık Sigortası",
            Category = "Sağlık",
            Description = "Sağlık giderlerinin karşılanması ve tedavi masrafları",
            BasePrice = 3000.00m,
            CoverageDetails = "Hastane, ilaç, doktor, ameliyat",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // 5. Hayat Sigortası
        var hayatInsurance = new InsuranceType
        {
            Name = "Hayat Sigortası",
            Category = "Hayat",
            Description = "Hayat riskine karşı koruma ve tasarruf imkanı",
            BasePrice = 5000.00m,
            CoverageDetails = "Vefat, maluliyet, tasarruf",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // 6. İşyeri Sigortası
        var isyeriInsurance = new InsuranceType
        {
            Name = "İşyeri Sigortası",
            Category = "İşyeri",
            Description = "İşyeri ve işletme varlıklarının korunması",
            BasePrice = 2000.00m,
            CoverageDetails = "Yangın, hırsızlık, iş kazası, sorumluluk",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // 7. Deprem Sigortası
        var depremInsurance = new InsuranceType
        {
            Name = "Deprem Sigortası",
            Category = "Doğal Afet",
            Description = "Deprem sonrası oluşan zararları karşılayan özel sigorta",
            BasePrice = 1500.00m,
            CoverageDetails = "Deprem hasarı, yıkım, hasar tespiti",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // 8. Seyahat Sigortası
        var seyahatInsurance = new InsuranceType
        {
            Name = "Seyahat Sigortası",
            Category = "Seyahat",
            Description = "Seyahat sırasında oluşabilecek risklere karşı koruma",
            BasePrice = 300.00m,
            CoverageDetails = "Sağlık, bagaj, iptal, kaza",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.InsuranceTypes.AddRange(
            kaskoInsurance, trafikInsurance, konutInsurance, saglikInsurance,
            hayatInsurance, isyeriInsurance, depremInsurance, seyahatInsurance
        );
        await context.SaveChangesAsync();
        
        Console.WriteLine("Sigorta türleri başarıyla eklendi!");
        
        // Şimdi her sigorta türü için teminatlar ekleyelim
        await AddCoveragesForInsuranceTypes(context);
    }
    */
}

// Teminatları ekleyen yardımcı metod - GEÇİCİ OLARAK DEVRE DIŞI
/*
static async Task AddCoveragesForInsuranceTypes(InsuranceDbContext context)
{
    var insuranceTypes = await context.InsuranceTypes.ToListAsync();
    
    foreach (var insuranceType in insuranceTypes)
    {
        var coverages = new List<Coverage>();
        
        switch (insuranceType.Category)
        {
            case "Araç":
                if (insuranceType.Name.Contains("Kasko"))
                {
                    coverages.AddRange(new[]
                    {
                        new Coverage { Name = "Çarpışma Teminatı", Description = "Araç çarpışması sonucu oluşan hasarlar", Limit = 50000, Premium = 800, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Coverage { Name = "Hırsızlık Teminatı", Description = "Araç hırsızlığı ve çalınması", Limit = 100000, Premium = 600, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Coverage { Name = "Doğal Afet Teminatı", Description = "Sel, fırtına, dolu gibi doğal afetler", Limit = 75000, Premium = 400, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Coverage { Name = "Vandalizm Teminatı", Description = "Kasıtlı zarar verme", Limit = 25000, Premium = 300, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
                    });
                }
                else if (insuranceType.Name.Contains("Trafik"))
                {
                    coverages.AddRange(new[]
                    {
                        new Coverage { Name = "Üçüncü Şahıs Maddi Tazminat", Description = "Üçüncü şahıslara verilen maddi zararlar", Limit = 100000, Premium = 500, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Coverage { Name = "Üçüncü Şahıs Manevi Tazminat", Description = "Üçüncü şahıslara verilen manevi zararlar", Limit = 50000, Premium = 300, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
                    });
                }
                break;
                
            case "Konut":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Yangın Teminatı", Description = "Yangın sonucu oluşan hasarlar", Limit = 200000, Premium = 400, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Hırsızlık Teminatı", Description = "Ev hırsızlığı ve eşya çalınması", Limit = 50000, Premium = 300, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Su Baskını Teminatı", Description = "Su baskını ve su hasarı", Limit = 75000, Premium = 250, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Deprem Teminatı", Description = "Deprem sonucu oluşan hasarlar", Limit = 150000, Premium = 500, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "Sağlık":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Hastane Teminatı", Description = "Yataklı tedavi giderleri", Limit = 100000, Premium = 1200, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "İlaç Teminatı", Description = "Reçeteli ilaç giderleri", Limit = 25000, Premium = 600, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Doktor Muayene", Description = "Poliklinik ve muayene giderleri", Limit = 15000, Premium = 400, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Ameliyat Teminatı", Description = "Cerrahi müdahale giderleri", Limit = 200000, Premium = 800, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "Hayat":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Vefat Teminatı", Description = "Vefat durumunda ödenen tazminat", Limit = 500000, Premium = 2000, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Maluliyet Teminatı", Description = "Sürekli maluliyet durumunda ödeme", Limit = 300000, Premium = 1500, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Tasarruf Teminatı", Description = "Belirli süre sonra ödenen tasarruf", Limit = 100000, Premium = 1000, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "İşyeri":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Yangın Teminatı", Description = "İşyeri yangın hasarları", Limit = 300000, Premium = 600, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Hırsızlık Teminatı", Description = "İşyeri hırsızlık ve soygun", Limit = 100000, Premium = 400, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "İş Kazası Teminatı", Description = "Çalışan iş kazası tazminatı", Limit = 200000, Premium = 800, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Sorumluluk Teminatı", Description = "Üçüncü şahıs sorumluluğu", Limit = 150000, Premium = 500, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "Doğal Afet":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Deprem Hasarı", Description = "Deprem sonucu yapısal hasarlar", Limit = 200000, Premium = 800, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Hasar Tespiti", Description = "Deprem hasar tespit giderleri", Limit = 10000, Premium = 200, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Geçici Konaklama", Description = "Deprem sonrası geçici konaklama", Limit = 25000, Premium = 300, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
                
            case "Seyahat":
                coverages.AddRange(new[]
                {
                    new Coverage { Name = "Sağlık Teminatı", Description = "Seyahat sırasında sağlık giderleri", Limit = 50000, Premium = 150, IsOptional = false, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Bagaj Teminatı", Description = "Bagaj kaybı ve hasarı", Limit = 10000, Premium = 50, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new Coverage { Name = "Seyahat İptali", Description = "Seyahat iptal giderleri", Limit = 15000, Premium = 100, IsOptional = true, InsuranceTypeId = insuranceType.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
                });
                break;
        }
        
        if (coverages.Any())
        {
            context.Coverages.AddRange(coverages);
        }
    }
    
    await context.SaveChangesAsync();
    Console.WriteLine("Teminatlar başarıyla eklendi!");
}
*/

app.Run();
