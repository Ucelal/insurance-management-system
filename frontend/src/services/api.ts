import axios, { AxiosInstance, AxiosResponse } from 'axios';
import { LoginDto, RegisterDto, AuthResponse, User, Customer, Policy, Claim } from '../types';

// Backend API base URL
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
      timeout: 10000,
    });

    // Request interceptor to add auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        console.log('🔑 API Interceptor: Token from localStorage:', token);
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
          console.log('✅ API Interceptor: Authorization header added:', `Bearer ${token}`);
        } else {
          console.log('⚠️ API Interceptor: No token found in localStorage');
        }
        console.log('🌐 API Interceptor: Request config:', config);
        return config;
      },
      (error) => {
        console.error('❌ API Interceptor: Request error:', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor to handle auth errors
    this.api.interceptors.response.use(
      (response) => {
        console.log('✅ API Interceptor: Response received:', response.status, response.config.url);
        return response;
      },
      (error) => {
        console.error('❌ API Interceptor: Response error:', error.response?.status, error.response?.data, error.config?.url);
        if (error.response?.status === 401) {
          console.log('🚫 API Interceptor: 401 Unauthorized, clearing auth data');
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          // Don't redirect automatically, let components handle auth errors
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth endpoints
  async login(loginData: LoginDto): Promise<AuthResponse> {
    try {
      const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/login', loginData);
      console.log('Login response:', response.data); // Debug log
      console.log('User role from response:', response.data.user?.role); // Debug log
      console.log('Full user object:', response.data.user); // Debug log
      return response.data;
    } catch (error: any) {
      console.error('Login error:', error.response?.data || error.message); // Debug log
      throw new Error(error.response?.data?.message || 'Giriş başarısız');
    }
  }

  async register(registerData: RegisterDto): Promise<AuthResponse> {
    try {
      const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/register', registerData);
      console.log('Register response:', response.data); // Debug log
      return response.data;
    } catch (error: any) {
      console.error('Register error:', error.response?.data || error.message); // Debug log
      throw new Error(error.response?.data?.message || 'Kayıt başarısız');
    }
  }

  async registerCustomer(customerData: any): Promise<AuthResponse> {
    try {
      const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/register/customer', customerData);
      console.log('Customer register response:', response.data); // Debug log
      return response.data;
    } catch (error: any) {
      console.error('Customer register error:', error.response?.data || error.message); // Debug log
      throw new Error(error.response?.data?.message || 'Müşteri kaydı başarısız');
    }
  }

  async registerAdmin(adminData: any): Promise<AuthResponse> {
    try {
      console.log('📤 API: Sending admin registration data:', adminData);
      const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/register/admin', adminData);
      console.log('✅ API: Admin register response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Admin register error:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Admin kaydı başarısız');
    }
  }

  async getCurrentUser(): Promise<User> {
    try {
      const response: AxiosResponse<User> = await this.api.get('/auth/me');
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Kullanıcı bilgileri alınamadı');
    }
  }

  // Offer endpoints
  async createOffer(offerData: any): Promise<any> {
    try {
      const response: AxiosResponse<any> = await this.api.post('/Offer', offerData);
      console.log('Create offer response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('Create offer error:', error.response?.data || error.message);
      throw new Error(error.response?.data?.message || 'Teklif oluşturulamadı');
    }
  }

  // Customer endpoints
  async getCustomers(): Promise<Customer[]> {
    try {
      console.log('🔍 API: Fetching customers from /Customer');
      const response: AxiosResponse<Customer[]> = await this.api.get('/Customer');
      console.log('✅ API: Customers response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching customers:', error.response?.status, error.response?.data);
      throw new Error('Müşteri listesi alınamadı');
    }
  }

  // Agent endpoints
  async getAgents(): Promise<any[]> {
    try {
      console.log('🔍 API: Fetching agents from /Agent');
      const response: AxiosResponse<any[]> = await this.api.get('/Agent');
      console.log('✅ API: Agents response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching agents:', error.response?.status, error.response?.data);
      throw new Error('Acenta listesi alınamadı');
    }
  }

  async getAgentByUserId(userId: number): Promise<any> {
    try {
      const response: AxiosResponse<any> = await this.api.get(`/Agent/user/${userId}`);
      return response.data;
    } catch (error: any) {
      throw new Error('Acenta bilgileri alınamadı');
    }
  }

  async updateAgent(agentId: number, updateData: any): Promise<any> {
    try {
      console.log(`✏️ API: Updating agent ${agentId}`, updateData);
      const response = await this.api.put(`/Agent/${agentId}`, updateData);
      console.log('✅ API: Agent updated successfully');
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error updating agent:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Acenta güncellenemedi');
    }
  }

  async deleteAgent(agentId: number): Promise<void> {
    try {
      console.log(`🗑️ API: Deleting agent ${agentId}`);
      await this.api.delete(`/Agent/${agentId}`);
      console.log('✅ API: Agent deleted successfully');
    } catch (error: any) {
      console.error('❌ API: Error deleting agent:', error.response?.status, error.response?.data);
      throw new Error('Acenta silinemedi');
    }
  }

  async updateCustomer(customerId: number, updateData: any): Promise<any> {
    try {
      console.log(`✏️ API: Updating customer ${customerId}`, updateData);
      const response = await this.api.put(`/Customer/${customerId}`, updateData);
      console.log('✅ API: Customer updated successfully');
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error updating customer:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Müşteri güncellenemedi');
    }
  }

  async deleteCustomer(customerId: number): Promise<void> {
    try {
      console.log(`🗑️ API: Deleting customer ${customerId}`);
      await this.api.delete(`/Customer/${customerId}`);
      console.log('✅ API: Customer deleted successfully');
    } catch (error: any) {
      console.error('❌ API: Error deleting customer:', error.response?.status, error.response?.data);
      throw new Error('Müşteri silinemedi');
    }
  }

  async getOffersByAgentDepartment(agentId: number): Promise<any[]> {
    try {
      const response: AxiosResponse<any[]> = await this.api.get(`/Agent/${agentId}/offers`);
      return response.data;
    } catch (error: any) {
      throw new Error('Departman teklifleri alınamadı');
    }
  }

  async getOffersByAgent(agentId: number): Promise<any[]> {
    try {
      console.log('🔍 API: Fetching offers for agent:', agentId);
      const response: AxiosResponse<any[]> = await this.api.get(`/Agent/${agentId}/offers`);
      console.log('✅ API: Agent offers response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching agent offers:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Agent teklifleri alınamadı');
    }
  }

  async getPendingOffersByAgentDepartment(agentId: number): Promise<any[]> {
    try {
      const response: AxiosResponse<any[]> = await this.api.get(`/Agent/${agentId}/offers/pending`);
      return response.data;
    } catch (error: any) {
      throw new Error('Bekleyen teklifler alınamadı');
    }
  }

  async updateOffer(offerId: number, updateData: any): Promise<any> {
    try {
      const response: AxiosResponse<any> = await this.api.put(`/Offer/${offerId}`, updateData);
      return response.data;
    } catch (error: any) {
      throw new Error('Teklif güncellenemedi');
    }
  }

  async uploadPolicyPdf(offerId: number, pdfFile: File): Promise<any> {
    try {
      console.log('📄 API: Uploading policy PDF for offer:', offerId);
      const formData = new FormData();
      formData.append('file', pdfFile);
      formData.append('offerId', offerId.toString());
      
      const response: AxiosResponse<any> = await this.api.post(`/Document/upload-policy-pdf`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      console.log('✅ API: Policy PDF uploaded successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error uploading policy PDF:', error.response?.status, error.response?.data);
      throw new Error('Poliçe PDF yüklenemedi');
    }
  }

  // Offer endpoints
  async getOffers(): Promise<any[]> {
    try {
      console.log('🔍 API: Fetching offers from /Offer');
      const response: AxiosResponse<any[]> = await this.api.get('/Offer');
      console.log('✅ API: Offers response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching offers:', error.response?.status, error.response?.data);
      throw new Error('Teklif listesi alınamadı');
    }
  }

  async deleteOffer(id: number): Promise<void> {
    try {
      console.log('🗑️ API: Deleting offer:', id);
      const response = await this.api.delete(`/Offer/${id}`);
      console.log('✅ API: Offer deleted successfully:', response.data);
    } catch (error: any) {
      console.error('❌ API: Error deleting offer:', error.response?.status, error.response?.data);
      throw new Error('Teklif silinemedi');
    }
  }

  async getOffersByCustomer(customerId: number): Promise<any[]> {
    try {
      console.log('🔍 API: Fetching offers for customer:', customerId);
      const response: AxiosResponse<any[]> = await this.api.get(`/Offer/customer/${customerId}`);
      console.log('✅ API: Customer offers response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching customer offers:', error.response?.status, error.response?.data);
      throw new Error('Müşteri teklifleri alınamadı');
    }
  }

  async updateOfferApproval(offerId: number, approved: boolean): Promise<any> {
    try {
      console.log('✅ API: Updating offer approval:', offerId, approved);
      const response: AxiosResponse<any> = await this.api.put(`/Offer/${offerId}/approval`, { 
        isCustomerApproved: approved,
        customerApprovedAt: approved ? new Date().toISOString() : null
      });
      console.log('✅ API: Offer approval updated successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error updating offer approval:', error.response?.status, error.response?.data);
      throw new Error('Teklif onayı güncellenemedi');
    }
  }

  async createPolicyFromPayment(offerId: number, paymentData: any): Promise<any> {
    try {
      console.log('💳 API: Creating policy from payment for offer:', offerId);
      const response: AxiosResponse<any> = await this.api.post(`/Offer/${offerId}/create-policy`, {
        paymentAmount: paymentData.paymentAmount,
        paymentMethod: paymentData.paymentMethod,
        transactionId: paymentData.transactionId,
        cardLast4: paymentData.cardLast4
      });
      console.log('✅ API: Policy created successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error creating policy from payment:', error.response?.status, error.response?.data);
      throw new Error('Poliçe oluşturulamadı');
    }
  }

  async createPaymentReceiptPdf(receiptData: any): Promise<string> {
    try {
      console.log('📄 API: Creating payment receipt PDF');
      const response: AxiosResponse<{ success: boolean; pdfUrl: string }> = await this.api.post('/Document/create-payment-receipt-pdf', receiptData);
      console.log('✅ API: Payment receipt PDF created:', response.data);
      return response.data.pdfUrl;
    } catch (error: any) {
      console.error('❌ API: Error creating payment receipt PDF:', error.response?.status, error.response?.data);
      throw new Error('Ödeme makbuzu oluşturulamadı');
    }
  }

  async getMyDocuments(): Promise<any[]> {
    try {
      console.log('📄 API: Fetching my documents from /Document/my-documents');
      const response: AxiosResponse<any[]> = await this.api.get('/Document/my-documents');
      console.log('✅ API: My documents response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching my documents:', error.response?.status, error.response?.data);
      throw new Error('Doküman listesi alınamadı');
    }
  }

  async getDocuments(): Promise<any[]> {
    try {
      console.log('📄 API: Fetching all documents from /Document');
      const response: AxiosResponse<any[]> = await this.api.get('/Document');
      console.log('✅ API: Documents response:', response.data?.length);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching documents:', error.response?.status, error.response?.data);
      throw new Error('Doküman listesi alınamadı');
    }
  }

  async deleteDocument(documentId: number): Promise<void> {
    try {
      console.log('🗑️ API: Deleting my claim document:', documentId);
      await this.api.delete(`/Document/my-claim-documents/${documentId}`);
      console.log('✅ API: My claim document deleted successfully');
    } catch (error: any) {
      console.error('❌ API: Error deleting my claim document:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Belge silinemedi');
    }
  }

  async getCustomerById(id: number): Promise<Customer> {
    try {
      const response: AxiosResponse<Customer> = await this.api.get(`/Customer/${id}`);
      return response.data;
    } catch (error: any) {
      throw new Error('Müşteri bilgileri alınamadı');
    }
  }

  async createCustomer(customerData: any): Promise<Customer> {
    try {
      const response: AxiosResponse<Customer> = await this.api.post('/Customer', customerData);
      return response.data;
    } catch (error: any) {
      throw new Error('Müşteri oluşturulamadı');
    }
  }

  // Policy endpoints
  async getPolicies(): Promise<Policy[]> {
    try {
      const response: AxiosResponse<Policy[]> = await this.api.get('/Policy');
      return response.data;
    } catch (error: any) {
      throw new Error('Poliçe listesi alınamadı');
    }
  }

  async getPolicyById(id: number): Promise<Policy> {
    try {
      const response: AxiosResponse<Policy> = await this.api.get(`/Policy/${id}`);
      return response.data;
    } catch (error: any) {
      throw new Error('Poliçe bilgileri alınamadı');
    }
  }

  async createPolicy(policyData: any): Promise<Policy> {
    try {
      const response: AxiosResponse<Policy> = await this.api.post('/Policy', policyData);
      return response.data;
    } catch (error: any) {
      throw new Error('Poliçe oluşturulamadı');
    }
  }

  async getMyPolicies(): Promise<Policy[]> {
    try {
      console.log('🔍 API: Fetching my policies from /Policy/my-policies');
      const response: AxiosResponse<Policy[]> = await this.api.get('/Policy/my-policies');
      console.log('✅ API: My policies response:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching my policies:', error.response?.status, error.response?.data);
      throw new Error('Poliçe listesi alınamadı');
    }
  }

  // Claim endpoints
  async getClaims(): Promise<Claim[]> {
    try {
      const response: AxiosResponse<Claim[]> = await this.api.get('/Claim');
      return response.data;
    } catch (error: any) {
      throw new Error('Talep listesi alınamadı');
    }
  }

  async createClaim(claimData: any): Promise<Claim> {
    try {
      console.log('📝 API: Creating claim:', claimData);
      const response: AxiosResponse<Claim> = await this.api.post('/Claim', claimData);
      console.log('✅ API: Claim created successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error creating claim:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Talep oluşturulamadı');
    }
  }

  // Müşterinin kendi claim'lerini getir
  async getMyClaims(): Promise<Claim[]> {
    try {
      console.log('📝 API: Getting my claims');
      const response: AxiosResponse<Claim[]> = await this.api.get('/Claim/my-claims');
      console.log('✅ API: My claims fetched successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching my claims:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Hasar bildirimleri alınamadı');
    }
  }

  // Agent'ın departmanına ait claim'leri getir
  async getClaimsByAgentDepartment(agentId: number): Promise<Claim[]> {
    try {
      console.log('📝 API: Getting department claims for agent:', agentId);
      const response: AxiosResponse<Claim[]> = await this.api.get(`/Agent/${agentId}/department-claims`);
      console.log('✅ API: Department claims fetched successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error fetching department claims:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Departman olay bildirimleri alınamadı');
    }
  }

  // Admin/Agent claim güncelleme
  async updateClaim(claimId: number, updateData: any): Promise<Claim> {
    try {
      console.log('📝 API: Updating claim:', claimId, updateData);
      const response: AxiosResponse<Claim> = await this.api.put(`/Claim/${claimId}`, updateData);
      console.log('✅ API: Claim updated successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error updating claim:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Hasar bildirimi güncellenemedi');
    }
  }

  // Müşterinin kendi pending claim'ini güncelle
  async updateMyClaim(claimId: number, updateData: any): Promise<Claim> {
    try {
      console.log('📝 API: Updating my claim:', claimId, updateData);
      const response: AxiosResponse<Claim> = await this.api.put(`/Claim/my-claims/${claimId}`, updateData);
      console.log('✅ API: My claim updated successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error updating my claim:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Hasar bildirimi güncellenemedi');
    }
  }

  // Müşterinin kendi pending claim'ini sil
  async deleteMyClaim(claimId: number): Promise<void> {
    try {
      console.log('📝 API: Deleting my claim:', claimId);
      await this.api.delete(`/Claim/my-claims/${claimId}`);
      console.log('✅ API: My claim deleted successfully');
    } catch (error: any) {
      console.error('❌ API: Error deleting my claim:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Hasar bildirimi silinemedi');
    }
  }

  // Olay Formu için belge yükleme
  async uploadIncidentDocument(file: File, claimId: number): Promise<any> {
    try {
      console.log('📄 API: Uploading incident document:', file.name, 'for claim:', claimId);
      
      const formData = new FormData();
      formData.append('file', file);
      formData.append('claimId', claimId.toString());
      
      const response: AxiosResponse<any> = await this.api.post('/Document/upload-incident-document', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      
      console.log('✅ API: Incident document uploaded successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error uploading incident document:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'Belge yüklenemedi');
    }
  }

  // Olay Formu için çoklu belge yükleme
  async uploadIncidentDocuments(files: File[], claimId: number): Promise<any[]> {
    try {
      console.log('📄 API: Uploading multiple incident documents:', files.length, 'files for claim:', claimId);
      
      const uploadPromises = files.map(file => this.uploadIncidentDocument(file, claimId));
      const results = await Promise.all(uploadPromises);
      
      console.log('✅ API: All incident documents uploaded successfully:', results);
      return results;
    } catch (error: any) {
      console.error('❌ API: Error uploading incident documents:', error);
      throw new Error('Belgeler yüklenemedi');
    }
  }

  // PDF dosya yükleme (Teklif formu için)
  async uploadPdf(file: File): Promise<any> {
    try {
      console.log('📄 API: Uploading PDF file:', file.name);
      
      const formData = new FormData();
      formData.append('file', file);
      
      const response: AxiosResponse<any> = await this.api.post('/Document/upload-pdf', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      
      console.log('✅ API: PDF uploaded successfully:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ API: Error uploading PDF:', error.response?.status, error.response?.data);
      throw new Error(error.response?.data?.message || 'PDF yüklenemedi');
    }
  }

  // Quote endpoints
  async getQuote(quoteData: any): Promise<any> {
    try {
      const response: AxiosResponse<any> = await this.api.post('/Offer', quoteData);
      return response.data;
    } catch (error: any) {
      throw new Error('Teklif alınamadı');
    }
  }

  // Health check
  async healthCheck(): Promise<boolean> {
    try {
      await this.api.get('/health');
      return true;
    } catch {
      return false;
    }
  }

  // Logout - Token'ı blacklist'e ekle
  async logout(): Promise<void> {
    try {
      await this.api.post('/auth/logout');
      console.log('✅ Logout API call successful');
    } catch (error: any) {
      console.error('❌ Logout API error:', error.response?.data || error.message);
      throw new Error(error.response?.data?.message || 'Çıkış işlemi başarısız');
    }
  }

  // Profile endpoints
  async getMyProfile(): Promise<any> {
    try {
      const response: AxiosResponse<any> = await this.api.get('/profile/me');
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Profil bilgileri alınamadı');
    }
  }

  async updateMyProfile(profileData: any): Promise<any> {
    try {
      const response: AxiosResponse<any> = await this.api.put('/profile/me', profileData);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Profil güncellenemedi');
    }
  }

  async changePassword(passwordData: any): Promise<any> {
    try {
      const response: AxiosResponse<any> = await this.api.put('/profile/me/password', passwordData);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Şifre değiştirilemedi');
    }
  }

  async updateCustomerProfile(customerData: any): Promise<any> {
    try {
      const response: AxiosResponse<any> = await this.api.put('/profile/me/customer', customerData);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Müşteri bilgileri güncellenemedi');
    }
  }

  async updateAgentProfile(agentData: any): Promise<any> {
    try {
      const response: AxiosResponse<any> = await this.api.put('/profile/me/agent', agentData);
      return response.data;
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Agent bilgileri güncellenemedi');
    }
  }
}

export const apiService = new ApiService();
export default apiService; 