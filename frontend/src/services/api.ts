import axios, { AxiosInstance, AxiosResponse } from 'axios';
import { LoginDto, RegisterDto, AuthResponse, User } from '../types';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7001/api';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor to add auth token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor to handle auth errors
    this.api.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Auth endpoints
  async login(loginData: LoginDto): Promise<AuthResponse> {
    const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/login', loginData);
    return response.data;
  }

  async register(registerData: RegisterDto): Promise<AuthResponse> {
    const response: AxiosResponse<AuthResponse> = await this.api.post('/auth/register', registerData);
    return response.data;
  }

  async getCurrentUser(): Promise<User> {
    const response: AxiosResponse<User> = await this.api.get('/auth/me');
    return response.data;
  }

  async validateToken(token: string): Promise<boolean> {
    try {
      await this.api.post('/auth/validate', token);
      return true;
    } catch {
      return false;
    }
  }
}

export const apiService = new ApiService();
export default apiService; 