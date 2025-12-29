import React, { useState } from 'react';
import {
  Box,
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  useTheme,
  useMediaQuery,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Stepper,
  Step,
  StepLabel,
  StepContent,
  Alert,
  CircularProgress,
  Chip,
  Divider,
} from '@mui/material';
import {
  ArrowBack,
  Calculate,
  CheckCircle,
  TrendingUp,
  Shield,
  Phone,
  Email,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import Header from './Header';

interface QuoteForm {
  serviceType: string;
  customerType: string;
  name: string;
  email: string;
  phone: string;
  age: string;
  vehicleType?: string;
  vehicleYear?: string;
  homeType?: string;
  homeSize?: string;
  businessType?: string;
  employeeCount?: string;
  travelDestination?: string;
  travelDuration?: string;
  coverageAmount: string;
  additionalInfo: string;
}

const QuotePage: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const navigate = useNavigate();

  const [activeStep, setActiveStep] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [quoteReceived, setQuoteReceived] = useState(false);
  const [quoteAmount, setQuoteAmount] = useState<string>('');
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState<QuoteForm>({
    serviceType: '',
    customerType: '',
    name: '',
    email: '',
    phone: '',
    age: '',
    vehicleType: '',
    vehicleYear: '',
    homeType: '',
    homeSize: '',
    businessType: '',
    employeeCount: '',
    travelDestination: '',
    travelDuration: '',
    coverageAmount: '',
    additionalInfo: '',
  });

  const steps = [
    {
      label: 'Hizmet Seçimi',
      description: 'Sigorta türünü ve müşteri tipini seçin',
    },
    {
      label: 'Kişisel Bilgiler',
      description: 'Temel bilgilerinizi girin',
    },
    {
      label: 'Detay Bilgiler',
      description: 'Seçilen hizmete özel bilgileri girin',
    },
    {
      label: 'Kapsam ve Özet',
      description: 'Kapsam miktarını ve ek bilgileri belirtin',
    },
  ];

  const handleNext = () => {
    setActiveStep((prevActiveStep) => prevActiveStep + 1);
    setError(null);
  };

  const handleBack = () => {
    setActiveStep((prevActiveStep) => prevActiveStep - 1);
  };

  const handleFormChange = (field: keyof QuoteForm, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    setError(null);
  };

  const handleSubmit = async () => {
    setIsLoading(true);
    setError(null);

    try {
      // Validation
      if (!formData.serviceType || !formData.customerType || !formData.name || 
          !formData.email || !formData.phone || !formData.age || !formData.coverageAmount) {
        throw new Error('Lütfen tüm zorunlu alanları doldurun');
      }

      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      // Calculate mock quote based on service type - Backend ile eşleşen fiyatlar
      let baseAmount = 0;
      switch (formData.serviceType) {
        case 'traffic':
          baseAmount = 800; // Trafik Sigortası - Backend: 800 TL
          break;
        case 'home':
          baseAmount = 1200; // Konut Sigortası - Backend: 1200 TL
          break;
        case 'health':
          baseAmount = 3000; // Sağlık Sigortası - Backend: 3000 TL
          break;
        case 'business':
          baseAmount = 2000; // İş Yeri Sigortası - Backend: 2000 TL
          break;
        case 'travel':
          baseAmount = 300; // Seyahat Sigortası - Backend: 300 TL
          break;
        case 'life':
          baseAmount = 5000; // Hayat Sigortası - Backend: 5000 TL
          break;
        default:
          baseAmount = 2000;
      }

      // Base amount without customer discount (discount will be applied by admin/agent)
      const finalAmount = baseAmount;

      setQuoteAmount(finalAmount.toFixed(0));
      setQuoteReceived(true);
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Teklif alınamadı');
    } finally {
      setIsLoading(false);
    }
  };

  const handleBackClick = () => {
    navigate('/');
  };

  const handleContactClick = () => {
    navigate('/insurance-agencies');
  };

  const renderServiceSelection = () => (
    <Grid container spacing={3}>
      <Grid item xs={12} md={6}>
        <FormControl fullWidth required>
          <InputLabel>Sigorta Türü</InputLabel>
          <Select
            value={formData.serviceType}
            label="Sigorta Türü"
            onChange={(e) => handleFormChange('serviceType', e.target.value)}
          >
            <MenuItem value="traffic">Trafik Sigortası</MenuItem>
            <MenuItem value="home">Konut Sigortası</MenuItem>
            <MenuItem value="health">Sağlık Sigortası</MenuItem>
            <MenuItem value="business">İşyeri Sigortası</MenuItem>
            <MenuItem value="travel">Seyahat Sigortası</MenuItem>
            <MenuItem value="life">Hayat Sigortası</MenuItem>
          </Select>
        </FormControl>
      </Grid>
      <Grid item xs={12} md={6}>
        <FormControl fullWidth required>
          <InputLabel>Müşteri Tipi</InputLabel>
          <Select
            value={formData.customerType}
            label="Müşteri Tipi"
            onChange={(e) => handleFormChange('customerType', e.target.value)}
          >
            <MenuItem value="individual">Bireysel</MenuItem>
            <MenuItem value="corporate">Kurumsal</MenuItem>
          </Select>
        </FormControl>
      </Grid>
    </Grid>
  );

  const renderPersonalInfo = () => (
    <Grid container spacing={3}>
      <Grid item xs={12} md={6}>
        <TextField
          fullWidth
          label="Ad Soyad"
          value={formData.name}
          onChange={(e) => handleFormChange('name', e.target.value)}
          required
        />
      </Grid>
      <Grid item xs={12} md={6}>
        <TextField
          fullWidth
          label="E-posta"
          type="email"
          value={formData.email}
          onChange={(e) => handleFormChange('email', e.target.value)}
          required
        />
      </Grid>
      <Grid item xs={12} md={6}>
        <TextField
          fullWidth
          label="Telefon"
          value={formData.phone}
          onChange={(e) => handleFormChange('phone', e.target.value)}
          required
        />
      </Grid>
      <Grid item xs={12} md={6}>
        <TextField
          fullWidth
          label="Yaş"
          type="number"
          value={formData.age}
          onChange={(e) => handleFormChange('age', e.target.value)}
          required
        />
      </Grid>
    </Grid>
  );

  const renderServiceDetails = () => {
    switch (formData.serviceType) {
      case 'traffic':
        return (
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Araç Türü</InputLabel>
                <Select
                  value={formData.vehicleType}
                  label="Araç Türü"
                  onChange={(e) => handleFormChange('vehicleType', e.target.value)}
                >
                  <MenuItem value="car">Otomobil</MenuItem>
                  <MenuItem value="suv">SUV</MenuItem>
                  <MenuItem value="truck">Kamyon</MenuItem>
                  <MenuItem value="motorcycle">Motosiklet</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Araç Model Yılı"
                value={formData.vehicleYear}
                onChange={(e) => handleFormChange('vehicleYear', e.target.value)}
              />
            </Grid>
          </Grid>
        );
      case 'home':
        return (
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>Konut Türü</InputLabel>
                <Select
                  value={formData.homeType}
                  label="Konut Türü"
                  onChange={(e) => handleFormChange('homeType', e.target.value)}
                >
                  <MenuItem value="apartment">Daire</MenuItem>
                  <MenuItem value="house">Müstakil Ev</MenuItem>
                  <MenuItem value="villa">Villa</MenuItem>
                  <MenuItem value="office">Ofis</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Konut Büyüklüğü (m²)"
                value={formData.homeSize}
                onChange={(e) => handleFormChange('homeSize', e.target.value)}
              />
            </Grid>
          </Grid>
        );
      case 'business':
        return (
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <FormControl fullWidth>
                <InputLabel>İşletme Türü</InputLabel>
                <Select
                  value={formData.businessType}
                  label="İşletme Türü"
                  onChange={(e) => handleFormChange('businessType', e.target.value)}
                >
                  <MenuItem value="retail">Perakende</MenuItem>
                  <MenuItem value="manufacturing">Üretim</MenuItem>
                  <MenuItem value="service">Hizmet</MenuItem>
                  <MenuItem value="office">Ofis</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Çalışan Sayısı"
                value={formData.employeeCount}
                onChange={(e) => handleFormChange('employeeCount', e.target.value)}
              />
            </Grid>
          </Grid>
        );
      case 'travel':
        return (
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Seyahat Destinasyonu"
                value={formData.travelDestination}
                onChange={(e) => handleFormChange('travelDestination', e.target.value)}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Seyahat Süresi (gün)"
                type="number"
                value={formData.travelDuration}
                onChange={(e) => handleFormChange('travelDuration', e.target.value)}
              />
            </Grid>
          </Grid>
        );
      default:
        return null;
    }
  };

  const renderCoverageSummary = () => (
    <Grid container spacing={3}>
      <Grid item xs={12} md={6}>
        <TextField
          fullWidth
          label="Kapsam Miktarı (TL)"
          type="number"
          value={formData.coverageAmount}
          onChange={(e) => handleFormChange('coverageAmount', e.target.value)}
          required
        />
      </Grid>
      <Grid item xs={12} md={6}>
        <TextField
          fullWidth
          label="Ek Bilgiler"
          multiline
          rows={3}
          value={formData.additionalInfo}
          onChange={(e) => handleFormChange('additionalInfo', e.target.value)}
        />
      </Grid>
    </Grid>
  );

  if (quoteReceived) {
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
                textAlign: 'center',
                p: 6,
              }}
            >
              <CheckCircle sx={{ fontSize: 80, color: '#4caf50', mb: 3 }} />
              <Typography variant="h4" component="h1" sx={{ fontWeight: 700, mb: 2 }}>
                Teklifiniz Hazır!
              </Typography>
              <Typography variant="h6" sx={{ mb: 4, opacity: 0.8 }}>
                {formData.serviceType === 'traffic' ? 'Trafik' :
                 formData.serviceType === 'home' ? 'Konut' :
                 formData.serviceType === 'health' ? 'Sağlık' :
                 formData.serviceType === 'business' ? 'İşyeri' :
                 formData.serviceType === 'travel' ? 'Seyahat' :
                 formData.serviceType === 'life' ? 'Hayat' : 'Sigorta'} Sigortası için teklifiniz
              </Typography>
              
              <Box sx={{ mb: 4 }}>
                <Typography variant="h2" sx={{ fontWeight: 700, color: '#2196f3', mb: 1 }}>
                  {quoteAmount} TL
                </Typography>
                <Typography variant="body1" sx={{ opacity: 0.7 }}>
                  Temel fiyat - İndirim oranı admin/agent tarafından belirlenecek
                </Typography>
              </Box>

              <Divider sx={{ mb: 4 }} />

              <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
                <Button
                  variant="contained"
                  size="large"
                  onClick={handleContactClick}
                  startIcon={<Phone />}
                  sx={{
                    py: 1.5,
                    px: 4,
                    fontSize: '1.1rem',
                    fontWeight: 600,
                    borderRadius: 3,
                    background: 'linear-gradient(135deg, #4caf50 0%, #45a049 100%)',
                    '&:hover': {
                      background: 'linear-gradient(135deg, #45a049 0%, #3d8b40 100%)',
                    },
                  }}
                >
                  Acentelerle İletişim
                </Button>
                <Button
                  variant="outlined"
                  size="large"
                  onClick={handleBackClick}
                  startIcon={<ArrowBack />}
                  sx={{
                    py: 1.5,
                    px: 4,
                    fontSize: '1.1rem',
                    fontWeight: 600,
                    borderRadius: 3,
                  }}
                >
                  Ana Sayfaya Dön
                </Button>
              </Box>
            </Paper>
          </Container>
        </Box>
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh' }}>
      <Header />
      <Box
        sx={{
          minHeight: 'calc(100vh - 64px)',
          background: theme.palette.mode === 'dark'
            ? 'linear-gradient(135deg, #0d47a1 0%, #1565c0 50%, #1976d2 100%)'
            : 'linear-gradient(135deg, #e3f2fd 0%, #bbdefb 50%, #90caf9 100%)',
          py: 4,
        }}
      >
        <Container maxWidth="lg">
          {/* Back Button */}
          <Box sx={{ mb: 3 }}>
            <Button
              startIcon={<ArrowBack />}
              onClick={handleBackClick}
              sx={{
                color: 'white',
                fontWeight: 600,
                '&:hover': {
                  backgroundColor: 'rgba(255, 255, 255, 0.1)',
                },
              }}
            >
              Ana Sayfaya Dön
            </Button>
          </Box>

          {/* Header */}
          <Paper
            elevation={8}
            sx={{
              borderRadius: 4,
              overflow: 'hidden',
              background: theme.palette.mode === 'dark' ? 'rgba(0, 0, 0, 0.8)' : 'rgba(255, 255, 255, 0.95)',
              backdropFilter: 'blur(20px)',
              mb: 4,
            }}
          >
            <Box
              sx={{
                background: 'linear-gradient(135deg, #2196f3 0%, #1976d2 100%)',
                color: 'white',
                textAlign: 'center',
                py: 6,
                px: 3,
              }}
            >
              <Calculate sx={{ fontSize: 80, mb: 3 }} />
              <Typography
                variant={isMobile ? 'h4' : 'h3'}
                component="h1"
                sx={{
                  fontWeight: 700,
                  mb: 2,
                  textShadow: '2px 2px 4px rgba(0,0,0,0.3)',
                }}
              >
                Sigorta Teklifi Alın
              </Typography>
              <Typography
                variant="h6"
                sx={{
                  opacity: 0.9,
                  fontWeight: 400,
                  maxWidth: 600,
                  mx: 'auto',
                }}
              >
                Hızlı ve kolay bir şekilde sigorta teklifinizi alın
              </Typography>
            </Box>
          </Paper>

          {/* Stepper Form */}
          <Paper
            elevation={6}
            sx={{
              borderRadius: 4,
              background: theme.palette.mode === 'dark' ? 'rgba(0, 0, 0, 0.8)' : 'rgba(255, 255, 255, 0.95)',
              backdropFilter: 'blur(20px)',
              p: 4,
            }}
          >
            {error && (
              <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>
                {error}
              </Alert>
            )}

            <Stepper activeStep={activeStep} orientation={isMobile ? 'vertical' : 'horizontal'}>
              {steps.map((step, index) => (
                <Step key={step.label}>
                  <StepLabel>{step.label}</StepLabel>
                  {isMobile && (
                    <StepContent>
                      <Box sx={{ mt: 2, mb: 2 }}>
                        {index === 0 && renderServiceSelection()}
                        {index === 1 && renderPersonalInfo()}
                        {index === 2 && renderServiceDetails()}
                        {index === 3 && renderCoverageSummary()}
                      </Box>
                      <Box sx={{ mb: 2 }}>
                        <Button
                          variant="contained"
                          onClick={index === steps.length - 1 ? handleSubmit : handleNext}
                          disabled={isLoading}
                          sx={{ mr: 1 }}
                        >
                          {index === steps.length - 1 ? (
                            isLoading ? <CircularProgress size={20} /> : 'Teklif Al'
                          ) : (
                            'Devam Et'
                          )}
                        </Button>
                        <Button
                          disabled={index === 0}
                          onClick={handleBack}
                          sx={{ mr: 1 }}
                        >
                          Geri
                        </Button>
                      </Box>
                    </StepContent>
                  )}
                </Step>
              ))}
            </Stepper>

            {!isMobile && (
              <Box sx={{ mt: 4 }}>
                <Box sx={{ mb: 3 }}>
                  {activeStep === 0 && renderServiceSelection()}
                  {activeStep === 1 && renderPersonalInfo()}
                  {activeStep === 2 && renderServiceDetails()}
                  {activeStep === 3 && renderCoverageSummary()}
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Button
                    color="inherit"
                    disabled={activeStep === 0}
                    onClick={handleBack}
                    sx={{ mr: 1 }}
                  >
                    Geri
                  </Button>
                  <Box>
                    <Button
                      variant="contained"
                      onClick={activeStep === steps.length - 1 ? handleSubmit : handleNext}
                      disabled={isLoading}
                    >
                      {activeStep === steps.length - 1 ? (
                        isLoading ? <CircularProgress size={20} /> : 'Teklif Al'
                      ) : (
                        'Devam Et'
                      )}
                    </Button>
                  </Box>
                </Box>
              </Box>
            )}
          </Paper>
        </Container>
      </Box>
    </Box>
  );
};

export default QuotePage;
