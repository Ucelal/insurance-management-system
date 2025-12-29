// User types
export interface User {
  id: number;
  name: string;
  email: string;
  role?: string;
  createdAt?: string;
  customer?: Customer;
}

export interface Customer {
  id: number;
  customerId: number; // Backend DTO property name
  userId?: number;
  idNo: string;
  address?: string;
  phone?: string;
  user?: User;
  // User bilgileri (backend'den gelen)
  userName?: string;
  userEmail?: string;
  userRole?: string;
}

export interface Agent {
  id: number;
  agentId: number; // Backend DTO property name
  userId: number;
  agentCode: string;
  department: string;
  address?: string;
  phone?: string;
  user?: User;
  // User bilgileri (backend'den gelen)
  userName?: string;
  userEmail?: string;
  userRole?: string;
}

// API DTOs - matching the API service
export interface LoginDto {
  email: string;
  password: string;
  role?: 'admin' | 'agent' | 'customer';
}

export interface RegisterDto {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  phone: string;
  address: string;
  customerType?: 'individual' | 'corporate';
  tcKimlik?: string;
  birthDate?: string;
  gender?: string;
  companyName?: string;
  taxNumber?: string;
  taxOffice?: string;
  companyType?: string;
  employeeCount?: string;
  establishmentDate?: string;
  contactPerson?: string;
  contactPhone?: string;
}

// API Response types
export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
  message?: string;
}

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success?: boolean;
}

// Insurance specific types
export interface Policy {
  id: number;
  customerId: number;
  serviceType: string;
  coverageAmount: number;
  premium: number;
  startDate: string;
  endDate: string;
  status: 'active' | 'expired' | 'cancelled';
}

export interface Claim {
  id: number;
  claimId: number; // Backend DTO property
  policyId: number;
  policyNumber: string;
  createdByUserId: number;
  createdByUserName: string;
  createdByUserEmail?: string;
  processedByUserId?: number;
  processedByUserName?: string;
  processedByUserEmail?: string;
  processedByUserPhone?: string;
  customerId?: number;
  description: string;
  status: string;
  type: string;
  approvedAmount?: number;
  incidentDate?: string;
  createdAt: string;
  processedAt?: string;
  notes?: string;
  amount?: number; // Legacy field
}

export interface Quote {
  id: number;
  customerId: number;
  serviceType: string;
  coverageAmount: number;
  estimatedPremium: number;
  createdAt: string;
  status: 'pending' | 'accepted' | 'rejected';
}

export interface Offer {
  id: number;
  offerId: number;
  customerId: number;
  agentId?: number;
  insuranceTypeId: number;
  description?: string;
  basePrice: number;
  discountRate?: number;
  finalPrice?: number;
  status: string;
  department: string;
  coverageAmount: number;
  requestedStartDate: string;
  customerAdditionalInfo?: string;
  validUntil?: string;
  isCustomerApproved?: boolean;
  customerApprovedAt?: string;
  reviewedAt?: string;
  reviewedByAgentId?: number;
  agentNotes?: string;
  policyPdfUrl?: string;
  createdAt: string;
  updatedAt?: string;
  processedAt?: string;
  customer?: Customer;
  insuranceType?: {
    insuranceTypeId: number;
    name: string;
    description?: string;
    category?: string;
    basePrice: number;
    isActive: boolean;
  };
} 