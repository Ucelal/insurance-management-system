import React from 'react';
import {
  Box,
  Typography,
  Grid,
  Container,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import {
  TrendingUp,
  People,
  Security,
  Speed,
} from '@mui/icons-material';

interface StatCard {
  id: string;
  icon: React.ReactElement;
  number: string;
  label: string;
  description: string;
  color: string;
}

const AboutSection = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  const stats: StatCard[] = [
    {
      id: 'experience',
      icon: <TrendingUp sx={{ fontSize: 40 }} /> as React.ReactElement,
      number: '20+',
      label: 'Yıllık Deneyim',
      description: 'Sektörde uzun yıllara dayanan tecrübemiz',
      color: '#1976d2',
    },
    {
      id: 'customers',
      icon: <People sx={{ fontSize: 40 }} /> as React.ReactElement,
      number: '10,000+',
      label: 'Mutlu Müşteri',
      description: 'Güvenimizi kazanan memnun müşteri sayısı',
      color: '#2e7d32',
    },
    {
      id: 'security',
      icon: <Security sx={{ fontSize: 40 }} /> as React.ReactElement,
      number: '100%',
      label: 'Güvenli Hizmet',
      description: 'En yüksek güvenlik standartlarında hizmet',
      color: '#d32f2f',
    },
    {
      id: 'speed',
      icon: <Speed sx={{ fontSize: 40 }} /> as React.ReactElement,
      number: '24/7',
      label: 'Hızlı Destek',
      description: 'Kesintisiz müşteri desteği ve hızlı çözüm',
      color: '#ed6c02',
    },
  ];

  return (
    <Box
      sx={{
        py: 8,
        background: theme.palette.mode === 'dark'
          ? 'linear-gradient(180deg, #2d2d2d 0%, #1a1a1a 100%)'
          : 'linear-gradient(180deg, #ffffff 0%, #f8f9fa 100%)',
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
            Neden Bizi Seçmelisiniz?
          </Typography>
          <Typography
            variant="h6"
            component="p"
            sx={{
              color: theme.palette.mode === 'dark' ? 'rgba(255,255,255,0.7)' : 'text.secondary',
              maxWidth: 700,
              mx: 'auto',
              lineHeight: 1.6,
            }}
          >
            Müşteri memnuniyeti odaklı yaklaşımımız ve kaliteli hizmet anlayışımızla 
            sigorta sektöründe öncü konumdayız.
          </Typography>
        </Box>

        {/* Stats Grid */}
        <Grid container spacing={4}>
          {stats.map((stat) => (
            <Grid item xs={12} sm={6} md={3} key={stat.id}>
              <Box
                sx={{
                  textAlign: 'center',
                  p: 3,
                  borderRadius: 3,
                  background: theme.palette.mode === 'dark'
                    ? 'linear-gradient(135deg, #2d2d2d 0%, #1e1e1e 100%)'
                    : 'linear-gradient(135deg, #ffffff 0%, #f8f9fa 100%)',
                  border: `1px solid ${theme.palette.mode === 'dark' ? '#404040' : '#e0e0e0'}`,
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    transform: 'translateY(-5px)',
                    boxShadow: theme.palette.mode === 'dark'
                      ? '0 8px 30px rgba(0,0,0,0.5)'
                      : '0 8px 30px rgba(0,0,0,0.1)',
                  },
                }}
              >
                {/* Icon */}
                <Box
                  sx={{
                    mb: 2,
                    p: 2,
                    borderRadius: '50%',
                    background: `linear-gradient(135deg, ${stat.color}20 0%, ${stat.color}40 100%)`,
                    border: `2px solid ${stat.color}40`,
                    display: 'inline-flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    transition: 'all 0.3s ease',
                    '&:hover': {
                      background: `linear-gradient(135deg, ${stat.color}40 0%, ${stat.color}60 100%)`,
                      border: `2px solid ${stat.color}60`,
                      transform: 'scale(1.1)',
                    },
                  }}
                >
                  <Box sx={{ color: stat.color }}>
                    {stat.icon}
                  </Box>
                </Box>

                {/* Number */}
                <Typography
                  variant={isMobile ? 'h3' : 'h2'}
                  component="div"
                  sx={{
                    fontWeight: 700,
                    mb: 1,
                    color: stat.color,
                    fontSize: isMobile ? '2.5rem' : '3rem',
                  }}
                >
                  {stat.number}
                </Typography>

                {/* Label */}
                <Typography
                  variant="h6"
                  component="h3"
                  sx={{
                    fontWeight: 600,
                    mb: 1,
                    color: theme.palette.mode === 'dark' ? 'white' : 'text.primary',
                  }}
                >
                  {stat.label}
                </Typography>

                {/* Description */}
                <Typography
                  variant="body2"
                  sx={{
                    color: theme.palette.mode === 'dark' ? 'rgba(255,255,255,0.7)' : 'text.secondary',
                    lineHeight: 1.5,
                  }}
                >
                  {stat.description}
                </Typography>
              </Box>
            </Grid>
          ))}
        </Grid>

        {/* Additional Info */}
        <Box sx={{ mt: 8, textAlign: 'center' }}>
          <Typography
            variant="h5"
            component="h3"
            sx={{
              fontWeight: 600,
              mb: 3,
              color: theme.palette.mode === 'dark' ? 'white' : 'text.primary',
            }}
          >
            Kalite ve Güvenilirlik
          </Typography>
          <Typography
            variant="body1"
            sx={{
              color: theme.palette.mode === 'dark' ? 'rgba(255,255,255,0.8)' : 'text.secondary',
              maxWidth: 800,
              mx: 'auto',
              lineHeight: 1.8,
              fontSize: '1.1rem',
            }}
          >
            Sigorta sektöründe 20 yılı aşkın deneyimimizle, müşterilerimize en uygun 
            sigorta çözümlerini sunuyoruz. Güvenilir, şeffaf ve müşteri odaklı 
            yaklaşımımızla, sizin ve ailenizin geleceğini güvence altına alıyoruz. 
            Profesyonel ekibimiz, her zaman yanınızda ve ihtiyaçlarınızı en iyi 
            şekilde karşılamaya hazır.
          </Typography>
        </Box>
      </Container>
    </Box>
  );
};

export default AboutSection;
