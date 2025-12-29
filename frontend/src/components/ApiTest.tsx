import React, { useState } from 'react';
import {
  Box,
  Container,
  Paper,
  Typography,
  Button,
  useTheme,
  Alert,
  CircularProgress,
  Grid,
  Card,
  CardContent,
} from '@mui/material';
import {
  CheckCircle,
  Error as ErrorIcon,
  Refresh,
  Api,
} from '@mui/icons-material';
import Header from './Header';
import apiService from '../services/api';

interface HealthStatus {
  status: string;
  timestamp: string;
  database: string;
  message: string;
}

const ApiTest: React.FC = () => {
  const theme = useTheme();
  const [healthStatus, setHealthStatus] = useState<HealthStatus | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [testResults, setTestResults] = useState<any>({});

  const testBackendConnection = async () => {
    setIsLoading(true);
    setError(null);

    try {
      // Direct fetch test
      const fetchResponse = await fetch('http://localhost:5000/api/health', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!fetchResponse.ok) {
        const status = fetchResponse.status;
        const statusText = fetchResponse.statusText;
        const errorMessage = `HTTP Error: ${status} ${statusText}`;
        throw new Error(errorMessage);
      }

      const data = await fetchResponse.json();
      console.log('Direct fetch response:', data);

      // Axios test
      const response = await apiService.healthCheck();
      if (response) {
        setHealthStatus({
          status: 'healthy',
          timestamp: new Date().toISOString(),
          database: 'connected',
          message: 'Backend is accessible'
        });
      }
    } catch (err: any) {
      console.error('Connection error:', err);
      setError(err.message || 'Backend connection failed');
      setHealthStatus({
        status: 'unhealthy',
        timestamp: new Date().toISOString(),
        database: 'disconnected',
        message: 'Backend is not accessible'
      });
    } finally {
      setIsLoading(false);
    }
  };

  const runAllTests = async () => {
    setIsLoading(true);
    setError(null);
    const results: any = {};

    try {
      // Test 1: Health Check
      try {
        await apiService.healthCheck();
        results.healthCheck = { success: true, message: 'Health check passed' };
      } catch (err: any) {
        results.healthCheck = { success: false, message: err.message };
      }

      // Test 2: Get Customers (will fail without auth, but we can test the endpoint)
      try {
        await apiService.getCustomers();
        results.getCustomers = { success: true, message: 'Customers endpoint accessible' };
      } catch (err: any) {
        if (err.message.includes('401')) {
          results.getCustomers = { success: true, message: 'Endpoint accessible (auth required)' };
        } else {
          results.getCustomers = { success: false, message: err.message };
        }
      }

      // Test 3: Get Policies
      try {
        await apiService.getPolicies();
        results.getPolicies = { success: true, message: 'Policies endpoint accessible' };
      } catch (err: any) {
        if (err.message.includes('401')) {
          results.getPolicies = { success: true, message: 'Endpoint accessible (auth required)' };
        } else {
          results.getPolicies = { success: false, message: err.message };
        }
      }

      setTestResults(results);
    } catch (err: any) {
      setError(err.message || 'Test failed');
    } finally {
      setIsLoading(false);
    }
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
                variant="h3"
                component="h1"
                sx={{
                  fontWeight: 700,
                  mb: 2,
                  textShadow: '2px 2px 4px rgba(0,0,0,0.3)',
                }}
              >
                <Api sx={{ mr: 2, verticalAlign: 'middle' }} />
                API Test Paneli
              </Typography>
              <Typography
                variant="h6"
                sx={{
                  opacity: 0.9,
                  fontWeight: 400,
                }}
              >
                Backend bağlantısını test edin
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

            {/* Content */}
            <Box sx={{ p: 3 }}>
              {/* Health Status */}
              {healthStatus && (
                <Box sx={{ mb: 3 }}>
                  <Typography variant="h6" gutterBottom>
                    Backend Durumu
                  </Typography>
                  <Card
                    sx={{
                      background: healthStatus.status === 'healthy'
                        ? 'linear-gradient(135deg, #e8f5e8 0%, #c8e6c9 100%)'
                        : 'linear-gradient(135deg, #ffebee 0%, #ffcdd2 100%)',
                    }}
                  >
                    <CardContent>
                      <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                        {healthStatus.status === 'healthy' ? (
                          <CheckCircle sx={{ color: '#4caf50', mr: 1 }} />
                        ) : (
                          <ErrorIcon sx={{ color: '#f44336', mr: 1 }} />
                        )}
                        <Typography variant="h6" component="div">
                          {healthStatus.status === 'healthy' ? 'Sağlıklı' : 'Sağlıksız'}
                        </Typography>
                      </Box>
                      <Typography variant="body2" color="text.secondary">
                        {healthStatus.message}
                      </Typography>
                      <Typography variant="caption" display="block">
                        Timestamp: {new Date(healthStatus.timestamp).toLocaleString()}
                      </Typography>
                    </CardContent>
                  </Card>
                </Box>
              )}

              {/* Test Buttons */}
              <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
                <Button
                  variant="contained"
                  color="primary"
                  onClick={testBackendConnection}
                  disabled={isLoading}
                  startIcon={isLoading ? <CircularProgress size={20} /> : <Refresh />}
                  sx={{
                    background: 'linear-gradient(135deg, #2196f3 0%, #1976d2 100%)',
                    '&:hover': {
                      background: 'linear-gradient(135deg, #1976d2 0%, #1565c0 100%)',
                    },
                  }}
                >
                  {isLoading ? 'Test Ediliyor...' : 'Backend Bağlantısını Test Et'}
                </Button>

                <Button
                  variant="outlined"
                  color="secondary"
                  onClick={runAllTests}
                  disabled={isLoading}
                  startIcon={<Api />}
                >
                  Tüm Testleri Çalıştır
                </Button>
              </Box>

              {/* Test Results */}
              {Object.keys(testResults).length > 0 && (
                <Box>
                  <Typography variant="h6" gutterBottom>
                    Test Sonuçları
                  </Typography>
                  <Grid container spacing={2}>
                    {Object.entries(testResults).map(([testName, result]: [string, any]) => (
                      <Grid item xs={12} sm={6} key={testName}>
                        <Card>
                          <CardContent>
                            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                              {result.success ? (
                                <CheckCircle sx={{ color: '#4caf50', mr: 1 }} />
                              ) : (
                                <ErrorIcon sx={{ color: '#f44336', mr: 1 }} />
                              )}
                              <Typography variant="h6" component="div">
                                {testName}
                              </Typography>
                            </Box>
                            <Typography variant="body2" color="text.secondary">
                              {result.message}
                            </Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                    ))}
                  </Grid>
                </Box>
              )}

              {/* Instructions */}
              <Box sx={{ mt: 3, p: 2, backgroundColor: 'rgba(0, 0, 0, 0.04)', borderRadius: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  <strong>Test Adımları:</strong><br />
                  1. "Backend Bağlantısını Test Et" butonuna tıklayın<br />
                  2. Browser console'u açın (F12) ve log'ları kontrol edin<br />
                  3. "Tüm Testleri Çalıştır" ile detaylı test yapın<br />
                  4. Yeşil işaret = Başarılı, Kırmızı = Başarısız
                </Typography>
              </Box>
            </Box>
          </Paper>
        </Container>
      </Box>
    </Box>
  );
};

export default ApiTest;

