# Insurance Management System - Database Schema (Mermaid)

## 🗄️ Entity Relationship Diagram

```mermaid
erDiagram
    %% Core User Management
    Users {
        int User_Id PK
        string Name
        string Role
        string Email UK
        string PasswordHash
        datetime Created_At
    }

    %% Customer & Agent Management
    Customers {
        int Customer_Id PK
        int User_Id FK
        string Type
        string Id_No UK
        string Address
        string Phone
    }

    Agents {
        int Agent_Id PK
        int User_Id FK
        string AgentCode UK
        string Department
        string Address
        string Phone
    }

    %% Insurance Types & Coverage
    InsuranceTypes {
        int Ins_Type_Id PK
        string Name
        string Category
        string Description
        boolean IsActive
        decimal BasePrice
        string CoverageDetails
        datetime CreatedAt
        datetime UpdatedAt
    }

    Coverages {
        int Coverage_Id PK
        string Name
        string Description
        string Type
        decimal BasePremium
        boolean IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }

    %% Business Logic
    Offers {
        int Offer_Id PK
        int Customer_Id FK
        int Agent_Id FK
        int Insurance_Type_Id FK
        string Description
        decimal BasePrice
        decimal DiscountRate
        decimal FinalPrice
        string Status
        string Department
        string CustomerAdditionalInfo
        string AgentNotes
        decimal CoverageAmount
        datetime RequestedStartDate
        datetime ValidUntil
        boolean IsCustomerApproved
        datetime CustomerApprovedAt
        datetime ReviewedAt
        int ReviewedByAgentId
        datetime CreatedAt
        datetime UpdatedAt
    }

    Policies {
        int Policy_Id PK
        int Offer_Id FK
        string PolicyNumber UK
        datetime StartDate
        datetime EndDate
        decimal TotalPremium
        string Status
        string PaymentMethod
        datetime PaidAt
        string PaymentStatus
        string Notes
        datetime CreatedAt
        datetime UpdatedAt
    }

    Claims {
        int Claim_Id PK
        int Policy_Id FK
        int Customer_Id FK
        int Agent_Id FK
        string Description
        string Status
        string Type
        string Priority
        decimal ClaimAmount
        decimal ApprovedAmount
        datetime EstimatedResolutionDate
        datetime CreatedAt
        datetime ProcessedAt
        string Notes
    }

    Payments {
        int Payment_Id PK
        int Policy_Id FK
        decimal Amount
        datetime PaidAt
        string Method
        string Status
        string TransactionId
        string Notes
        datetime CreatedAt
        datetime UpdatedAt
    }

    %% File Management
    Documents {
        int Document_Id PK
        int Customer_Id FK
        int Claim_Id FK
        int Policy_Id FK
        string FileName
        string FileUrl
        string FileType
        long FileSize
        string Category
        string Status
        string Description
        string Version
        datetime UploadedAt
        datetime UpdatedAt
        datetime ExpiresAt
        int UploadedByUser_Id FK
    }

    %% Junction Tables
    SelectedCoverages {
        int Sel_Cov_Id PK
        int Offer_Id FK
        int Coverage_Id FK
        string Notes
    }

    %% Token Management
    TokenBlacklist {
        int Token_black_Id PK
        string Token
        datetime CreatedAt
    }

    %% Relationships
    Users ||--o{ Customers : "has"
    Users ||--o{ Agents : "has"
    Users ||--o{ Documents : "uploads"

    Customers ||--o{ Offers : "receives"
    Customers ||--o{ Documents : "owns"
    Customers ||--o{ Claims : "reports"

    Agents ||--o{ Offers : "creates"
    Agents ||--o{ Claims : "processes"

    InsuranceTypes ||--o{ Coverages : "contains"
    InsuranceTypes ||--o{ Offers : "offers"

    Coverages ||--o{ SelectedCoverages : "selected_in"

    Offers ||--o{ Policies : "becomes"
    Offers ||--o{ SelectedCoverages : "includes"

    Policies ||--o{ Claims : "generates"
    Policies ||--o{ Payments : "receives"
    Policies ||--o{ Documents : "related_to"

    Claims ||--o{ Documents : "supports"
```

## 📊 Database Tables Summary

### 🔐 **Core Tables**
- **Users** - Kullanıcı yönetimi (Admin, Agent, Customer)
- **Customers** - Müşteri bilgileri
- **Agents** - Acenta bilgileri

### 🏦 **Insurance Tables**
- **InsuranceTypes** - Sigorta türleri (Sağlık, Kasko, Deprem)
- **Coverages** - Teminat detayları
- **SelectedCoverages** - Seçilen teminatlar

### 💼 **Business Tables**
- **Offers** - Sigorta teklifleri
- **Policies** - Sigorta poliçeleri
- **Claims** - Hasar talepleri
- **Payments** - Ödeme kayıtları

### 📁 **File Management**
- **Documents** - Dosya yönetimi

## 🔗 **Key Relationships**

1. **User → Customer/Agent** (1:1) - Bir kullanıcı ya müşteri ya da acentadır
2. **Customer → Offers** (1:N) - Müşteri birden fazla teklif alabilir
3. **Agent → Offers** (1:N) - Acenta birden fazla teklif oluşturabilir
4. **Offer → Policy** (1:1) - Teklif onaylandığında poliçe olur
5. **Policy → Claims** (1:N) - Poliçeden birden fazla hasar talebi olabilir
6. **Policy → Payments** (1:N) - Poliçe için birden fazla ödeme olabilir
7. **InsuranceType → Coverages** (1:N) - Sigorta türü birden fazla teminat içerebilir

## 🎯 **Database Features**

- **Referential Integrity** - Foreign key constraints
- **Audit Trail** - CreatedAt, UpdatedAt fields
- **Soft Delete** - Status fields for active/inactive records
- **Flexible File Storage** - Documents linked to multiple entities
- **Role-Based Access** - User roles determine access levels

## 📝 **Mermaid Format Notes**

Bu diagram Mermaid ER diagram formatında yazılmıştır ve şu özelliklere sahiptir:

- **PK** = Primary Key
- **FK** = Foreign Key  
- **UK** = Unique Key
- **||--o{** = One-to-Many relationship
- **||--||** = One-to-One relationship
- **}o--o{** = Many-to-Many relationship

### 🚀 **Kullanım**

Bu diagram'ı şu platformlarda görselleştirebilirsin:
- **GitHub** - README.md dosyalarında otomatik render
- **GitLab** - Markdown dosyalarında otomatik render
- **Mermaid Live Editor** - https://mermaid.live
- **VS Code** - Mermaid extension ile
- **Notion** - Mermaid desteği ile
