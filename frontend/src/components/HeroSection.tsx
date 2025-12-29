import React from 'react';
import {
  Box,
  Typography,
  Button,
  Container,
  useTheme,
  useMediaQuery,
} from '@mui/material';
import { Security, TrendingUp, Support } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

const HeroSection: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const navigate = useNavigate();

  const handleGetQuote = () => {
    navigate('/quote');
  };

  return (
    <Box
      sx={{
        background: theme.palette.mode === 'dark'
          ? 'linear-gradient(135deg, #0d47a1 0%, #1565c0 50%, #1976d2 100%)'
          : 'linear-gradient(135deg, #e3f2fd 0%, #bbdefb 50%, #90caf9 100%)',
        minHeight: isMobile ? '60vh' : '80vh',
        display: 'flex',
        alignItems: 'center',
        position: 'relative',
        overflow: 'hidden',
        '&::before': {
          content: '""',
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'url("data:image/svg+xml,%3Csvg width="60" height="60" viewBox="0 0 60 60" xmlns="http://www.w3.org/2000/svg"%3E%3Cg fill="none" fill-rule="evenodd"%3E%3Cg fill="%23ffffff" fill-opacity="0.05"%3E%3Ccircle cx="30" cy="30" r="2"/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")',
          opacity: 0.3,
        },
      }}
    >
      <Container maxWidth="lg">
        <Box
          sx={{
            display: 'flex',
            flexDirection: isMobile ? 'column' : 'row',
            alignItems: 'center',
            gap: 4,
            position: 'relative',
            zIndex: 1,
          }}
        >
          {/* Left Content */}
          <Box
            sx={{
              flex: 1,
              textAlign: isMobile ? 'center' : 'left',
              color: theme.palette.mode === 'dark' ? 'white' : 'text.primary',
            }}
          >
            <Typography
              variant={isMobile ? 'h3' : 'h1'}
              component="h1"
              sx={{
                fontWeight: 700,
                mb: 2,
                background: theme.palette.mode === 'dark'
                  ? 'linear-gradient(45deg, #fff 30%, #e3f2fd 90%)'
                  : 'linear-gradient(45deg, #1976d2 30%, #0d47a1 90%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                textShadow: theme.palette.mode === 'dark' 
                  ? '0 2px 4px rgba(0,0,0,0.3)' 
                  : 'none',
              }}
            >
              Güvenilir Sigorta Çözümleri
            </Typography>
            
            <Typography
              variant={isMobile ? 'h6' : 'h4'}
              component="h2"
              sx={{
                mb: 3,
                color: theme.palette.mode === 'dark' ? 'rgba(255,255,255,0.9)' : 'text.secondary',
                fontWeight: 400,
                lineHeight: 1.4,
              }}
            >
              Ailenizi ve varlıklarınızı koruyun, geleceğinizi güvence altına alın
            </Typography>

            <Box
              sx={{
                display: 'flex',
                flexDirection: isMobile ? 'column' : 'row',
                gap: 2,
                justifyContent: isMobile ? 'center' : 'flex-start',
              }}
            >
              <Button
                variant="contained"
                size="large"
                onClick={handleGetQuote}
                sx={{
                  py: 2,
                  px: 4,
                  fontSize: '1.1rem',
                  fontWeight: 600,
                  borderRadius: 3,
                  background: theme.palette.mode === 'dark'
                    ? 'linear-gradient(45deg, #ff6b35 30%, #f7931e 90%)'
                    : 'linear-gradient(45deg, #ff6b35 30%, #f7931e 90%)',
                  color: 'white',
                  boxShadow: '0 4px 15px rgba(255,107,53,0.4)',
                  '&:hover': {
                    background: theme.palette.mode === 'dark'
                      ? 'linear-gradient(45deg, #e55a2b 30%, #e0851a 90%)'
                      : 'linear-gradient(45deg, #e55a2b 30%, #e0851a 90%)',
                    boxShadow: '0 6px 20px rgba(255,107,53,0.6)',
                    transform: 'translateY(-2px)',
                  },
                  transition: 'all 0.3s ease',
                }}
              >
                Hemen Teklif Alın
              </Button>
              
              <Button
                variant="outlined"
                size="large"
                sx={{
                  py: 2,
                  px: 4,
                  fontSize: '1.1rem',
                  fontWeight: 600,
                  borderRadius: 3,
                  borderColor: theme.palette.mode === 'dark' ? 'white' : 'primary.main',
                  color: theme.palette.mode === 'dark' ? 'white' : 'primary.main',
                  '&:hover': {
                    borderColor: theme.palette.mode === 'dark' ? 'white' : 'primary.dark',
                    backgroundColor: theme.palette.mode === 'dark' ? 'rgba(255,255,255,0.1)' : 'primary.50',
                  },
                  transition: 'all 0.3s ease',
                }}
              >
                Hizmetlerimizi Keşfedin
              </Button>
            </Box>
          </Box>

          {/* Right Content - Features */}
          {!isMobile && (
            <Box
              sx={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                gap: 3,
              }}
            >
              {/* Feature Cards */}
              <Box
                sx={{
                  display: 'flex',
                  gap: 2,
                  justifyContent: 'flex-end',
                }}
              >
                <Box
                  sx={{
                    p: 2,
                    borderRadius: 3,
                    background: 'rgba(255,255,255,0.1)',
                    backdropFilter: 'blur(10px)',
                    border: '1px solid rgba(255,255,255,0.2)',
                    textAlign: 'center',
                    minWidth: 120,
                  }}
                >
                  <Security sx={{ fontSize: 40, color: 'white', mb: 1 }} />
                  <Typography variant="body2" sx={{ color: 'white', fontWeight: 600 }}>
                    Güvenli
                  </Typography>
                </Box>
                
                <Box
                  sx={{
                    p: 2,
                    borderRadius: 3,
                    background: 'rgba(255,255,255,0.1)',
                    backdropFilter: 'blur(10px)',
                    border: '1px solid rgba(255,255,255,0.2)',
                    textAlign: 'center',
                    minWidth: 120,
                  }}
                >
                  <TrendingUp sx={{ fontSize: 40, color: 'white', mb: 1 }} />
                  <Typography variant="body2" sx={{ color: 'white', fontWeight: 600 }}>
                    Uygun Fiyat
                  </Typography>
                </Box>
              </Box>

              <Box
                sx={{
                  display: 'flex',
                  gap: 2,
                  justifyContent: 'flex-end',
                }}
              >
                <Box
                  sx={{
                    p: 2,
                    borderRadius: 3,
                    background: 'rgba(255,255,255,0.1)',
                    backdropFilter: 'blur(10px)',
                    border: '1px solid rgba(255,255,255,0.2)',
                    textAlign: 'center',
                    minWidth: 120,
                  }}
                >
                  <Support sx={{ fontSize: 40, color: 'white', mb: 1 }} />
                  <Typography variant="body2" sx={{ color: 'white', fontWeight: 600 }}>
                    7/24 Destek
                  </Typography>
                </Box>
              </Box>
            </Box>
          )}
        </Box>
      </Container>
    </Box>
  );
};

export default HeroSection;
