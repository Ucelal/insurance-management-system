import React, { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom';
import { ThemeProvider } from './contexts/ThemeContext';
import { AuthProvider } from './contexts/AuthContext';
import HomePage from './components/HomePage';
import CustomerLogin from './components/CustomerLogin';
import CustomerRegister from './components/CustomerRegister';


import AdminDashboard from './components/AdminDashboard';
import AgentDashboard from './components/AgentDashboard';
import CustomerDashboard from './components/CustomerDashboard';
import InsuranceAgencies from './components/InsuranceAgencies';
import ServicesPage from './components/ServicesPage';
import QuotePage from './components/QuotePage';
import ApiTest from './components/ApiTest';
import './App.css';

// Router refresh fix component
const RouterRefreshFix: React.FC = () => {
  const location = useLocation();

  useEffect(() => {
    // Store current path in localStorage for refresh recovery
    localStorage.setItem('currentPath', location.pathname);
  }, [location]);

  return null;
};

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <Router basename="/">
          <RouterRefreshFix />
          <div className="App">
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/customer-login" element={<CustomerLogin />} />
              <Route path="/customer-register" element={<CustomerRegister />} />


              <Route path="/customer-dashboard" element={<CustomerDashboard />} />
              <Route path="/admin-dashboard" element={<AdminDashboard />} />
              <Route path="/agent-dashboard" element={<AgentDashboard />} />
              <Route path="/insurance-agencies" element={<InsuranceAgencies />} />
              <Route path="/services/:serviceId" element={<ServicesPage />} />
              <Route path="/quote" element={<QuotePage />} />
              <Route path="/api-test" element={<ApiTest />} />
            </Routes>
          </div>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App; 