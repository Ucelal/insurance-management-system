import React, { useState } from 'react';
import {
  Box,
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import { Visibility, VisibilityOff } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import Header from './Header';
import { useAuth } from '../contexts/AuthContext';

const CustomerRegister: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const navigate = useNavigate();
  const { register, user } = useAuth();

  const [formData, setFormData] = useState({
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    tcNo: '',
    type: '',
    address: '',
    phone: '',
  });

  const customerTypes = [
    { value: 'bireysel', label: 'Bireysel' },
    { value: 'kurumsal', label: 'Kurumsal' },
  ];

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFormChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    setError(null);
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);

    try {
      // Validation
      if (!formData.name || !formData.email || !formData.password || 
          !formData.confirmPassword || !formData.tcNo || !formData.type || !formData.address || !formData.phone) {
        throw new Error('Lütfen tüm alanları doldurun');
      }

      if (formData.password !== formData.confirmPassword) {
        throw new Error('Şifreler eşleşmiyor');
      }

      if (formData.password.length < 6) {
        throw new Error('Şifre en az 6 karakter olmalıdır');
      }

      // Password complexity validation removed for testing
      // TODO: Re-enable when database passwords are updated to include letters and numbers

      // Validate email format
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(formData.email)) {
        throw new Error('Geçerli bir e-posta adresi girin');
      }

      // Validate phone number (basic validation)
      if (formData.phone.length < 10) {
        throw new Error('Geçerli bir telefon numarası girin');
      }

      // Validate name (at least 2 characters)
      if (formData.name.trim().length < 2) {
        throw new Error('Ad Soyad en az 2 karakter olmalıdır');
      }

      // Validate TC No (11 digits)
      if (!/^\d{11}$/.test(formData.tcNo)) {
        throw new Error('TC Kimlik No 11 haneli olmalıdır');
      }

      // Validate address (at least 10 characters)
      if (formData.address.trim().length < 10) {
        throw new Error('Adres en az 10 karakter olmalıdır');
      }

      // Use real API service for customer registration
      const success = await register({
        name: formData.name,
        email: formData.email,
        password: formData.password,
        confirmPassword: formData.confirmPassword,
        tcNo: formData.tcNo,
        phone: formData.phone,
        address: formData.address,
        customerType: formData.type,
      });
      
      if (success) {
        // Get user role from context to determine redirect
        const userRole = user?.role?.toLowerCase();
        console.log('Registration successful, user role:', userRole);
        
        // Redirect based on user role
        switch (userRole) {
          case 'admin':
            navigate('/admin-dashboard');
            break;
          case 'agent':
            navigate('/agent-dashboard');
            break;
          case 'customer':
          default:
            navigate('/customer-dashboard');
            break;
        }
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kayıt başarısız');
    } finally {
      setIsLoading(false);
    }
  };

  const handleLoginClick = () => {
    navigate('/customer-login');
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
          justifyContent: 'center',
          py: 4,
        }}
      >
        <Container maxWidth="md">
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
                Üye Kaydı                                                                                                                                                                                                                               
              </Typography>
              <Typography
                variant="h6"
                sx={{
                  opacity: 0.9,
                  fontWeight: 400,
                }}
              >
                Yeni hesap oluşturun
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

            {/* Registration Form */}
            <Box component="form" onSubmit={handleRegister} sx={{ p: 3 }}>
              <TextField
                fullWidth
                label="Ad Soyad"
                value={formData.name}
                onChange={(e) => handleFormChange('name', e.target.value)}
                margin="normal"
                required
                variant="outlined"
                sx={{ mb: 2 }}
                disabled={isLoading}
              />

              <TextField
                fullWidth
                label="E-posta"
                type="email"
                value={formData.email}
                onChange={(e) => handleFormChange('email', e.target.value)}
                margin="normal"
                required
                variant="outlined"
                sx={{ mb: 2 }}
                disabled={isLoading}
              />

              <TextField
                fullWidth
                label="Şifre"
                type={showPassword ? 'text' : 'password'}
                value={formData.password}
                onChange={(e) => handleFormChange('password', e.target.value)}
                margin="normal"
                required
                variant="outlined"
                sx={{ mb: 2 }}
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

              <TextField
                fullWidth
                label="Şifre Tekrar"
                type={showConfirmPassword ? 'text' : 'password'}
                value={formData.confirmPassword}
                onChange={(e) => handleFormChange('confirmPassword', e.target.value)}
                margin="normal"
                required
                variant="outlined"
                sx={{ mb: 2 }}
                disabled={isLoading}
                InputProps={{
                  endAdornment: (
                    <Button
                      type="button"
                      onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                      sx={{ minWidth: 'auto', p: 1 }}
                    >
                      {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                    </Button>
                  ),
                }}
              />

              <TextField
                fullWidth
                label="TC Kimlik No"
                value={formData.tcNo}
                onChange={(e) => handleFormChange('tcNo', e.target.value)}
                margin="normal"
                required
                variant="outlined"
                inputProps={{ maxLength: 11 }}
                sx={{ mb: 2 }}
                disabled={isLoading}
                placeholder="11 haneli TC Kimlik No"
              />

              <FormControl fullWidth margin="normal" sx={{ mb: 2 }}>
                <InputLabel>Müşteri Tipi</InputLabel>
                <Select
                  value={formData.type}
                  onChange={(e) => handleFormChange('type', e.target.value)}
                  label="Müşteri Tipi"
                  disabled={isLoading}
                >
                  {customerTypes.map((type) => (
                    <MenuItem key={type.value} value={type.value}>
                      {type.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <TextField
                fullWidth
                label="Adres"
                value={formData.address}
                onChange={(e) => handleFormChange('address', e.target.value)}
                margin="normal"
                required
                variant="outlined"
                multiline
                rows={3}
                sx={{ mb: 2 }}
                disabled={isLoading}
              />

              <TextField
                fullWidth
                label="Telefon"
                value={formData.phone}
                onChange={(e) => handleFormChange('phone', e.target.value)}
                margin="normal"
                required
                variant="outlined"
                sx={{ mb: 3 }}
                disabled={isLoading}
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
                {isLoading ? 'Kayıt Yapılıyor...' : 'Üye Ol'}
              </Button>

              <Button
                fullWidth
                variant="outlined"
                size="large"
                onClick={handleLoginClick}
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
                Zaten Hesabınız Var mı? Giriş Yapın
              </Button>
            </Box>
          </Paper>
        </Container>
      </Box>
    </Box>
  );
};

export default CustomerRegister;
