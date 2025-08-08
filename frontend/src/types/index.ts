export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
  createdAt: string;
  customer?: Customer;
}

export interface Customer {
  id: number;
  userId?: number;
  type: string;
  idNo: string;
  address?: string;
  phone?: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  role: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface ApiResponse<T> {
  data?: T;
  message?: string;
  success: boolean;
} 