# Postman Kullanım Kılavuzu

## 📋 Dosyalar

1. **`Postman_Collection_Import.json`** - Collection dosyası
2. **`Postman_Environment_Import.json`** - Environment dosyası

## 🚀 Kurulum

### 1. Backend'i Başlatın
```bash
cd backend\InsuranceAPI
dotnet run --launch-profile http
```

### 2. Postman'de Import

#### Collection Import:
1. Postman'i açın
2. **Import** butonuna tıklayın
3. **Upload Files** seçeneğini seçin
4. `Postman_Collection_Import.json` dosyasını seçin
5. **Import** butonuna tıklayın

#### Environment Import:
1. **Import** butonuna tıklayın
2. **Upload Files** seçeneğini seçin
3. `Postman_Environment_Import.json` dosyasını seçin
4. **Import** butonuna tıklayın

### 3. Environment'ı Aktif Hale Getirin
1. Sağ üst köşedeki environment dropdown'ına tıklayın
2. **"Insurance API Environment"** seçin

## 🧪 Test Sırası

### 1. Register User
- **Method:** POST
- **URL:** `{{baseUrl}}/api/auth/register`
- **Body:**
  ```json
  {
    "name": "Test User",
    "email": "test@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "role": "customer"
  }
  ```

### 2. Login User
- **Method:** POST
- **URL:** `{{baseUrl}}/api/auth/login`
- **Body:**
  ```json
  {
    "email": "test@example.com",
    "password": "Test123!"
  }
  ```

### 3. Get Current User
- **Method:** GET
- **URL:** `{{baseUrl}}/api/auth/me`

### 4. Create Customer
- **Method:** POST
- **URL:** `{{baseUrl}}/api/customer`
- **Body:**
  ```json
  {
    "type": "bireysel",
    "idNo": "12345678901",
    "address": "Test Address, Istanbul",
    "phone": "5551234567",
    "userId": {{userId}}
  }
  ```

### 5. Get All Customers
- **Method:** GET
- **URL:** `{{baseUrl}}/api/customer`

### 6. Create Offer
- **Method:** POST
- **URL:** `{{baseUrl}}/api/offer`
- **Body:**
  ```json
  {
    "customerId": {{customerId}},
    "insuranceType": "Kasko",
    "price": 1500.00,
    "status": "pending"
  }
  ```

### 7. Get All Offers
- **Method:** GET
- **URL:** `{{baseUrl}}/api/offer`

## 🆕 Yeni Modüller

### Agent Management
- **Get All Agents:** `GET {{baseUrl}}/api/Agent`
- **Get Agent by ID:** `GET {{baseUrl}}/api/Agent/{id}`
- **Get Agent by User ID:** `GET {{baseUrl}}/api/Agent/user/{userId}`
- **Get Agents by Department:** `GET {{baseUrl}}/api/Agent/department/{department}`
- **Create Agent:** `POST {{baseUrl}}/api/Agent`
- **Update Agent:** `PUT {{baseUrl}}/api/Agent/{id}`
- **Delete Agent:** `DELETE {{baseUrl}}/api/Agent/{id}`
- **Check Agent Code Unique:** `GET {{baseUrl}}/api/Agent/check-code/{code}`

### File Upload
- **Upload File:** `POST {{baseUrl}}/api/FileUpload/upload`
- **Download File:** `GET {{baseUrl}}/api/FileUpload/download/{documentId}`
- **Get Files by Customer:** `GET {{baseUrl}}/api/FileUpload/customer/{customerId}`
- **Get Files by Claim:** `GET {{baseUrl}}/api/FileUpload/claim/{claimId}`
- **Get Files by Policy:** `GET {{baseUrl}}/api/FileUpload/policy/{policyId}`
- **Update File Metadata:** `PUT {{baseUrl}}/api/FileUpload/{documentId}`
- **Delete File:** `DELETE {{baseUrl}}/api/FileUpload/{documentId}`
- **Check File Access:** `GET {{baseUrl}}/api/FileUpload/access/{documentId}`
- **Get Supported Formats:** `GET {{baseUrl}}/api/FileUpload/supported-formats`
- **Get File Status:** `GET {{baseUrl}}/api/FileUpload/status/{documentId}`

### Reporting
- **Generate Sales Report:** `POST {{baseUrl}}/api/Report/sales`
- **Generate Claims Report:** `POST {{baseUrl}}/api/Report/claims`
- **Generate Customer Report:** `POST {{baseUrl}}/api/Report/customers`
- **Generate Payment Report:** `POST {{baseUrl}}/api/Report/payments`
- **Get Dashboard Summary:** `GET {{baseUrl}}/api/Report/dashboard`
- **Get Chart Data:** `GET {{baseUrl}}/api/Report/charts/{chartType}`
- **Quick Reports:** `GET {{baseUrl}}/api/Report/sales/quick`, `GET {{baseUrl}}/api/Report/claims/quick`
- **Agent Performance Report:** `GET {{baseUrl}}/api/Report/agent-performance/{agentId}`
- **Customer Segmentation Report:** `GET {{baseUrl}}/api/Report/customer-segmentation`
- **Trend Analysis Report:** `GET {{baseUrl}}/api/Report/trend-analysis`

### 8. Create Policy
- **Method:** POST
- **URL:** `{{baseUrl}}/api/policy`
- **Body:**
  ```json
  {
    "offerId": {{offerId}},
    "startDate": "2024-01-01",
    "endDate": "2025-01-01",
    "policyNumber": "POL-2024-001"
  }
  ```

### 9. Get All Policies
- **Method:** GET
- **URL:** `{{baseUrl}}/api/policy`

## 🔧 Environment Variables

### Manuel Güncelleme:
1. **Login/Register sonrası:** Response'dan `token` değerini kopyalayıp `authToken` variable'ına yapıştırın
2. **Admin Login sonrası:** Response'dan `token` değerini kopyalayıp `adminToken` variable'ına yapıştırın
3. **Get Current User sonrası:** Response'dan `id` değerini kopyalayıp `userId` variable'ına yapıştırın
4. **Create Customer sonrası:** Response'dan `id` değerini kopyalayıp `customerId` variable'ına yapıştırın
5. **Create Offer sonrası:** Response'dan `id` değerini kopyalayıp `offerId` variable'ına yapıştırın
6. **Create Policy sonrası:** Response'dan `id` değerini kopyalayıp `policyId` variable'ına yapıştırın

### Variables:
- `{{baseUrl}}` = http://localhost:5000
- `{{authToken}}` = JWT token (manuel ekleyin)
- `{{adminToken}}` = Admin JWT token (manuel ekleyin)
- `{{userId}}` = Kullanıcı ID (manuel ekleyin)
- `{{customerId}}` = Müşteri ID (manuel ekleyin)
- `{{offerId}}` = Teklif ID (manuel ekleyin)
- `{{policyId}}` = Poliçe ID (manuel ekleyin)

## 🔑 Token Kullanımı

### Admin Token:
- **Agent Management** modülündeki tüm endpoint'ler için `{{adminToken}}` kullanın
- **File Upload** modülündeki tüm endpoint'ler için `{{adminToken}}` kullanın  
- **Reporting** modülündeki tüm endpoint'ler için `{{adminToken}}` kullanın

### Normal Token:
- **Customer Management** modülündeki endpoint'ler için `{{authToken}}` kullanın
- **Production Module** (Offer) endpoint'leri için `{{authToken}}` kullanın

### Public Endpoints:
- **Authentication** modülündeki Login ve Register endpoint'leri token gerektirmez
- **File Upload > Get Supported Formats** endpoint'i token gerektirmez

## ⚠️ Önemli Notlar

1. **Authentication:** Tüm testler JWT token gerektirir
2. **Token Yönetimi:** Login/Register sonrası token'ı manuel olarak environment'a ekleyin
3. **Variable Güncelleme:** Her response'dan ID'leri manuel olarak kopyalayıp ilgili variable'a yapıştırın
4. **Test Sırası:** Yukarıdaki sırayı takip edin
5. **Backend Çalışıyor:** Import öncesi backend'in çalıştığından emin olun

## 🎯 Beklenen Sonuçlar

- **200 OK:** Başarılı GET istekleri
- **201 Created:** Başarılı POST istekleri
- **400 Bad Request:** Geçersiz veri
- **401 Unauthorized:** Geçersiz token
- **404 Not Found:** Bulunamayan kaynak

## 🧪 Test Sırası

### 1. Admin Login
- **Endpoint:** `POST {{baseUrl}}/api/Auth/login`
- **Body:** `{"email": "admin@insurance.com", "password": "Admin123!"}`
- **Sonra:** Response'dan token'ı kopyalayıp `adminToken` variable'ına yapıştırın

### 2. Normal User Login/Register
- **Endpoint:** `POST {{baseUrl}}/api/Auth/login` veya `POST {{baseUrl}}/api/Auth/register/customer`
- **Sonra:** Response'dan token'ı kopyalayıp `authToken` variable'ına yapıştırın

### 3. Test Modüller
- **Agent Management:** Admin token ile test edin
- **File Upload:** Admin token ile test edin
- **Reporting:** Admin token ile test edin
- **Customer Management:** Normal token ile test edin

## 🔗 Swagger UI

API dokümantasyonu için: http://localhost:5000/swagger

---

**Not:** Bu kılavuz, Postman'in resmi schema formatı ile uyumlu dosyalar için hazırlanmıştır.
