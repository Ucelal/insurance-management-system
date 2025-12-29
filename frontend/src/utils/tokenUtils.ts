// Token utility functions
export interface TokenInfo {
  role: string;
  expiresAt: Date;
  remainingTime: string;
  isExpired: boolean;
}

export const getTokenInfo = (token: string): TokenInfo | null => {
  try {
    // JWT token'ı decode et
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    const payload = JSON.parse(jsonPayload);
    const expiresAt = new Date(payload.exp * 1000);
    const now = new Date();
    const remainingMs = expiresAt.getTime() - now.getTime();
    
    // Role'e göre beklenen süre
    const expectedHours = payload.role?.toLowerCase() === 'customer' ? 1 : 4;
    
    return {
      role: payload.role || 'unknown',
      expiresAt,
      remainingTime: formatRemainingTime(remainingMs),
      isExpired: remainingMs <= 0
    };
  } catch (error) {
    console.error('Token decode error:', error);
    return null;
  }
};

export const formatRemainingTime = (ms: number): string => {
  if (ms <= 0) return 'Süresi dolmuş';
  
  const hours = Math.floor(ms / (1000 * 60 * 60));
  const minutes = Math.floor((ms % (1000 * 60 * 60)) / (1000 * 60));
  
  if (hours > 0) {
    return `${hours} saat ${minutes} dakika`;
  } else {
    return `${minutes} dakika`;
  }
};

export const getExpectedTokenDuration = (role: string): string => {
  switch (role?.toLowerCase()) {
    case 'admin':
    case 'agent':
      return '4 saat';
    case 'customer':
      return '1 saat';
    default:
      return '1 saat';
  }
};
