import React from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  Box,
  IconButton,
  useTheme,
  useMediaQuery,
  Menu,
  MenuItem,
  Tooltip,
} from '@mui/material';
import {
  Brightness4,
  Brightness7,
  Login,
  PersonAdd,
  Shield,
  Business,
  Menu as MenuIcon,
  Api,
  Settings,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useTheme as useCustomTheme } from '../contexts/ThemeContext';
import { useAuth } from '../contexts/AuthContext';
import { getTokenInfo, getExpectedTokenDuration } from '../utils/tokenUtils';
import ProfileSettings from './ProfileSettings';

const Header: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const { isDarkMode, toggleTheme } = useCustomTheme();
  const { user, isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  // Check if we're on a login/register page
  const isAuthPage = location.pathname.includes('login') || location.pathname.includes('register');

  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const [profileOpen, setProfileOpen] = React.useState(false);

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleNavigation = (path: string) => {
    navigate(path);
    handleMenuClose();
  };

  const handleProfileOpen = () => {
    setProfileOpen(true);
    handleMenuClose();
  };

  const handleProfileClose = () => {
    setProfileOpen(false);
  };

  const handleLoginClick = () => {
    navigate('/customer-login');
  };

  const handleRegisterClick = () => {
    navigate('/customer-register');
  };

  const handleLogout = () => {
    logout();
    navigate('/');
  };



  return (
    <AppBar 
      position="static" 
      elevation={4}
      sx={{
        background: theme.palette.mode === 'dark' 
          ? 'linear-gradient(45deg, #1a237e 30%, #0d47a1 90%)'
          : 'linear-gradient(45deg, #1976d2 30%, #42a5f5 90%)',
        borderBottom: '2px solid rgba(255,255,255,0.2)',
      }}
    >
      <Toolbar sx={{ justifyContent: 'space-between' }}>
        {/* Logo */}
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            cursor: 'pointer',
            '&:hover': {
              opacity: 0.8,
            },
          }}
          onClick={() => navigate('/')}
        >
          <Shield 
            sx={{ 
              fontSize: 32, 
              color: 'white',
              mr: 1,
              filter: 'drop-shadow(0 2px 4px rgba(0,0,0,0.3))',
            }} 
          />
          <Typography
            variant="h4"
            component="div"
            sx={{
              fontWeight: 700,
              color: 'white',
              textShadow: '0 2px 4px rgba(0,0,0,0.3)',
            }}
          >
            InsurancePro
          </Typography>
        </Box>

        {/* Navigation Menu - Desktop */}
        {!isMobile && !isAuthPage && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Button
              color="inherit"
              startIcon={<Business />}
              onClick={() => navigate('/insurance-agencies')}
              sx={{
                color: 'white',
                border: '1px solid rgba(255,255,255,0.3)',
                '&:hover': {
                  border: '1px solid rgba(255,255,255,0.6)',
                  backgroundColor: 'rgba(255,255,255,0.1)',
                },
              }}
            >
              Acenteler
            </Button>
          </Box>
        )}

        {/* Right Side - Desktop */}
        {!isMobile && !isAuthPage && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            {/* Authentication Buttons */}
            {!isAuthenticated ? (
              <>
                <Button
                  color="inherit"
                  startIcon={<Login />}
                  onClick={handleLoginClick}
                  sx={{
                    color: 'white',
                    border: '1px solid rgba(255,255,255,0.3)',
                    '&:hover': {
                      border: '1px solid rgba(255,255,255,0.6)',
                      backgroundColor: 'rgba(255,255,255,0.1)',
                    },
                  }}
                >
                  Üye Girişi
                </Button>
                <Button
                  color="inherit"
                  startIcon={<PersonAdd />}
                  onClick={handleRegisterClick}
                  sx={{
                    color: 'white',
                    border: '1px solid rgba(255,255,255,0.3)',
                    '&:hover': {
                      border: '1px solid rgba(255,255,255,0.6)',
                      backgroundColor: 'rgba(255,255,255,0.1)',
                    },
                  }}
                >
                  Üye Ol
                </Button>
              </>
            ) : (
              <>
                                {/* User Info - Clickable */}
                <Tooltip title={`${user?.role} Dashboard'a git`} arrow>
                  <Box 
                    sx={{ 
                      display: 'flex', 
                      flexDirection: 'column',
                      alignItems: 'flex-start',
                      gap: 0.5,
                      cursor: 'pointer',
                      padding: '4px 8px',
                      borderRadius: 1,
                      '&:hover': {
                        backgroundColor: 'rgba(255,255,255,0.1)',
                      },
                      transition: 'background-color 0.2s ease',
                    }}
                    onClick={() => {
                      // Role'e göre dashboard'a yönlendir
                      switch (user?.role?.toLowerCase()) {
                        case 'admin':
                          navigate('/admin-dashboard');
                          break;
                        case 'agent':
                          navigate('/agent-dashboard');
                          break;
                        case 'customer':
                          navigate('/customer-dashboard');
                          break;
                        default:
                          navigate('/');
                      }
                    }}
                  >
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="body2" sx={{ color: 'white', fontWeight: 500 }}>
                        {user?.name}
                      </Typography>
                      <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.7)' }}>
                        ({user?.role})
                      </Typography>
                    </Box>
                    {/* Token süre bilgisi */}
                    {(() => {
                      const token = localStorage.getItem('token');
                      if (token) {
                        const tokenInfo = getTokenInfo(token);
                        if (tokenInfo) {
                          return (
                            <Typography 
                              variant="caption" 
                              sx={{ 
                                color: tokenInfo.isExpired ? '#ff6b6b' : 'rgba(255,255,255,0.6)',
                                fontSize: '0.7rem',
                                fontWeight: tokenInfo.isExpired ? 600 : 400,
                              }}
                            >
                              {tokenInfo.isExpired ? 'Token süresi dolmuş' : `Kalan: ${tokenInfo.remainingTime}`}
                            </Typography>
                          );
                        }
                      }
                      return (
                        <Typography 
                          variant="caption" 
                          sx={{ 
                            color: 'rgba(255,255,255,0.6)',
                            fontSize: '0.7rem',
                          }}
                        >
                          Süre: {getExpectedTokenDuration(user?.role || '')}
                        </Typography>
                      );
                    })()}
                  </Box>
                </Tooltip>
                
                {/* User Menu */}
                <IconButton
                  color="inherit"
                  onClick={handleMenuOpen}
                  sx={{
                    color: 'white',
                    border: '1px solid rgba(255,255,255,0.3)',
                    '&:hover': {
                      border: '1px solid rgba(255,255,255,0.6)',
                      backgroundColor: 'rgba(255,255,255,0.1)',
                    },
                  }}
                >
                  <Settings />
                </IconButton>
              </>
            )}

            <Button
              color="inherit"
              startIcon={<Api />}
              onClick={() => navigate('/api-test')}
              sx={{
                color: 'white',
                border: '1px solid rgba(255,255,255,0.3)',
                '&:hover': {
                  border: '1px solid rgba(255,255,255,0.6)',
                  backgroundColor: 'rgba(255,255,255,0.1)',
                },
              }}
            >
              API Test
            </Button>

            {/* Theme Toggle */}
            <IconButton
              color="inherit"
              onClick={toggleTheme}
              sx={{
                border: '1px solid rgba(255,255,255,0.3)',
                '&:hover': {
                  border: '1px solid rgba(255,255,255,0.6)',
                  backgroundColor: 'rgba(255,255,255,0.1)',
                },
              }}
            >
              {isDarkMode ? <Brightness7 /> : <Brightness4 />}
            </IconButton>
          </Box>
        )}

        {/* Right Side - Mobile */}
        {isMobile && !isAuthPage && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {/* Mobile Menu */}
            <IconButton
              color="inherit"
              onClick={handleMenuOpen}
              sx={{
                border: '1px solid rgba(255,255,255,0.3)',
                '&:hover': {
                  border: '1px solid rgba(255,255,255,0.6)',
                  backgroundColor: 'rgba(255,255,255,0.1)',
                },
              }}
            >
              <MenuIcon />
            </IconButton>

            {/* Theme Toggle */}
            <IconButton
              color="inherit"
              onClick={toggleTheme}
              sx={{
                border: '1px solid rgba(255,255,255,0.3)',
                '&:hover': {
                  border: '1px solid rgba(255,255,255,0.6)',
                  backgroundColor: 'rgba(255,255,255,0.1)',
                },
              }}
            >
              {isDarkMode ? <Brightness7 /> : <Brightness4 />}
            </IconButton>
          </Box>
        )}

        {/* Auth Pages - Only Theme Toggle */}
        {isAuthPage && (
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <IconButton
              color="inherit"
              onClick={toggleTheme}
              sx={{
                border: '1px solid rgba(255,255,255,0.3)',
                '&:hover': {
                  border: '1px solid rgba(255,255,255,0.6)',
                  backgroundColor: 'rgba(255,255,255,0.1)',
                },
              }}
            >
              {isDarkMode ? <Brightness7 /> : <Brightness4 />}
            </IconButton>
          </Box>
        )}

        {/* Mobile Navigation Menu */}
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleMenuClose}
          PaperProps={{
            sx: {
              mt: 1,
              minWidth: 200,
              backgroundColor: theme.palette.background.paper,
              border: `1px solid ${theme.palette.divider}`,
            }
          }}
        >
          {!isAuthenticated ? (
            <>
              <MenuItem onClick={() => handleNavigation('/')}>
                <Typography>Ana Sayfa</Typography>
              </MenuItem>
              <MenuItem onClick={() => handleNavigation('/insurance-agencies')}>
                <Typography>Acenteler</Typography>
              </MenuItem>
              <MenuItem onClick={handleLoginClick}>
                <Typography>Üye Girişi</Typography>
              </MenuItem>
              <MenuItem onClick={handleRegisterClick}>
                <Typography>Üye Ol</Typography>
              </MenuItem>
            </>
          ) : (
            <>
              <MenuItem 
                onClick={() => {
                  // Role'e göre dashboard'a yönlendir
                  switch (user?.role?.toLowerCase()) {
                    case 'admin':
                      navigate('/admin-dashboard');
                      break;
                    case 'agent':
                      navigate('/agent-dashboard');
                      break;
                    case 'customer':
                      navigate('/customer-dashboard');
                      break;
                    default:
                      navigate('/');
                  }
                  handleMenuClose();
                }}
                sx={{
                  cursor: 'pointer',
                  '&:hover': {
                    backgroundColor: 'rgba(0,0,0,0.04)',
                  },
                }}
              >
                <Typography variant="body2" sx={{ fontWeight: 500 }}>
                  {user?.name} ({user?.role})
                </Typography>
              </MenuItem>
              <MenuItem onClick={handleProfileOpen}>
                <Settings sx={{ mr: 1 }} />
                <Typography>Profil Ayarları</Typography>
              </MenuItem>
              <MenuItem onClick={handleLogout}>
                <Typography>Çıkış</Typography>
              </MenuItem>
            </>
          )}
        </Menu>
      </Toolbar>
      
      {/* Profile Settings Dialog */}
      <ProfileSettings 
        open={profileOpen} 
        onClose={handleProfileClose} 
      />
    </AppBar>
  );
};

export default Header;
