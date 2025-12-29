import React from 'react';
import {
  Box,
  Container,
  Typography,
  Grid,
  Card,
  CardContent,
  CardActions,
  Button,
  useTheme,
  useMediaQuery,
  Paper,
  Chip,
  Divider,
} from '@mui/material';
import {
  DirectionsCar,
  Home,
  LocalHospital,
  Business,
  Flight,
  Favorite,
  ArrowBack,
  CheckCircle,
  Star,
} from '@mui/icons-material';
import { useNavigate, useParams } from 'react-router-dom';
import Header from './Header';

interface Service {
  id: string;
  title: string;
  description: string;
  icon: React.ReactElement;
  color: string;
  features: string[];
  coverage: string[];
  premium: string;
  duration: string;
}

const ServicesPage: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const navigate = useNavigate();
  const { serviceId } = useParams();

  const services: Service[] = [
    {
      id: 'traffic',
      title: 'Trafik Sigortası',
      description: 'Aracınız için kapsamlı koruma ve güvenlik',
      icon: <DirectionsCar sx={{ fontSize: 60 }} />,
      color: '#e74c3c',
      features: [
        'Kasko ve MTPL kapsamı',
        'Çalınma ve hırsızlık koruması',
        'Doğal afet koruması',
        'Yol yardım hizmeti',
        '7/24 müşteri desteği'
      ],
      coverage: [
        'Araç hasarı',
        'Üçüncü şahıs zararları',
        'Sürücü ve yolcu koruması',
        'Çalınma ve hırsızlık',
        'Doğal afetler'
      ],
      premium: 'Yıllık 2.500 TL\'den başlayan',
      duration: '1 yıl'
    },
    {
      id: 'home',
      title: 'Konut Sigortası',
      description: 'Eviniz ve eşyalarınız için tam koruma',
      icon: <Home sx={{ fontSize: 60 }} />,
      color: '#3498db',
      features: [
        'Yangın ve patlama koruması',
        'Hırsızlık ve vandalizm',
        'Su hasarı koruması',
        'Doğal afet koruması',
        'Eşya koruması'
      ],
      coverage: [
        'Bina hasarı',
        'Eşya hasarı',
        'Hırsızlık',
        'Su hasarı',
        'Doğal afetler'
      ],
      premium: 'Yıllık 800 TL\'den başlayan',
      duration: '1 yıl'
    },
    {
      id: 'health',
      title: 'Sağlık Sigortası',
      description: 'Sağlığınız için kapsamlı koruma',
      icon: <LocalHospital sx={{ fontSize: 60 }} />,
      color: '#2ecc71',
      features: [
        'Hastane yatış giderleri',
        'Ameliyat giderleri',
        'İlaç giderleri',
        'Kontrol ve muayene',
        'Yurtdışı tedavi'
      ],
      coverage: [
        'Hastane masrafları',
        'Ameliyat giderleri',
        'İlaç giderleri',
        'Muayene giderleri',
        'Özel tedavi'
      ],
      premium: 'Yıllık 3.500 TL\'den başlayan',
      duration: '1 yıl'
    },
    {
      id: 'business',
      title: 'İşyeri Sigortası',
      description: 'İşletmeniz için kapsamlı koruma',
      icon: <Business sx={{ fontSize: 60 }} />,
      color: '#f39c12',
      features: [
        'İşyeri hasarı koruması',
        'İş durması tazminatı',
        'Sorumluluk sigortası',
        'Çalışan kazası',
        'Ekipman koruması'
      ],
      coverage: [
        'İşyeri binası',
        'İş ekipmanları',
        'Stok ve mal',
        'İş durması',
        'Sorumluluk'
      ],
      premium: 'Yıllık 5.000 TL\'den başlayan',
      duration: '1 yıl'
    },
    {
      id: 'travel',
      title: 'Seyahat Sigortası',
      description: 'Seyahatlerinizde güvenle, her an koruma altında kalın',
      icon: <Flight sx={{ fontSize: 60 }} />,
      color: '#9c27b0',
      features: [
        'Sağlık koruması',
        'Bagaj kaybı',
        'Uçuş iptali',
        'Acil tıbbi yardım',
        'Yurtdışı koruması'
      ],
      coverage: [
        'Sağlık giderleri',
        'Bagaj kaybı',
        'Uçuş iptali',
        'Acil tıbbi yardım',
        'Yurtdışı koruması'
      ],
      premium: 'Seyahat başına 150 TL\'den başlayan',
      duration: 'Seyahat süresi'
    },
    {
      id: 'life',
      title: 'Hayat Sigortası',
      description: 'Ailenizin geleceğini güvence altına alın',
      icon: <Favorite sx={{ fontSize: 60 }} />,
      color: '#e91e63',
      features: [
        'Vefat tazminatı',
        'Maluliyet koruması',
        'Kritik hastalık',
        'Birikim imkanı',
        'Aile koruması'
      ],
      coverage: [
        'Vefat tazminatı',
        'Maluliyet koruması',
        'Kritik hastalık',
        'Birikim',
        'Aile koruması'
      ],
      premium: 'Yıllık 4.500 TL\'den başlayan',
      duration: '10-30 yıl'
    }
  ];

  const selectedService = services.find(service => service.id === serviceId) || services[0];

  const handleBackClick = () => {
    navigate('/');
  };

  const handleQuoteClick = () => {
    navigate('/quote');
  };

  const handleContactClick = () => {
    navigate('/insurance-agencies');
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

          {/* Service Header */}
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
                background: `linear-gradient(135deg, ${selectedService.color} 0%, ${selectedService.color}dd 100%)`,
                color: 'white',
                textAlign: 'center',
                py: 6,
                px: 3,
              }}
            >
              <Box sx={{ mb: 3 }}>
                {selectedService.icon}
              </Box>
              <Typography
                variant={isMobile ? 'h4' : 'h3'}
                component="h1"
                sx={{
                  fontWeight: 700,
                  mb: 2,
                  textShadow: '2px 2px 4px rgba(0,0,0,0.3)',
                }}
              >
                {selectedService.title}
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
                {selectedService.description}
              </Typography>
            </Box>
          </Paper>

          {/* Service Details */}
          <Grid container spacing={4}>
            {/* Features */}
            <Grid item xs={12} md={6}>
              <Paper
                elevation={6}
                sx={{
                  borderRadius: 3,
                  p: 3,
                  height: '100%',
                  background: theme.palette.mode === 'dark' ? 'rgba(0, 0, 0, 0.8)' : 'rgba(255, 255, 255, 0.95)',
                }}
              >
                <Typography
                  variant="h5"
                  component="h2"
                  sx={{
                    fontWeight: 700,
                    mb: 3,
                    color: selectedService.color,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                  }}
                >
                  <Star sx={{ fontSize: 28 }} />
                  Özellikler
                </Typography>
                <Box sx={{ mb: 3 }}>
                  {selectedService.features.map((feature, index) => (
                    <Box
                      key={index}
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 2,
                        mb: 2,
                      }}
                    >
                      <CheckCircle sx={{ color: selectedService.color, fontSize: 20 }} />
                      <Typography variant="body1">{feature}</Typography>
                    </Box>
                  ))}
                </Box>
              </Paper>
            </Grid>

            {/* Coverage */}
            <Grid item xs={12} md={6}>
              <Paper
                elevation={6}
                sx={{
                  borderRadius: 3,
                  p: 3,
                  height: '100%',
                  background: theme.palette.mode === 'dark' ? 'rgba(0, 0, 0, 0.8)' : 'rgba(255, 255, 255, 0.95)',
                }}
              >
                <Typography
                  variant="h5"
                  component="h2"
                  sx={{
                    fontWeight: 700,
                    mb: 3,
                    color: selectedService.color,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1,
                  }}
                >
                  <CheckCircle sx={{ fontSize: 28 }} />
                  Kapsam
                </Typography>
                <Box sx={{ mb: 3 }}>
                  {selectedService.coverage.map((item, index) => (
                    <Chip
                      key={index}
                      label={item}
                      sx={{
                        m: 0.5,
                        backgroundColor: `${selectedService.color}20`,
                        color: selectedService.color,
                        fontWeight: 600,
                      }}
                    />
                  ))}
                </Box>
              </Paper>
            </Grid>

            {/* Pricing & Actions */}
            <Grid item xs={12}>
              <Paper
                elevation={6}
                sx={{
                  borderRadius: 3,
                  p: 4,
                  background: theme.palette.mode === 'dark' ? 'rgba(0, 0, 0, 0.8)' : 'rgba(255, 255, 255, 0.95)',
                  textAlign: 'center',
                }}
              >
                <Typography
                  variant="h4"
                  component="h3"
                  sx={{
                    fontWeight: 700,
                    mb: 2,
                    color: selectedService.color,
                  }}
                >
                  {selectedService.premium}
                </Typography>
                <Typography
                  variant="h6"
                  sx={{
                    mb: 4,
                    opacity: 0.8,
                  }}
                >
                  Süre: {selectedService.duration}
                </Typography>

                <Divider sx={{ mb: 4 }} />

                <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
                  <Button
                    variant="contained"
                    size="large"
                    onClick={handleQuoteClick}
                    sx={{
                      py: 1.5,
                      px: 4,
                      fontSize: '1.1rem',
                      fontWeight: 600,
                      borderRadius: 3,
                      background: `linear-gradient(135deg, ${selectedService.color} 0%, ${selectedService.color}dd 100%)`,
                      '&:hover': {
                        background: `linear-gradient(135deg, ${selectedService.color}dd 0%, ${selectedService.color} 100%)`,
                      },
                      boxShadow: `0 8px 25px ${selectedService.color}40`,
                    }}
                  >
                    Teklif Al
                  </Button>
                  <Button
                    variant="outlined"
                    size="large"
                    onClick={handleContactClick}
                    sx={{
                      py: 1.5,
                      px: 4,
                      fontSize: '1.1rem',
                      fontWeight: 600,
                      borderRadius: 3,
                      borderColor: selectedService.color,
                      color: selectedService.color,
                      '&:hover': {
                        backgroundColor: `${selectedService.color}10`,
                        borderColor: selectedService.color,
                      },
                    }}
                  >
                    Acentelerle İletişim
                  </Button>
                </Box>
              </Paper>
            </Grid>
          </Grid>
        </Container>
      </Box>
    </Box>
  );
};

export default ServicesPage;
