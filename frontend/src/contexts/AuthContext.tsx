import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, AuthResponse } from '../types';
import apiService from '../services/api';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  token: string | null;
  login: (email: string, password: string, role?: 'admin' | 'agent' | 'customer') => Promise<boolean>;
  register: (userData: any) => Promise<boolean>;
  logout: () => void;
  updateUser: (user: User) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = async () => {
      const storedToken = localStorage.getItem('token');
      const storedUser = localStorage.getItem('user');

      if (storedToken && storedUser) {
        try {
          // Try to get current user to validate token
          const currentUser = await apiService.getCurrentUser();
          if (currentUser) {
            setUser(currentUser);
            setToken(storedToken);
          } else {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            setToken(null);
          }
        } catch (error) {
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          setToken(null);
        }
      }
      setIsLoading(false);
    };

    initializeAuth();
  }, []);

  const login = async (email: string, password: string, role?: 'admin' | 'agent' | 'customer'): Promise<boolean> => {
    try {
      console.log('🔐 AuthContext: Login attempt for:', email);
      
      // Create login data object matching the backend LoginDto
      const loginData = { email, password };
      
      const response: AuthResponse = await apiService.login(loginData);
      console.log('✅ AuthContext: Login successful, response:', response);
      
      localStorage.setItem('token', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('expiresAt', response.expiresAt);
      localStorage.setItem('user', JSON.stringify(response.user));
      
      console.log('💾 AuthContext: Token stored:', response.token);
      console.log('👤 AuthContext: User stored:', response.user);
      
      // Update user state immediately
      setUser(response.user);
      setToken(response.token);
      
      console.log('🔄 AuthContext: State updated');
      
      return true;
    } catch (error) {
      console.error('❌ AuthContext: Login error:', error);
      // Re-throw the error so the calling component can handle it properly
      throw error;
    }
  };

  const register = async (userData: any): Promise<boolean> => {
    try {
      // Use customer register endpoint for customer registration
      const response: AuthResponse = await apiService.registerCustomer(userData);
      
      localStorage.setItem('token', response.token);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('expiresAt', response.expiresAt);
      localStorage.setItem('user', JSON.stringify(response.user));
      setUser(response.user);
      setToken(response.token);
      return true;
    } catch (error) {
      console.error('Register error:', error);
      // Re-throw the error so the calling component can handle it properly
      throw error;
    }
  };

  const logout = async () => {
    try {
      // Backend'e logout isteği gönder ve token'ı blacklist'e ekle
      if (token) {
        await apiService.logout();
        console.log('✅ Logout successful - token blacklisted');
      }
    } catch (error) {
      console.error('❌ Logout error:', error);
      // Logout başarısız olsa bile local storage'ı temizle
    } finally {
      // Local storage'ı temizle
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('expiresAt');
      localStorage.removeItem('user');
      setUser(null);
      setToken(null);
      console.log('🧹 Local storage cleared');
    }
  };

  const updateUser = (updatedUser: User) => {
    setUser(updatedUser);
    localStorage.setItem('user', JSON.stringify(updatedUser));
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    token,
    login,
    register,
    logout,
    updateUser,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}; 