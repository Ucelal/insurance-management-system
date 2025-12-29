import React from 'react';
import { Box } from '@mui/material';
import Header from './Header';
import HeroSection from './HeroSection';
import ServicesSection from './ServicesSection';
import AboutSection from './AboutSection';
import Footer from './Footer';

const HomePage: React.FC = () => {
  return (
    <Box sx={{ minHeight: '100vh' }}>
      <Header />
      <HeroSection />
      <ServicesSection />
      <AboutSection />
      <Footer />
    </Box>
  );
};

export default HomePage;
