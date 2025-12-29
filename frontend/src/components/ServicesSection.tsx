import React from 'react';
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  CardActionArea,
  Container,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  DirectionsCar,
  Home,
  LocalHospital,
  Business,
  Flight,
  Favorite,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

interface ServiceCard {
  id: string;
  title: string;
  description: string;
  icon: React.ReactNode;
  color: string;
  route: string;
}

const ServicesSection: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const navigate = useNavigate();

  const services: ServiceCard[] = [
    {
      id: 'traffic',
      title: 'Trafik Sigortası',
      description: 'Aracınız için kapsamlı koruma ve güvenlik',
      icon: <DirectionsCar sx={{ fontSize: 50 }} />,
      color: '#e74c3c',
      route: '/services/traffic',
    },
    {
      id: 'home',
      title: 'Konut Sigortası',
      description: 'Eviniz ve eşyalarınız için tam koruma',
      icon: <Home sx={{ fontSize: 50 }} />,
      color: '#3498db',
      route: '/services/home',
    },
    {
      id: 'health',
      title: 'Sağlık Sigortası',
      description: 'Sağlığınız için kapsamlı koruma',
      icon: <LocalHospital sx={{ fontSize: 50 }} />,
      color: '#2ecc71',
      route: '/services/health',
    },
    {
      id: 'business',
      title: 'İşyeri Sigortası',
      description: 'İşletmeniz için kapsamlı koruma',
      icon: <Business sx={{ fontSize: 50 }} />,
      color: '#f39c12',
      route: '/services/business',
    },
    {
      id: 'travel',
      title: 'Seyahat Sigortası',
      description: 'Seyahatlerinizde güvenle, her an koruma altında kalın',
      icon: <Flight sx={{ fontSize: 50 }} />,
      color: '#9c27b0',
      route: '/services/travel',
    },
    {
      id: 'life',
      title: 'Hayat Sigortası',
      description: 'Ailenizin geleceğini güvence altına alın, onlara maddi destek sağlayın',
      icon: <Favorite sx={{ fontSize: 50 }} />,
      color: '#e91e63',
      route: '/services/life',
    },
  ];

  const handleServiceClick = (route: string) => {
    navigate(route);
  };

  return (
    <Box
      sx={{
        py: 8,
        background: theme.palette.mode === 'dark' 
          ? 'linear-gradient(180deg, #1a1a1a 0%, #2d2d2d 100%)'
          : 'linear-gradient(180deg, #f8f9fa 0%, #e9ecef 100%)',
      }}
    >
      <Container maxWidth="lg">
        {/* Section Header */}
        <Box sx={{ textAlign: 'center', mb: 6 }}>
          <Typography
            variant={isMobile ? 'h4' : 'h2'}
            component="h2"
            sx={{
              fontWeight: 700,
              mb: 2,
              color: theme.palette.mode === 'dark' ? 'white' : 'text.primary',
            }}
          >
            Hizmetlerimiz
          </Typography>
          <Typography
            variant="h6"
            component="p"
            sx={{
              color: theme.palette.mode === 'dark' ? 'rgba(255,255,255,0.7)' : 'text.secondary',
              maxWidth: 600,
              mx: 'auto',
              lineHeight: 1.6,
            }}
          >
            Size en uygun sigorta çözümlerini sunuyoruz. 
            Her ihtiyacınız için özel olarak tasarlanmış koruma paketleri.
          </Typography>
        </Box>

        {/* Services Grid */}
        <Grid container spacing={3}>
          {services.map((service) => (
            <Grid item xs={12} sm={6} md={4} key={service.id}>
              <Card
                sx={{
                  height: '100%',
                  borderRadius: 3,
                  overflow: 'hidden',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    transform: 'translateY(-8px)',
                    boxShadow: theme.palette.mode === 'dark'
                      ? '0 12px 40px rgba(0,0,0,0.6)'
                      : '0 12px 40px rgba(0,0,0,0.15)',
                  },
                  background: theme.palette.mode === 'dark' 
                    ? 'linear-gradient(135deg, #2d2d2d 0%, #1e1e1e 100%)'
                    : 'linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%)',
                  border: `1px solid ${theme.palette.mode === 'dark' ? '#404040' : '#e0e0e0'}`,
                }}
              >
                <CardActionArea
                  onClick={() => handleServiceClick(service.route)}
                  sx={{
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'stretch',
                  }}
                >
                  <CardContent
                    sx={{
                      p: 3,
                      height: '100%',
                      display: 'flex',
                      flexDirection: 'column',
                      alignItems: 'center',
                      textAlign: 'center',
                    }}
                  >
                    {/* Icon */}
                    <Box
                      sx={{
                        mb: 2,
                        p: 2,
                        borderRadius: '50%',
                        background: `linear-gradient(135deg, ${service.color}20 0%, ${service.color}40 100%)`,
                        border: `2px solid ${service.color}40`,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        transition: 'all 0.3s ease',
                        '&:hover': {
                          background: `linear-gradient(135deg, ${service.color}40 0%, ${service.color}60 100%)`,
                          border: `2px solid ${service.color}60`,
                          transform: 'scale(1.1)',
                        },
                      }}
                    >
                      <Box sx={{ color: service.color }}>
                        {service.icon}
                      </Box>
                    </Box>

                    {/* Title */}
                    <Typography
                      variant="h5"
                      component="h3"
                      sx={{
                        fontWeight: 600,
                        mb: 2,
                        color: theme.palette.mode === 'dark' ? 'white' : 'text.primary',
                      }}
                    >
                      {service.title}
                    </Typography>

                    {/* Description */}
                    <Typography
                      variant="body1"
                      sx={{
                        color: theme.palette.mode === 'dark' ? 'rgba(255,255,255,0.7)' : 'text.secondary',
                        lineHeight: 1.6,
                        flex: 1,
                      }}
                    >
                      {service.description}
                    </Typography>

                    {/* Learn More Button */}
                    <Box
                      sx={{
                        mt: 3,
                        pt: 2,
                        borderTop: `1px solid ${theme.palette.mode === 'dark' ? '#404040' : '#e0e0e0'}`,
                        width: '100%',
                      }}
                    >
                      <Typography
                        variant="body2"
                        sx={{
                          color: service.color,
                          fontWeight: 600,
                          textTransform: 'uppercase',
                          letterSpacing: 0.5,
                          '&:hover': {
                            textDecoration: 'underline',
                          },
                        }}
                      >
                        Detayları Gör →
                      </Typography>
                    </Box>
                  </CardContent>
                </CardActionArea>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>
    </Box>
  );
};

export default ServicesSection;
