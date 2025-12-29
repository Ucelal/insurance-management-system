import React, { useState } from 'react';
import {
  Box,
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Alert,
  CircularProgress,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import { Visibility, VisibilityOff } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import Header from './Header';
import { useAuth } from '../contexts/AuthContext';

const CustomerLogin: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const navigate = useNavigate();
  const { login, user } = useAuth();

  const [formData, setFormData] = useState({
    email: '',
    password: '',
  });

  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    setError(null);
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      // Check if form is valid
      if (!formData.email || !formData.password) {
        throw new Error('Lütfen tüm alanları doldurun');
      }

      // Validate email format
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(formData.email)) {
        throw new Error('Geçerli bir e-posta adresi girin');
      }

      // Validate password length
      if (formData.password.length < 6) {
        throw new Error('Şifre en az 6 karakter olmalıdır');
      }

      // Password complexity validation removed for testing
      // TODO: Re-enable when database passwords are updated to include letters and numbers

      // Use real API service for authentication
      const success = await login(formData.email, formData.password);
      
      if (success) {
        // Get user role from the response directly
        const storedUser = localStorage.getItem('user');
        console.log('Stored user data:', storedUser); // Debug log
        
        if (storedUser) {
          const userData = JSON.parse(storedUser);
          console.log('Parsed user data:', userData); // Debug log
          console.log('User role type:', typeof userData.role); // Debug log
          console.log('User role value:', userData.role); // Debug log
          console.log('User role lowercase:', userData.role?.toLowerCase()); // Debug log
          
          const userRole = userData.role?.toLowerCase();
          console.log('Login successful, user role:', userRole);
          
          // Redirect based on user role
          switch (userRole) {
            case 'admin':
              console.log('Redirecting to admin dashboard'); // Debug log
              navigate('/admin-dashboard');
              break;
            case 'agent':
              console.log('Redirecting to agent dashboard'); // Debug log
              navigate('/agent-dashboard');
              break;
            case 'customer':
            default:
              console.log('Redirecting to customer dashboard'); // Debug log
              navigate('/customer-dashboard');
              break;
          }
        } else {
          console.log('No stored user data found'); // Debug log
        }
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Giriş başarısız');
    } finally {
      setIsLoading(false);
    }
  };

  const handleRegisterClick = () => {
    navigate('/customer-register');
  };

  return (
    <Box sx={{ minHeight: '100vh' }}>
      <Header />
      <Box
        sx={{
          minHeight: 'calc(100vh - 64px)',
          background: theme.palette.mode === 'dark'
            ? 'linear-gradient(135deg, #0d47a1 0%, #1565c0 50%, #1976d2 100%)'
            : 'linear-gradient(135deg, #e3f2fd 0%, #bbdefb 50%, #90caf9 100%)',
          display: 'flex',
          alignItems: 'center',
          py: 4,
        }}
      >
        <Container maxWidth="sm">
          <Paper
            elevation={24}
            sx={{
              borderRadius: 4,
              overflow: 'hidden',
              background: theme.palette.mode === 'dark' ? 'rgba(0, 0, 0, 0.8)' : 'rgba(255, 255, 255, 0.95)',
              backdropFilter: 'blur(20px)',
              border: `1px solid ${theme.palette.mode === 'dark' ? 'rgba(255, 255, 255, 0.1)' : 'rgba(0, 0, 0, 0.1)'}`,
            }}
          >
            {/* Header */}
            <Box
              sx={{
                background: theme.palette.mode === 'dark'
                  ? 'linear-gradient(135deg, #1976d2 0%, #1565c0 100%)'
                  : 'linear-gradient(135deg, #2196f3 0%, #1976d2 100%)',
                color: 'white',
                textAlign: 'center',
                py: 4,
                px: 3,
              }}
            >
              <Typography
                variant={isMobile ? 'h4' : 'h3'}
                component="h1"
                sx={{
                  fontWeight: 700,
                  mb: 1,
                  textShadow: '2px 2px 4px rgba(0,0,0,0.3)',
                }}
              >
                Üye Girişi
              </Typography>
              <Typography
                variant="h6"
                sx={{
                  opacity: 0.9,
                  fontWeight: 400,
                }}
              >
                Hesabınıza giriş yapın
              </Typography>
            </Box>

            {/* Error Alert */}
            {error && (
              <Box sx={{ px: 3, pt: 2 }}>
                <Alert severity="error" sx={{ borderRadius: 2 }}>
                  {error}
                </Alert>
              </Box>
            )}

            {/* Login Form */}
            <Box component="form" onSubmit={handleLogin} sx={{ p: 3 }}>
              <TextField
                fullWidth
                label="E-posta"
                name="email"
                type="email"
                value={formData.email}
                onChange={handleInputChange}
                margin="normal"
                required
                variant="outlined"
                sx={{ mb: 2 }}
                disabled={isLoading}
              />

              <TextField
                fullWidth
                label="Şifre"
                name="password"
                type={showPassword ? 'text' : 'password'}
                value={formData.password}
                onChange={handleInputChange}
                margin="normal"
                required
                variant="outlined"
                sx={{ mb: 3 }}
                disabled={isLoading}
                InputProps={{
                  endAdornment: (
                    <Button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      sx={{ minWidth: 'auto', p: 1 }}
                    >
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </Button>
                  ),
                }}
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={isLoading}
                sx={{
                  py: 1.5,
                  fontSize: '1.1rem',
                  fontWeight: 600,
                  borderRadius: 3,
                  mb: 2,
                  background: 'linear-gradient(135deg, #2196f3 0%, #1976d2 100%)',
                  '&:hover': {
                    background: 'linear-gradient(135deg, #1976d2 0%, #1565c0 100%)',
                  },
                }}
                startIcon={isLoading ? <CircularProgress size={20} /> : null}
              >
                {isLoading ? 'Giriş Yapılıyor...' : 'Giriş Yap'}
              </Button>

              <Button
                fullWidth
                variant="outlined"
                size="large"
                onClick={handleRegisterClick}
                disabled={isLoading}
                sx={{
                  py: 1.5,
                  fontSize: '1.1rem',
                  fontWeight: 600,
                  borderRadius: 3,
                  borderColor: theme.palette.primary.main,
                  color: theme.palette.primary.main,
                  '&:hover': {
                    borderColor: theme.palette.primary.dark,
                    backgroundColor: theme.palette.primary.main,
                    color: 'white',
                  },
                }}
              >
                Hesabınız Yok mu? Üye Olun
              </Button>
            </Box>
          </Paper>
        </Container>
      </Box>
    </Box>
  );
};

export default CustomerLogin;
