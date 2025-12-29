import React, { useState } from 'react';
import {
  Box,
  Container,
  Grid,
  Paper,
  Typography,
  Card,
  Button,
  IconButton,
  Avatar,
  Chip,
  useTheme,
  useMediaQuery,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
} from '@mui/material';
import {
  ExpandMore,
  Phone,
  Email,
  LocationOn,
  Business,
  DirectionsCar,
  Home,
  LocalHospital,
  Flight,
  Work,
  Favorite,
} from '@mui/icons-material';
import Header from './Header';

const InsuranceAgencies: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  // 6 ana sigorta hizmeti ve acenteleri
  const insuranceAgencies = [
    {
      id: 1,
      name: 'Trafik Sigortası Acentesi',
      type: 'Trafik',
      icon: <DirectionsCar />,
      color: '#e74c3c',
      description: 'Kasko, Trafik, MTPL ve araç ile ilgili tüm sigorta hizmetleri',
      address: 'İstanbul, Kadıköy, Moda Caddesi No:123',
      phone: '0216 555 01 01',
      email: 'trafik@insurancepro.com',
      agent: 'Ahmet Yılmaz',
      departments: [
        { name: 'Trafik Departmanı', responsible: 'Ahmet Yılmaz', phone: '0216 555 01 02' },
      ]
    },
    {
      id: 2,
      name: 'Konut Sigortası Acentesi',
      type: 'Konut',
      icon: <Home />,
      color: '#3498db',
      description: 'Ev, iş yeri, villa ve ticari gayrimenkul sigortaları',
      address: 'İstanbul, Beşiktaş, Levent Caddesi No:456',
      phone: '0212 555 02 01',
      email: 'konut@insurancepro.com',
      agent: 'Zeynep Arslan',
      departments: [
        { name: 'Konut Departmanı', responsible: 'Zeynep Arslan', phone: '0212 555 02 02' },
      ]
    },
    {
      id: 3,
      name: 'Sağlık Sigortası Acentesi',
      type: 'Sağlık',
      icon: <LocalHospital />,
      color: '#2ecc71',
      description: 'Bireysel ve kurumsal sağlık sigortaları, tamamlayıcı sağlık',
      address: 'İstanbul, Şişli, Teşvikiye Caddesi No:789',
      phone: '0212 555 03 01',
      email: 'saglik@insurancepro.com',
      agent: 'Deniz Yılmaz',
      departments: [
        { name: 'Sağlık Departmanı', responsible: 'Deniz Yılmaz', phone: '0212 555 03 02' },
      ]
    },
    {
      id: 4,
      name: 'Seyahat Sigortası Acentesi',
      type: 'Seyahat',
      icon: <Flight />,
      color: '#9b59b6',
      description: 'Yurtdışı seyahat, Schengen vizesi ve turizm sigortaları',
      address: 'İstanbul, Taksim, İstiklal Caddesi No:321',
      phone: '0212 555 04 01',
      email: 'seyahat@insurancepro.com',
      agent: 'Aslı Yıldız',
      departments: [
        { name: 'Seyahat Departmanı', responsible: 'Aslı Yıldız', phone: '0212 555 04 02' },
      ]
    },
    {
      id: 5,
      name: 'İş Yeri Sigortası Acentesi',
      type: 'İş Yeri',
      icon: <Work />,
      color: '#34495e',
      description: 'İşveren sorumluluğu, mesleki sorumluluk ve iş kazası sigortaları',
      address: 'İstanbul, Maslak, Büyükdere Caddesi No:987',
      phone: '0212 555 05 01',
      email: 'isyeri@insurancepro.com',
      agent: 'Kemal Yıldız',
      departments: [
        { name: 'İş Yeri Departmanı', responsible: 'Kemal Yıldız', phone: '0212 555 05 02' },
      ]
    },
    {
      id: 6,
      name: 'Hayat Sigortası Acentesi',
      type: 'Hayat',
      icon: <Favorite />,
      color: '#e91e63',
      description: 'Bireysel hayat, aile koruma, kritik hastalık ve sürekli sakatlık sigortaları',
      address: 'İstanbul, Kadıköy, Fenerbahçe Caddesi No:741',
      phone: '0216 555 06 01',
      email: 'hayat@insurancepro.com',
      agent: 'Leyla Yıldız',
      departments: [
        { name: 'Hayat Departmanı', responsible: 'Leyla Yıldız', phone: '0216 555 06 02' },
      ]
    }
  ];

  const [expandedAgency, setExpandedAgency] = useState<number | null>(null);

  const handleAgencyExpand = (agencyId: number) => {
    setExpandedAgency(expandedAgency === agencyId ? null : agencyId);
  };

  const handleContact = (type: 'phone' | 'email', value: string) => {
    if (type === 'phone') {
      window.open(`tel:${value}`);
    } else {
      window.open(`mailto:${value}`);
    }
  };

  return (
    <Box sx={{ minHeight: '100vh', backgroundColor: theme.palette.background.default }}>
      <Header />
      
      <Container maxWidth="xl" sx={{ mt: 3, mb: 3 }}>
        {/* Header Section */}
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <Typography variant="h3" component="h1" fontWeight={700} gutterBottom>
            Sigorta Acenteleri
          </Typography>
          <Typography variant="h6" color="text.secondary" sx={{ maxWidth: 800, mx: 'auto' }}>
            6 ana sigorta hizmetimiz için uzmanlaşmış acentelerimiz ve departmanlarımız ile hizmetinizdeyiz
          </Typography>
        </Box>

        {/* Statistics Cards */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card sx={{ textAlign: 'center', p: 2 }}>
              <Typography variant="h4" color="primary" fontWeight={700}>
                {insuranceAgencies.length}
              </Typography>
              <Typography variant="body1" color="text.secondary">
                Uzman Acente
              </Typography>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card sx={{ textAlign: 'center', p: 2 }}>
              <Typography variant="h4" color="primary" fontWeight={700}>
                {insuranceAgencies.length}
              </Typography>
              <Typography variant="body1" color="text.secondary">
                Uzman Departman
              </Typography>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card sx={{ textAlign: 'center', p: 2 }}>
              <Typography variant="h4" color="primary" fontWeight={700}>
                24/7
              </Typography>
              <Typography variant="body1" color="text.secondary">
                Müşteri Hizmeti
              </Typography>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card sx={{ textAlign: 'center', p: 2 }}>
              <Typography variant="h4" color="primary" fontWeight={700}>
                %99.9
              </Typography>
              <Typography variant="body1" color="text.secondary">
                Müşteri Memnuniyeti
              </Typography>
            </Card>
          </Grid>
        </Grid>

        {/* Insurance Agencies Grid */}
        <Grid container spacing={3}>
          {insuranceAgencies.map((agency) => (
            <Grid item xs={12} lg={6} key={agency.id}>
              <Paper 
                elevation={3}
                sx={{ 
                  p: 3, 
                  height: '100%',
                  border: `2px solid ${agency.color}20`,
                  '&:hover': {
                    border: `2px solid ${agency.color}40`,
                    transform: 'translateY(-2px)',
                    transition: 'all 0.3s ease',
                  }
                }}
              >
                {/* Agency Header */}
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <Avatar 
                    sx={{ 
                      bgcolor: agency.color, 
                      width: 56, 
                      height: 56, 
                      mr: 2 
                    }}
                  >
                    {agency.icon}
                  </Avatar>
                  <Box>
                    <Typography variant="h6" fontWeight={600} gutterBottom>
                      {agency.name}
                    </Typography>
                    <Chip 
                      label={agency.type} 
                      size="small" 
                      sx={{ 
                        bgcolor: agency.color, 
                        color: 'white',
                        fontWeight: 600
                      }} 
                    />
                  </Box>
                </Box>

                {/* Agency Description */}
                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  {agency.description}
                </Typography>

                {/* Agent Information */}
                <Box sx={{ mb: 2, p: 2, bgcolor: `${agency.color}10`, borderRadius: 2 }}>
                  <Typography variant="body2" fontWeight={600} color="primary" gutterBottom>
                    Sorumlu Agent:
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {agency.agent}
                  </Typography>
                </Box>

                {/* Contact Information */}
                <Box sx={{ mb: 2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                    <LocationOn sx={{ fontSize: 16, color: theme.palette.text.secondary, mr: 1 }} />
                    <Typography variant="body2" fontSize="0.875rem">
                      {agency.address}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                    <Phone sx={{ fontSize: 16, color: theme.palette.text.secondary, mr: 1 }} />
                    <Typography 
                      variant="body2" 
                      fontSize="0.875rem"
                      sx={{ cursor: 'pointer', '&:hover': { color: 'primary.main' } }}
                      onClick={() => handleContact('phone', agency.phone)}
                    >
                      {agency.phone}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    <Email sx={{ fontSize: 16, color: theme.palette.text.secondary, mr: 1 }} />
                    <Typography 
                      variant="body2" 
                      fontSize="0.875rem"
                      sx={{ cursor: 'pointer', '&:hover': { color: 'primary.main' } }}
                      onClick={() => handleContact('email', agency.email)}
                    >
                      {agency.email}
                    </Typography>
                  </Box>
                </Box>

                {/* Departments Accordion */}
                <Accordion 
                  expanded={expandedAgency === agency.id}
                  onChange={() => handleAgencyExpand(agency.id)}
                  sx={{ 
                    boxShadow: 'none',
                    '&:before': { display: 'none' },
                    border: `1px solid ${theme.palette.divider}`,
                    borderRadius: 2,
                  }}
                >
                  <AccordionSummary expandIcon={<ExpandMore />}>
                    <Typography variant="body2" fontWeight={600} color="primary">
                      Departman Detayları
                    </Typography>
                  </AccordionSummary>
                  <AccordionDetails>
                    <List dense>
                      {agency.departments.map((dept, index) => (
                        <ListItem key={index}>
                          <ListItemIcon>
                            <Business sx={{ fontSize: 20, color: agency.color }} />
                          </ListItemIcon>
                          <ListItemText
                            primary={dept.name}
                            secondary={`Sorumlu: ${dept.responsible}`}
                          />
                          <IconButton 
                            size="small" 
                            color="primary"
                            onClick={() => handleContact('phone', dept.phone)}
                          >
                            <Phone />
                          </IconButton>
                        </ListItem>
                      ))}
                    </List>
                  </AccordionDetails>
                </Accordion>

                {/* Action Buttons */}
                <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<Phone />}
                    onClick={() => handleContact('phone', agency.phone)}
                    sx={{ flex: 1 }}
                  >
                    Ara
                  </Button>
                  <Button
                    variant="outlined"
                    size="small"
                    startIcon={<Email />}
                    onClick={() => handleContact('email', agency.email)}
                    sx={{ flex: 1 }}
                  >
                    E-posta
                  </Button>
                </Box>
              </Paper>
            </Grid>
          ))}
        </Grid>
      </Container>
    </Box>
  );
};

export default InsuranceAgencies;
