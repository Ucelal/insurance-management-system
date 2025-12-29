import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Grid,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
  IconButton,
  Tooltip,
  TableSortLabel,
  Tabs,
  Tab
} from '@mui/material';
import {
  Assignment,
  PendingActions,
  CheckCircle,
  Cancel,
  Edit,
  Visibility,
  Refresh,
  Delete,
  Description,
  Image
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';
import { apiService } from '../services/api';
import { Offer, Agent, Claim } from '../types';
import Header from './Header';

const AgentDashboard: React.FC = () => {
  const { user } = useAuth();
  const [agent, setAgent] = useState<Agent | null>(null);
  const [offers, setOffers] = useState<Offer[]>([]);
  const [pendingOffers, setPendingOffers] = useState<Offer[]>([]);
  const [claims, setClaims] = useState<Claim[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedOffer, setSelectedOffer] = useState<Offer | null>(null);
  const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
  const [updateData, setUpdateData] = useState({
    status: '',
    notes: '',
    finalPrice: '',
    discountRate: ''
  });
  const [updateLoading, setUpdateLoading] = useState(false);

  // View offer modal states
  const [viewModalOpen, setViewModalOpen] = useState(false);
  const [selectedViewOffer, setSelectedViewOffer] = useState<Offer | null>(null);
  
  // PDF Modal states for offer documents
  const [isPdfModalOpen, setIsPdfModalOpen] = useState(false);
  const [pdfUrl, setPdfUrl] = useState<string>('');
  const [pdfTitle, setPdfTitle] = useState<string>('');
  
  // Inline edit states
  const [editingOffer, setEditingOffer] = useState<number | null>(null);
  const [inlineEditData, setInlineEditData] = useState<{
    basePrice: string;
    discountRate: string;
    status: string;
  }>({
    basePrice: '',
    discountRate: '',
    status: ''
  });
  const [savingOffer, setSavingOffer] = useState<number | null>(null);

  // Main tab state (Teklifler vs Olay Bildirimleri)
  const [mainTabValue, setMainTabValue] = useState(0);

  // Sub-tab state for offers (Bekleyen vs Onaylanmış)
  const [offerTabValue, setOfferTabValue] = useState(0);

  // Sub-tab state for claims (Bekleyen vs Onaylanmış)
  const [claimTabValue, setClaimTabValue] = useState(0);

  // Sorting states for offers
  const [sortBy, setSortBy] = useState<'offerId' | 'description' | 'customer' | 'customerEmail' | 'insuranceType' | 'basePrice' | 'discountRate' | 'finalPrice' | 'coverageAmount' | 'requestedStartDate' | 'status' | 'isCustomerApproved' | 'createdAt'>('createdAt');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');

  // Sorting states for claims
  const [claimSortBy, setClaimSortBy] = useState<'claimId' | 'policyNumber' | 'customer' | 'customerEmail' | 'type' | 'description' | 'incidentDate' | 'status' | 'amount' | 'notes' | 'processedBy' | 'processedByEmail' | 'processedByPhone' | 'date'>('date');
  const [claimSortOrder, setClaimSortOrder] = useState<'asc' | 'desc'>('desc');
  const [claimSearch, setClaimSearch] = useState('');

  // Main tab change handler
  const handleMainTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setMainTabValue(newValue);
  };

  // Sub-tab change handler for offers
  const handleOfferTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setOfferTabValue(newValue);
  };

  // Sub-tab change handler for claims
  const handleClaimTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setClaimTabValue(newValue);
  };

  // Filter offers based on tab
  const getFilteredOffersByTab = () => {
    if (!offers || offers.length === 0) return [];
    
    if (offerTabValue === 0) {
      // Bekleyen Teklifler - status: pending
      return offers.filter(offer => offer.status === 'pending');
    } else {
      // Onaylanmış Teklifler - status: approved
      return offers.filter(offer => offer.status === 'approved');
    }
  };

  // Sorting handler
  const handleSort = (column: 'offerId' | 'description' | 'customer' | 'customerEmail' | 'insuranceType' | 'basePrice' | 'discountRate' | 'finalPrice' | 'coverageAmount' | 'requestedStartDate' | 'status' | 'isCustomerApproved' | 'createdAt') => {
    if (sortBy === column) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(column);
      setSortOrder('asc');
    }
  };

  // Get sorted offers with tab filtering
  const getSortedOffers = () => {
    // First filter by tab
    const filteredOffers = getFilteredOffersByTab();
    
    // Then sort the filtered offers
    const sorted = [...filteredOffers].sort((a, b) => {
      let compareValue = 0;

      switch (sortBy) {
        case 'offerId':
          compareValue = (a.offerId || 0) - (b.offerId || 0);
          break;
        case 'description':
          compareValue = (a.description || '').localeCompare(b.description || '');
          break;
        case 'customer':
          compareValue = (a.customer?.user?.name || '').localeCompare(b.customer?.user?.name || '');
          break;
        case 'customerEmail':
          compareValue = (a.customer?.user?.email || '').localeCompare(b.customer?.user?.email || '');
          break;
        case 'insuranceType':
          compareValue = (a.insuranceType?.name || '').localeCompare(b.insuranceType?.name || '');
          break;
        case 'basePrice':
          compareValue = (a.basePrice || 0) - (b.basePrice || 0);
          break;
        case 'discountRate':
          compareValue = (a.discountRate || 0) - (b.discountRate || 0);
          break;
        case 'finalPrice':
          compareValue = (a.finalPrice || 0) - (b.finalPrice || 0);
          break;
        case 'coverageAmount':
          compareValue = (a.coverageAmount || 0) - (b.coverageAmount || 0);
          break;
        case 'requestedStartDate':
          compareValue = new Date(a.requestedStartDate || 0).getTime() - new Date(b.requestedStartDate || 0).getTime();
          break;
        case 'status':
          compareValue = (a.status || '').localeCompare(b.status || '');
          break;
        case 'isCustomerApproved':
          compareValue = (a.isCustomerApproved === b.isCustomerApproved) ? 0 : (a.isCustomerApproved ? 1 : -1);
          break;
        case 'createdAt':
          compareValue = new Date(a.createdAt || 0).getTime() - new Date(b.createdAt || 0).getTime();
          break;
      }

      return sortOrder === 'asc' ? compareValue : -compareValue;
    });

    return sorted;
  };

  // Claim sorting handler
  const handleClaimSort = (column: 'claimId' | 'policyNumber' | 'customer' | 'customerEmail' | 'type' | 'description' | 'incidentDate' | 'status' | 'amount' | 'notes' | 'processedBy' | 'processedByEmail' | 'processedByPhone' | 'date') => {
    if (claimSortBy === column) {
      setClaimSortOrder(claimSortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setClaimSortBy(column);
      setClaimSortOrder('asc');
    }
  };

  // Filter claims based on sub-tab
  const getFilteredClaimsByTab = () => {
    if (!claims || claims.length === 0) return [];
    
    if (claimTabValue === 0) {
      // Bekleyen Claims - status: Pending
      return claims.filter(claim => claim.status === 'Pending');
    } else {
      // Onaylanmış Claims - status: Approved
      return claims.filter(claim => claim.status === 'Approved');
    }
  };

  // Get filtered and sorted claims
  const getFilteredAndSortedClaims = () => {
    // First filter by tab
    let filtered = getFilteredClaimsByTab();
    
    // Search filter
    if (claimSearch) {
      filtered = filtered.filter(claim => 
        claim.claimId?.toString().includes(claimSearch) ||
        claim.policyNumber?.toLowerCase().includes(claimSearch.toLowerCase()) ||
        claim.createdByUserName?.toLowerCase().includes(claimSearch.toLowerCase()) ||
        claim.createdByUserEmail?.toLowerCase().includes(claimSearch.toLowerCase()) ||
        claim.type?.toLowerCase().includes(claimSearch.toLowerCase()) ||
        claim.description?.toLowerCase().includes(claimSearch.toLowerCase())
      );
    }
    
    // Sort
    filtered.sort((a, b) => {
      let compareValue = 0;
      
      switch (claimSortBy) {
        case 'claimId':
          compareValue = (a.claimId || 0) - (b.claimId || 0);
          break;
        case 'policyNumber':
          compareValue = (a.policyNumber || '').localeCompare(b.policyNumber || '');
          break;
        case 'customer':
          compareValue = (a.createdByUserName || '').localeCompare(b.createdByUserName || '');
          break;
        case 'customerEmail':
          compareValue = (a.createdByUserEmail || '').localeCompare(b.createdByUserEmail || '');
          break;
        case 'type':
          compareValue = (a.type || '').localeCompare(b.type || '');
          break;
        case 'description':
          compareValue = (a.description || '').localeCompare(b.description || '');
          break;
        case 'incidentDate':
          compareValue = new Date(a.incidentDate || 0).getTime() - new Date(b.incidentDate || 0).getTime();
          break;
        case 'status':
          compareValue = (a.status || '').localeCompare(b.status || '');
          break;
        case 'amount':
          compareValue = (a.approvedAmount || 0) - (b.approvedAmount || 0);
          break;
        case 'notes':
          compareValue = (a.notes || '').localeCompare(b.notes || '');
          break;
        case 'processedBy':
          compareValue = (a.processedByUserName || '').localeCompare(b.processedByUserName || '');
          break;
        case 'processedByEmail':
          compareValue = (a.processedByUserEmail || '').localeCompare(b.processedByUserEmail || '');
          break;
        case 'processedByPhone':
          compareValue = (a.processedByUserPhone || '').localeCompare(b.processedByUserPhone || '');
          break;
        case 'date':
          compareValue = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
          break;
      }
      
      return claimSortOrder === 'asc' ? compareValue : -compareValue;
    });
    
    return filtered;
  };

  // Inline edit functions
  const handleEditOffer = (offer: Offer) => {
    setEditingOffer(offer.offerId);
    setInlineEditData({
      basePrice: offer.basePrice.toString(),
      discountRate: offer.discountRate ? offer.discountRate.toString() : '',
      status: offer.status
    });
  };

  const handleCancelEdit = () => {
    setEditingOffer(null);
    setInlineEditData({
      basePrice: '',
      discountRate: '',
      status: ''
    });
  };

  const handleSaveOffer = async (offerId: number) => {
    try {
      setSavingOffer(offerId);
      
      const updateData = {
        basePrice: parseFloat(inlineEditData.basePrice),
        discountRate: parseFloat(inlineEditData.discountRate) || 0,
        status: inlineEditData.status,
        finalPrice: 0 // Backend hesaplayacak
      };

      console.log('💾 Saving offer inline:', { offerId, updateData });
      console.log('📤 Sending to backend:', {
        basePrice: updateData.basePrice,
        discountRate: updateData.discountRate,
        status: updateData.status,
        finalPrice: updateData.finalPrice
      });
      
      await apiService.updateOffer(offerId, updateData);
      
      // Teklifleri yeniden yükle
      await fetchAgentData();
      
      setEditingOffer(null);
      setInlineEditData({
        basePrice: '',
        discountRate: '',
        status: ''
      });
      
      console.log('✅ Offer updated successfully');
    } catch (error) {
      console.error('❌ Error updating offer:', error);
      alert('Teklif güncellenirken hata oluştu');
    } finally {
      setSavingOffer(null);
    }
  };

  const fetchAgentData = useCallback(async () => {
    try {
      setLoading(true);
      
      console.log('🔍 AgentDashboard: Fetching agent data for user ID:', user!.id);
      
      // Agent bilgilerini getir
      const agentData = await apiService.getAgentByUserId(user!.id);
      console.log('👤 AgentDashboard: Agent data received:', agentData);
      setAgent(agentData);
      
      if (agentData) {
        console.log('📋 AgentDashboard: Fetching offers for agent ID:', agentData.id);
        console.log('🏢 AgentDashboard: Agent department:', agentData.department);
        
        // Agent'ın departmanına göre teklifleri getir
        const allOffers = await apiService.getOffersByAgentDepartment(agentData.id);
        console.log('📊 AgentDashboard: All offers received:', allOffers);
        setOffers(allOffers);
        
        // Bekleyen teklifleri getir
        const pending = await apiService.getPendingOffersByAgentDepartment(agentData.id);
        console.log('⏳ AgentDashboard: Pending offers received:', pending);
        setPendingOffers(pending);

        // Agent'ın departmanına göre claim'leri getir
        try {
          const departmentClaims = await apiService.getClaimsByAgentDepartment(agentData.id);
          console.log('📝 AgentDashboard: Department claims received:', departmentClaims);
          setClaims(departmentClaims);
        } catch (error) {
          console.error('❌ AgentDashboard: Error fetching department claims:', error);
          setClaims([]);
        }
      }
    } catch (error) {
      console.error('❌ AgentDashboard: Error fetching agent data:', error);
    } finally {
      setLoading(false);
    }
  }, [user]);

  useEffect(() => {
    if (user) {
      fetchAgentData();
    }
  }, [user, fetchAgentData]);

  const handleUpdateOffer = (offer: Offer) => {
    setSelectedOffer(offer);
    setUpdateData({
      status: offer.status,
      notes: offer.description || '',
      finalPrice: offer.finalPrice?.toString() || '',
      discountRate: offer.discountRate?.toString() || ''
    });
    setUpdateDialogOpen(true);
  };

  const handleViewOffer = (offer: Offer) => {
    console.log('🔍 AgentDashboard: Viewing Offer:', offer);
    console.log('🔍 AgentDashboard: Additional Data:', offer.customerAdditionalInfo);
    
    setSelectedViewOffer(offer);
    setViewModalOpen(true);
  };

  // Delete offer states
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [offerToDelete, setOfferToDelete] = useState<Offer | null>(null);

  // Delete offer handler
  const handleDeleteOffer = (offer: Offer) => {
    setOfferToDelete(offer);
    setDeleteModalOpen(true);
  };

  // Confirm delete
  const confirmDeleteOffer = async () => {
    if (!offerToDelete) return;
    
    try {
      await apiService.deleteOffer(offerToDelete.offerId);
      console.log('✅ AgentDashboard: Offer deleted successfully');
      
      // Refresh offers list
      await fetchAgentData();
      
      setDeleteModalOpen(false);
      setOfferToDelete(null);
    } catch (error) {
      console.error('❌ AgentDashboard: Error deleting offer:', error);
      alert('Teklif silinirken hata oluştu');
    }
  };

  const handleUpdateSubmit = async () => {
    if (!selectedOffer || !agent) return;

    try {
      setUpdateLoading(true);
      
      const updatePayload = {
        status: updateData.status,
        notes: updateData.notes,
        finalPrice: updateData.finalPrice ? parseFloat(updateData.finalPrice) : undefined,
        discountRate: updateData.discountRate ? parseFloat(updateData.discountRate) : undefined
      };

      await apiService.updateOffer(selectedOffer.id, updatePayload);
      
      // Verileri yenile
      await fetchAgentData();
      
      setUpdateDialogOpen(false);
      setSelectedOffer(null);
    } catch (error) {
      console.error('Teklif güncellenirken hata:', error);
    } finally {
      setUpdateLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending': return 'warning';
      case 'approved': return 'success';
      case 'rejected': return 'error';
      case 'processing': return 'info';
      default: return 'default';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending': return 'Beklemede';
      case 'approved': return 'Onaylandı';
      case 'rejected': return 'Reddedildi';
      case 'processing': return 'İşleniyor';
      default: return status;
    }
  };

  if (loading) {
    return (
      <Box sx={{ minHeight: '100vh', backgroundColor: '#f5f5f5' }}>
        <Header />
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
          <CircularProgress />
        </Box>
      </Box>
    );
  }

  if (!agent) {
    return (
      <Box sx={{ minHeight: '100vh', backgroundColor: '#f5f5f5' }}>
        <Header />
        <Container maxWidth="lg" sx={{ mt: 4 }}>
          <Alert severity="error">
            Agent bilgileri bulunamadı. Lütfen yönetici ile iletişime geçin.
          </Alert>
        </Container>
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', backgroundColor: '#f5f5f5' }}>
      <Header />
      <Container maxWidth={false} sx={{ mt: 3, mb: 3, px: 2, width: '95%' }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" gutterBottom>
              Agent Dashboard
            </Typography>
          <Typography variant="h6" color="text.secondary">
            {agent.department} Departmanı - {agent.agentCode}
            </Typography>
          </Box>
          
        {/* Stats Cards */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Box display="flex" alignItems="center">
                  <Assignment color="primary" sx={{ mr: 2, fontSize: 40 }} />
                  <Box>
                    <Typography variant="h4">{offers.length}</Typography>
                    <Typography color="text.secondary">Toplam Teklif</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Box display="flex" alignItems="center">
                  <PendingActions color="warning" sx={{ mr: 2, fontSize: 40 }} />
                  <Box>
                    <Typography variant="h4">{pendingOffers.length}</Typography>
                    <Typography color="text.secondary">Bekleyen Teklif</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <Card>
                <CardContent>
                <Box display="flex" alignItems="center">
                  <CheckCircle color="success" sx={{ mr: 2, fontSize: 40 }} />
                  <Box>
                    <Typography variant="h4">
                      {offers.filter(o => o.status.toLowerCase() === 'approved').length}
                  </Typography>
                    <Typography color="text.secondary">Onaylanan</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Box display="flex" alignItems="center">
                  <Cancel color="error" sx={{ mr: 2, fontSize: 40 }} />
                  <Box>
                    <Typography variant="h4">
                      {offers.filter(o => o.status.toLowerCase() === 'rejected').length}
                    </Typography>
                    <Typography color="text.secondary">Reddedilen</Typography>
                  </Box>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
        </Grid>

        {/* Actions */}
        <Box sx={{ mb: 3, display: 'flex', gap: 2 }}>
                <Button
                  variant="contained"
            startIcon={<Refresh />}
            onClick={fetchAgentData}
                >
            Yenile
                </Button>
              </Box>
              
        {/* Department Panel with Tabs */}
        <Card sx={{ width: '100%', maxWidth: 'none' }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Departman Yönetimi ({agent.department})
                  </Typography>
            
            {/* Ana Tab'lar - Teklifler vs Olay Bildirimleri */}
            <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
              <Tabs value={mainTabValue} onChange={handleMainTabChange} aria-label="main tabs">
                <Tab label="Departman Teklifleri" />
                <Tab label="Olay Bildirimleri" />
              </Tabs>
            </Box>

            {/* TAB 0: Departman Teklifleri */}
            {mainTabValue === 0 && (
              <>
                {/* Alt Tab'lar - Bekleyen vs Onaylanmış */}
                <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
                  <Tabs value={offerTabValue} onChange={handleOfferTabChange} aria-label="offer tabs">
                    <Tab label="Bekleyen Teklifler" />
                    <Tab label="Onaylanmış Teklifler" />
                  </Tabs>
                </Box>
            
            <TableContainer component={Paper} sx={{ width: '100%', overflowX: 'auto', maxWidth: 'none' }}>
              <Table sx={{ minWidth: 1400, width: '100%' }}>
                    <TableHead>
                      <TableRow>
                    <TableCell sx={{ minWidth: 70, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'offerId'}
                        direction={sortBy === 'offerId' ? sortOrder : 'asc'}
                        onClick={() => handleSort('offerId')}
                      >
                        Teklif ID
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'description'}
                        direction={sortBy === 'description' ? sortOrder : 'asc'}
                        onClick={() => handleSort('description')}
                      >
                        Açıklama
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'customer'}
                        direction={sortBy === 'customer' ? sortOrder : 'asc'}
                        onClick={() => handleSort('customer')}
                      >
                        Müşteri
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 150, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'customerEmail'}
                        direction={sortBy === 'customerEmail' ? sortOrder : 'asc'}
                        onClick={() => handleSort('customerEmail')}
                      >
                        Müşteri E-posta
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 110, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'insuranceType'}
                        direction={sortBy === 'insuranceType' ? sortOrder : 'asc'}
                        onClick={() => handleSort('insuranceType')}
                      >
                        Sigorta Türü
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 90, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'basePrice'}
                        direction={sortBy === 'basePrice' ? sortOrder : 'asc'}
                        onClick={() => handleSort('basePrice')}
                      >
                        Temel Fiyat
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 90, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'discountRate'}
                        direction={sortBy === 'discountRate' ? sortOrder : 'asc'}
                        onClick={() => handleSort('discountRate')}
                      >
                        İndirim Oranı
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 90, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'finalPrice'}
                        direction={sortBy === 'finalPrice' ? sortOrder : 'asc'}
                        onClick={() => handleSort('finalPrice')}
                      >
                        Final Fiyat
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 110, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'coverageAmount'}
                        direction={sortBy === 'coverageAmount' ? sortOrder : 'asc'}
                        onClick={() => handleSort('coverageAmount')}
                      >
                        Kapsam Seviyesi
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 110, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'requestedStartDate'}
                        direction={sortBy === 'requestedStartDate' ? sortOrder : 'asc'}
                        onClick={() => handleSort('requestedStartDate')}
                      >
                        Başlangıç Tarihi
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 80, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'status'}
                        direction={sortBy === 'status' ? sortOrder : 'asc'}
                        onClick={() => handleSort('status')}
                      >
                        Durum
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 110, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'isCustomerApproved'}
                        direction={sortBy === 'isCustomerApproved' ? sortOrder : 'asc'}
                        onClick={() => handleSort('isCustomerApproved')}
                      >
                        Müşteri Onayı
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 90, padding: '8px 6px' }}>
                      <TableSortLabel
                        active={sortBy === 'createdAt'}
                        direction={sortBy === 'createdAt' ? sortOrder : 'asc'}
                        onClick={() => handleSort('createdAt')}
                      >
                        Oluşturulma
                      </TableSortLabel>
                    </TableCell>
                    <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>İşlemler</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                  {(() => {
                    const sortedOffers = getSortedOffers();
                    return sortedOffers.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={14} sx={{ textAlign: 'center', py: 4 }}>
                          <Typography variant="body2" color="text.secondary">
                            {offerTabValue === 0 ? 'Henüz bekleyen teklif bulunmuyor.' : 'Henüz onaylanmış teklif bulunmuyor.'}
                          </Typography>
                        </TableCell>
                      </TableRow>
                    ) : (
                      sortedOffers.map((offer) => (
                    <TableRow key={offer.offerId}>
                      <TableCell>#{offer.offerId}</TableCell>
                                                     <TableCell>
                        <Box>
                          <Typography variant="body2" noWrap>
                            {offer.description || 'Açıklama yok'}
                          </Typography>
                          {offer.customerAdditionalInfo && (
                            <Tooltip title={(() => {
                              try {
                                const parsed = JSON.parse(offer.customerAdditionalInfo);
                                return JSON.stringify(parsed, null, 2);
                              } catch {
                                return offer.customerAdditionalInfo;
                              }
                            })()}>
                              <Typography variant="caption" color="text.secondary" noWrap>
                                + Ek Bilgi
                               </Typography>
                            </Tooltip>
                          )}
                             </Box>
                           </TableCell>
                          <TableCell>
                        {offer.customer ? (
                          <Box>
                            <Typography variant="body2" fontWeight="bold">
                              {offer.customer.user?.name || 'N/A'}
                            </Typography>
                                 <Typography variant="caption" color="text.secondary">
                              {offer.customer.phone}
                                 </Typography>
                               </Box>
                        ) : (
                          'Bilinmeyen'
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {offer.customer?.user?.email || 'N/A'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {offer.insuranceType?.name || 'Bilinmeyen'}
                      </TableCell>
                      <TableCell>
                        {editingOffer === offer.offerId ? (
                          <TextField
                            size="small"
                            type="number"
                            value={inlineEditData.basePrice}
                            onChange={(e) => setInlineEditData(prev => ({ ...prev, basePrice: e.target.value }))}
                            sx={{ width: '80px' }}
                            inputProps={{ min: 0, step: 0.01 }}
                          />
                        ) : (
                          <Typography variant="body2">
                            ₺{offer.basePrice?.toLocaleString() || '0'}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        {editingOffer === offer.offerId ? (
                          <TextField
                            size="small"
                            type="number"
                            value={inlineEditData.discountRate}
                            onChange={(e) => setInlineEditData(prev => ({ ...prev, discountRate: e.target.value }))}
                            sx={{ width: '80px' }}
                            inputProps={{ min: 0, max: 100, step: 0.01 }}
                          />
                        ) : (
                          <Typography variant="body2">
                            %{offer.discountRate || '0'}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" fontWeight="bold">
                          ₺{offer.finalPrice?.toLocaleString() || '0'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {offer.coverageAmount === 0 ? 'Temel (+0%)' :
                           offer.coverageAmount === 25 ? 'Orta (+25%)' :
                           offer.coverageAmount === 40 ? 'Premium (+40%)' :
                           'Bilinmeyen'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {offer.requestedStartDate ? new Date(offer.requestedStartDate).toLocaleDateString('tr-TR') : '-'}
                        </Typography>
                      </TableCell>
                          <TableCell>
                            {editingOffer === offer.offerId ? (
                              <FormControl size="small" sx={{ minWidth: 120 }}>
                                <Select
                                  value={inlineEditData.status}
                                  onChange={(e) => setInlineEditData(prev => ({ ...prev, status: e.target.value }))}
                                >
                                  <MenuItem key="pending" value="pending">Bekliyor</MenuItem>
                                  <MenuItem key="approved" value="approved">Onaylandı</MenuItem>
                                  <MenuItem key="rejected" value="rejected">Reddedildi</MenuItem>
                                  <MenuItem key="expired" value="expired">Süresi Doldu</MenuItem>
                                </Select>
                              </FormControl>
                            ) : (
                              <Chip
                                label={getStatusLabel(offer.status)}
                                color={getStatusColor(offer.status) as any}
                                size="small"
                              />
                            )}
                          </TableCell>
                          <TableCell>
                            <Chip
                          label={offer.isCustomerApproved ? 'Onaylandı' : 'Bekliyor'}
                          color={offer.isCustomerApproved ? 'success' : 'warning'}
                              size="small"
                            />
                          </TableCell>
                          <TableCell>
                        <Typography variant="body2">
                          {new Date(offer.createdAt).toLocaleDateString('tr-TR')}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Box sx={{ display: 'flex', gap: 0.5 }}>
                              {editingOffer === offer.offerId ? (
                                <>
                                  <Tooltip title="Kaydet">
                                    <IconButton 
                                      size="small" 
                                      color="success"
                                      onClick={() => handleSaveOffer(offer.offerId)}
                                      disabled={savingOffer === offer.offerId}
                                      sx={{ padding: '4px' }}
                                    >
                                      {savingOffer === offer.offerId ? (
                                        <CircularProgress size={16} />
                                      ) : (
                                        <CheckCircle sx={{ fontSize: '1rem' }} />
                                      )}
                                    </IconButton>
                                  </Tooltip>
                                  <Tooltip title="İptal">
                                    <IconButton 
                                      size="small" 
                                      color="error"
                                      onClick={handleCancelEdit}
                                      sx={{ padding: '4px' }}
                                    >
                                      <Cancel sx={{ fontSize: '1rem' }} />
                                    </IconButton>
                                  </Tooltip>
                                </>
                              ) : (
                                <>
                                  {/* Düzenle butonu sadece müşteri onaylamadıysa gösterilir */}
                                  {!offer.isCustomerApproved && (
                                    <Tooltip title="Düzenle">
                                      <IconButton 
                                        size="small" 
                                        onClick={() => handleEditOffer(offer)}
                                        sx={{ padding: '4px' }}
                                      >
                                        <Edit sx={{ fontSize: '1rem' }} />
                                      </IconButton>
                                    </Tooltip>
                                  )}
                                  <Tooltip title="Detaylar">
                                    <IconButton 
                                      size="small" 
                                      onClick={() => handleViewOffer(offer)}
                                      sx={{ padding: '4px' }}
                                    >
                                      <Visibility sx={{ fontSize: '1rem' }} />
                                    </IconButton>
                                  </Tooltip>
                                  {/* Silme butonu sadece Bekleyen Teklifler tab'ında gösterilir */}
                                  {offerTabValue === 0 && (
                                    <Tooltip title="Sil">
                                      <IconButton 
                                        size="small" 
                                        color="error"
                                        onClick={() => handleDeleteOffer(offer)}
                                        sx={{ padding: '4px' }}
                                      >
                                        <Delete sx={{ fontSize: '1rem' }} />
                                      </IconButton>
                                    </Tooltip>
                                  )}
                                </>
                              )}
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))
                    );
                  })()}
                    </TableBody>
                  </Table>
                </TableContainer>
              </>
            )}

            {/* TAB 1: Departman Olay Bildirimleri */}
            {mainTabValue === 1 && (
              <>
                {/* Alt Tab'lar - Bekleyen vs Onaylanmış Claims */}
                <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
                  <Tabs value={claimTabValue} onChange={handleClaimTabChange} aria-label="claim tabs">
                    <Tab label="Bekleyen Bildirimler" />
                    <Tab label="Onaylanmış Bildirimler" />
                  </Tabs>
                </Box>

                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="subtitle1" fontWeight={600}>
                    Toplam {getFilteredAndSortedClaims().length} Bildirim
                  </Typography>
                  <TextField
                    size="small"
                    placeholder="Bildirim ara..."
                    value={claimSearch}
                    onChange={(e) => setClaimSearch(e.target.value)}
                    sx={{ width: 250 }}
                  />
                </Box>

                {getFilteredClaimsByTab().length === 0 ? (
                  <Box sx={{ textAlign: 'center', py: 4 }}>
                    <Typography variant="body1" color="text.secondary">
                      {claimTabValue === 0 ? 'Henüz bekleyen olay bildirimi bulunmuyor.' : 'Henüz onaylanmış olay bildirimi bulunmuyor.'}
                    </Typography>
                  </Box>
                ) : (
                  <TableContainer component={Paper} sx={{ width: '100%', overflowX: 'auto', maxWidth: 'none' }}>
                <Table sx={{ minWidth: 1400, width: '100%' }}>
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ minWidth: 70, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'claimId'}
                          direction={claimSortBy === 'claimId' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('claimId')}
                        >
                          Claim ID
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 120, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'policyNumber'}
                          direction={claimSortBy === 'policyNumber' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('policyNumber')}
                        >
                          Poliçe No
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'customer'}
                          direction={claimSortBy === 'customer' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('customer')}
                        >
                          Müşteri
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 150, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'customerEmail'}
                          direction={claimSortBy === 'customerEmail' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('customerEmail')}
                        >
                          Müşteri E-posta
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'type'}
                          direction={claimSortBy === 'type' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('type')}
                        >
                          Olay Türü
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 150, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'description'}
                          direction={claimSortBy === 'description' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('description')}
                        >
                          Açıklama
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'incidentDate'}
                          direction={claimSortBy === 'incidentDate' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('incidentDate')}
                        >
                          Olay Tarihi
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'status'}
                          direction={claimSortBy === 'status' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('status')}
                        >
                          Durum
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 110, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'amount'}
                          direction={claimSortBy === 'amount' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('amount')}
                        >
                          Onaylanan Tutar
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 150, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'notes'}
                          direction={claimSortBy === 'notes' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('notes')}
                        >
                          Yetkili Notu
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 120, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'processedBy'}
                          direction={claimSortBy === 'processedBy' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('processedBy')}
                        >
                          İşleyen Yetkili
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 150, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'processedByEmail'}
                          direction={claimSortBy === 'processedByEmail' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('processedByEmail')}
                        >
                          Yetkili E-posta
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 120, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'processedByPhone'}
                          direction={claimSortBy === 'processedByPhone' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('processedByPhone')}
                        >
                          Yetkili Telefon
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={claimSortBy === 'date'}
                          direction={claimSortBy === 'date' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('date')}
                        >
                          Oluşturma Tarihi
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 80, padding: '8px 6px' }}>İşlemler</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {getFilteredAndSortedClaims().map((claim) => (
                      <TableRow key={claim.claimId} hover>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" fontWeight={600} sx={{ fontSize: '0.8rem' }}>
                            #{claim.claimId}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" sx={{ fontSize: '0.8rem' }}>
                            {claim.policyNumber || 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" fontWeight={500} sx={{ fontSize: '0.8rem' }}>
                            {claim.createdByUserName || 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" sx={{ fontSize: '0.8rem' }}>
                            {claim.createdByUserEmail || 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Chip
                            label={claim.type}
                            size="small"
                            color="primary"
                            variant="outlined"
                            sx={{ fontSize: '0.7rem', height: '20px' }}
                          />
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" sx={{ maxWidth: 150, overflow: 'hidden', textOverflow: 'ellipsis', fontSize: '0.8rem' }}>
                            {claim.description}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="caption" sx={{ fontSize: '0.75rem' }}>
                            {claim.incidentDate ? new Date(claim.incidentDate).toLocaleDateString('tr-TR') : '-'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Chip
                            label={
                              claim.status === 'Pending' ? 'Beklemede' :
                              claim.status === 'Approved' ? 'Onaylandı' :
                              claim.status === 'Rejected' ? 'Reddedildi' :
                              claim.status
                            }
                            size="small"
                            color={
                              claim.status === 'Pending' ? 'warning' :
                              claim.status === 'Approved' ? 'success' :
                              claim.status === 'Rejected' ? 'error' :
                              'default'
                            }
                            sx={{ fontSize: '0.7rem', height: '20px' }}
                          />
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" fontWeight={600} sx={{ fontSize: '0.8rem' }}>
                            {claim.approvedAmount ? `₺${claim.approvedAmount.toLocaleString()}` : '-'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" sx={{ maxWidth: 150, overflow: 'hidden', textOverflow: 'ellipsis', fontSize: '0.8rem' }}>
                            {claim.notes || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" fontWeight={500} sx={{ fontSize: '0.8rem' }}>
                            {claim.processedByUserName || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" sx={{ fontSize: '0.8rem' }}>
                            {claim.processedByUserEmail || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" sx={{ fontSize: '0.8rem' }}>
                            {claim.processedByUserPhone || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="caption" sx={{ fontSize: '0.75rem' }}>
                            {new Date(claim.createdAt).toLocaleDateString('tr-TR')}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Box sx={{ display: 'flex', gap: 1 }}>
                            <Tooltip title="Detaylar">
                              <IconButton 
                                size="small" 
                                color="primary"
                                sx={{ padding: '4px' }}
                                onClick={() => {
                                  console.log('View claim:', claim);
                                }}
                              >
                                <Visibility sx={{ fontSize: '1rem' }} />
                              </IconButton>
                            </Tooltip>
                          </Box>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
                )}
              </>
            )}

          </CardContent>
        </Card>

        {/* Update Offer Dialog */}
        <Dialog open={updateDialogOpen} onClose={() => setUpdateDialogOpen(false)} maxWidth="sm" fullWidth>
          <DialogTitle>Teklif Güncelle</DialogTitle>
          <DialogContent>
            {selectedOffer && (
              <Box sx={{ pt: 2 }}>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Teklif #{selectedOffer.offerId} - {selectedOffer.insuranceType?.name}
                  </Typography>
                  
                <FormControl fullWidth sx={{ mt: 2, mb: 2 }}>
                  <InputLabel>Durum</InputLabel>
                  <Select
                    value={updateData.status}
                    onChange={(e) => setUpdateData({ ...updateData, status: e.target.value })}
                    label="Durum"
                  >
                    <MenuItem key="Pending" value="Pending">Beklemede</MenuItem>
                    <MenuItem key="Processing" value="Processing">İşleniyor</MenuItem>
                    <MenuItem key="Approved" value="Approved">Onaylandı</MenuItem>
                    <MenuItem key="Rejected" value="Rejected">Reddedildi</MenuItem>
                  </Select>
                </FormControl>
                
                <TextField
                  fullWidth
                  label="Notlar"
                  multiline
                  rows={3}
                  value={updateData.notes}
                  onChange={(e) => setUpdateData({ ...updateData, notes: e.target.value })}
                  sx={{ mb: 2 }}
                />
                
                <TextField
                  fullWidth
                  label="İndirim Oranı (%)"
                  type="number"
                  value={updateData.discountRate}
                  onChange={(e) => {
                    const discountRate = e.target.value;
                    const basePrice = selectedOffer?.basePrice || 0;
                    const coverageIncreaseRate = selectedOffer?.coverageAmount || 0; // 0, 25, 40
                    
                    // Kapsam artış oranını uygula
                    const priceWithCoverage = basePrice * (1 + coverageIncreaseRate / 100);
                    
                    // İndirim oranını uygula
                    const finalPrice = priceWithCoverage * (1 - (parseFloat(discountRate) || 0) / 100);
                    
                    setUpdateData({ 
                      ...updateData, 
                      discountRate, 
                      finalPrice: Math.max(0, finalPrice).toString()
                    });
                  }}
                  sx={{ mb: 2 }}
                  helperText="0-100 arası değer girin"
                />
                
                <TextField
                  fullWidth
                  label="Final Fiyat (TL)"
                  type="number"
                  value={updateData.finalPrice}
                  onChange={(e) => setUpdateData({ ...updateData, finalPrice: e.target.value })}
                  helperText="İndirim sonrası fiyat (otomatik hesaplanır)"
                />
          </Box>
            )}
        </DialogContent>
        <DialogActions>
            <Button onClick={() => setUpdateDialogOpen(false)}>
            İptal
          </Button>
          <Button 
              onClick={handleUpdateSubmit}
              variant="contained"
              disabled={updateLoading}
              startIcon={updateLoading ? <CircularProgress size={20} /> : null}
            >
              {updateLoading ? 'Güncelleniyor...' : 'Güncelle'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* View Offer Details Modal */}
        <Dialog 
          open={viewModalOpen} 
          onClose={() => setViewModalOpen(false)} 
          maxWidth="md" 
          fullWidth
          PaperProps={{
            sx: {
              borderRadius: 2,
              boxShadow: '0 8px 32px rgba(0,0,0,0.12)'
            }
          }}
        >
          <DialogTitle sx={{ 
            backgroundColor: 'primary.main', 
            color: 'white',
            display: 'flex',
            alignItems: 'center',
            gap: 1
          }}>
            <Visibility />
            Teklif Detayları
          </DialogTitle>
          <DialogContent sx={{ p: 3 }}>
            {selectedViewOffer && (
              <Box>
                {/* Header Info */}
                <Box sx={{ mb: 3, p: 2, backgroundColor: 'grey.50', borderRadius: 1 }}>
                  <Typography variant="h6" gutterBottom>
                    Teklif #{selectedViewOffer.offerId}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {selectedViewOffer.insuranceType?.name}
                  </Typography>
                </Box>

                {/* Basic Details Grid */}
                <Grid container spacing={2} sx={{ mb: 3 }}>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="primary" gutterBottom>
                      Müşteri Bilgileri
                    </Typography>
                    <Typography variant="body2"><strong>Ad:</strong> {selectedViewOffer.customer?.user?.name || 'N/A'}</Typography>
                    <Typography variant="body2"><strong>Telefon:</strong> {selectedViewOffer.customer?.phone || 'N/A'}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="primary" gutterBottom>
                      Teklif Bilgileri
                    </Typography>
                    <Typography variant="body2"><strong>Sigorta Türü:</strong> {selectedViewOffer.insuranceType?.name || 'N/A'}</Typography>
                    <Typography variant="body2"><strong>Durum:</strong> {selectedViewOffer.status || 'N/A'}</Typography>
                  </Grid>
                </Grid>

                {/* Financial Details */}
                <Box sx={{ mb: 3, p: 2, backgroundColor: 'success.50', borderRadius: 1 }}>
                  <Typography variant="subtitle2" color="success.dark" gutterBottom>
                    Finansal Detaylar
                  </Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={4}>
                      <Typography variant="body2"><strong>Base Fiyat:</strong></Typography>
                      <Typography variant="h6" color="primary">₺{selectedViewOffer.basePrice?.toLocaleString() || '0'}</Typography>
                    </Grid>
                    <Grid item xs={4}>
                      <Typography variant="body2"><strong>İndirim Oranı:</strong></Typography>
                      <Typography variant="h6" color="warning.main">%{selectedViewOffer.discountRate || '0'}</Typography>
                    </Grid>
                    <Grid item xs={4}>
                      <Typography variant="body2"><strong>Final Fiyat:</strong></Typography>
                      <Typography variant="h6" color="success.main">₺{selectedViewOffer.finalPrice?.toLocaleString() || '0'}</Typography>
                    </Grid>
                  </Grid>
                </Box>

                {/* Policy PDF Section */}
                {selectedViewOffer.policyPdfUrl && (
                  <Box sx={{ mb: 3, p: 2, backgroundColor: 'warning.50', borderRadius: 1 }}>
                    <Typography variant="subtitle2" color="warning.dark" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <span>📄</span>
                      Poliçe PDF Belgesi
                    </Typography>
                    <Box sx={{ 
                      p: 2, 
                      backgroundColor: 'white', 
                      borderRadius: 1, 
                      border: '1px solid',
                      borderColor: 'warning.200'
                    }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                        <Typography variant="body2" color="text.secondary">
                          Poliçe PDF dosyası mevcut
                        </Typography>
                        <Button
                          variant="contained"
                          size="small"
                          startIcon={<span>👁️</span>}
                          onClick={() => {
                            const pdfUrl = `http://localhost:5000${selectedViewOffer.policyPdfUrl}`;
                            setPdfUrl(pdfUrl);
                            setPdfTitle('Poliçe PDF Belgesi');
                            setIsPdfModalOpen(true);
                          }}
                        >
                          Görüntüle
                        </Button>
                      </Box>
                    </Box>
                  </Box>
                )}

                {/* Additional Data */}
                {selectedViewOffer.customerAdditionalInfo && (
                  <Box sx={{ mb: 3, p: 2, backgroundColor: 'info.50', borderRadius: 1 }}>
                    <Typography variant="subtitle2" color="info.dark" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Assignment />
                      Form Bilgileri ve Yüklenen Dosyalar
                    </Typography>
                    <Box sx={{ 
                      p: 2, 
                      backgroundColor: 'white', 
                      borderRadius: 1, 
                      border: '1px solid',
                      borderColor: 'info.200'
                    }}>
                      {(() => {
                        try {
                          const data = JSON.parse(selectedViewOffer.customerAdditionalInfo);
                          
                          return Object.entries(data).map(([key, value]) => {
                            // PDF dosyası kontrolü
                            const isPdfFile = String(value).includes('.pdf') && String(value).includes('/uploads/');
                            const isImageFile = String(value).includes('/uploads/') && (String(value).includes('.jpg') || String(value).includes('.jpeg') || String(value).includes('.png'));
                            
                            const label = key === 'destination' ? '📍 Hedef' :
                                         key === 'address' ? '🏠 Adres' :
                                         key === 'propertyType' ? '🏢 Bina Tipi' :
                                         key === 'buildingAge' ? '📅 Bina Yaşı' :
                                         key === 'securityFeatures' ? '🔒 Güvenlik Önlemleri' :
                                         key === 'insuranceType' ? '📋 Sigorta Türü' :
                                         key === 'travelDuration' ? '⏰ Seyahat Süresi' :
                                         key === 'travelPurpose' ? '🎯 Seyahat Amacı' :
                                         key === 'healthReport' ? '🏥 Sağlık Raporu' :
                                         key === 'deedDocument' ? '📄 Tapu Belgesi' :
                                         key === 'annualRevenueReport' ? '📊 Yıllık Gelir Raporu' :
                                         key === 'riskReport' ? '⚠️ Risk Raporu' :
                                         key === 'idFrontPhoto' ? '🆔 Kimlik Ön Yüz' :
                                         key === 'idBackPhoto' ? '🆔 Kimlik Arka Yüz' :
                                         key === 'accidentHistory' ? '🚗 Kaza Geçmişi' :
                                         key.charAt(0).toUpperCase() + key.slice(1);
                            
                            if (isPdfFile || isImageFile) {
                              // PDF veya resim dosyası için buton oluştur
                              let fileUrl = String(value).match(/\(([^)]+)\)/)?.[1] || String(value);
                              
                              // URL'yi düzelt - /uploads/ ile başlıyorsa /api/Document/serve/ ile değiştir
                              if (fileUrl.startsWith('/uploads/')) {
                                fileUrl = fileUrl.replace('/uploads/', '/api/Document/serve/');
                              }
                              
                              // Tam URL oluştur
                              const fullUrl = `http://localhost:5000${fileUrl}`;
                              
                              return (
                                <Box key={key} sx={{ display: 'flex', alignItems: 'center', gap: 1, p: 1, backgroundColor: 'grey.50', borderRadius: 1, border: '1px solid #e0e0e0', mb: 1 }}>
                                  <Typography variant="body2" sx={{ flex: 1 }}>
                                    <strong>{label}:</strong>
                                  </Typography>
                                  <Button
                                    variant="outlined"
                                    size="small"
                                    color="primary"
                                    startIcon={isPdfFile ? <Description /> : <Image />}
                                    onClick={() => {
                                      if (isPdfFile) {
                                        setPdfUrl(fullUrl);
                                        setPdfTitle(label);
                                        setIsPdfModalOpen(true);
                                      } else {
                                        // Resimler için yeni sekmede aç
                                        window.open(fullUrl, '_blank');
                                      }
                                    }}
                                    sx={{ minWidth: 120 }}
                                  >
                                    {isPdfFile ? 'PDF Aç' : 'Resmi Aç'}
                                  </Button>
                                </Box>
                              );
                            } else {
                              // Normal form verisi için
                              return (
                                <Box key={key} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                                  <Typography variant="body2" fontWeight="bold" color="primary">
                                    {label}:
                                  </Typography>
                                  <Typography variant="body2">{String(value)}</Typography>
                                </Box>
                              );
                            }
                          });
                        } catch {
                          return (
                            <Box sx={{ 
                              fontFamily: 'monospace',
                              fontSize: '0.875rem',
                              p: 1,
                              backgroundColor: 'grey.100',
                              borderRadius: 1
                            }}>
                              {selectedViewOffer.customerAdditionalInfo}
                            </Box>
                          );
                        }
                      })()}
                    </Box>
                  </Box>
                )}

                {/* Dates */}
                <Box sx={{ p: 2, backgroundColor: 'grey.50', borderRadius: 1 }}>
                  <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                    Tarih Bilgileri
                  </Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={6}>
                      <Typography variant="body2"><strong>Başlangıç:</strong> {selectedViewOffer.requestedStartDate ? new Date(selectedViewOffer.requestedStartDate).toLocaleDateString('tr-TR') : 'N/A'}</Typography>
                    </Grid>
                    <Grid item xs={6}>
                      <Typography variant="body2"><strong>Geçerlilik:</strong> {selectedViewOffer.validUntil ? new Date(selectedViewOffer.validUntil).toLocaleDateString('tr-TR') : 'N/A'}</Typography>
                    </Grid>
                  </Grid>
                </Box>
              </Box>
            )}
          </DialogContent>
          <DialogActions sx={{ p: 2, backgroundColor: 'grey.50' }}>
            <Button 
              onClick={() => setViewModalOpen(false)}
            variant="contained" 
              color="primary"
              startIcon={<Visibility />}
          >
              Kapat
          </Button>
        </DialogActions>
        </Dialog>

        {/* Delete Confirmation Dialog */}
        <Dialog
          open={deleteModalOpen}
          onClose={() => setDeleteModalOpen(false)}
          maxWidth="sm"
          fullWidth
          PaperProps={{ sx: { borderRadius: 2, boxShadow: '0 8px 32px rgba(0,0,0,0.12)' } }}
        >
          <DialogTitle sx={{ backgroundColor: 'error.main', color: 'white', display: 'flex', alignItems: 'center', gap: 1 }}>
            <Delete />
            Teklif Silme Onayı
          </DialogTitle>
          <DialogContent sx={{ p: 3 }}>
            <Typography variant="body1" gutterBottom>
              Bu teklifi silmek istediğinizden emin misiniz?
            </Typography>
            {offerToDelete && (
              <Box sx={{ mt: 2, p: 2, backgroundColor: 'grey.50', borderRadius: 1 }}>
                <Typography variant="subtitle2" gutterBottom>Teklif Bilgileri:</Typography>
                <Typography variant="body2"><strong>Teklif ID:</strong> #{offerToDelete.offerId}</Typography>
                <Typography variant="body2"><strong>Müşteri:</strong> {offerToDelete.customer?.user?.name || 'N/A'}</Typography>
                <Typography variant="body2"><strong>Sigorta Türü:</strong> {offerToDelete.insuranceType?.name || 'N/A'}</Typography>
                <Typography variant="body2"><strong>Final Fiyat:</strong> ₺{offerToDelete.finalPrice || '0'}</Typography>
              </Box>
            )}
            <Typography variant="body2" color="error" sx={{ mt: 2, fontWeight: 500 }}>
              ⚠️ Bu işlem geri alınamaz!
            </Typography>
          </DialogContent>
          <DialogActions sx={{ p: 2, backgroundColor: 'grey.50' }}>
            <Button 
              onClick={() => setDeleteModalOpen(false)} 
              variant="outlined"
              color="primary"
            >
              İptal
            </Button>
            <Button 
              onClick={confirmDeleteOffer} 
              variant="contained" 
              color="error"
              startIcon={<Delete />}
            >
              Sil
            </Button>
          </DialogActions>
        </Dialog>

        {/* PDF Modal */}
        <Dialog
          open={isPdfModalOpen}
          onClose={() => setIsPdfModalOpen(false)}
          maxWidth="lg"
          fullWidth
          sx={{
            '& .MuiDialog-paper': {
              margin: 2,
              maxHeight: '90vh',
              borderRadius: 2,
            }
          }}
        >
          <DialogTitle sx={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center',
            backgroundColor: 'primary.main',
            color: 'white',
            padding: 2
          }}>
            <Typography variant="h6" component="div">
              📄 {pdfTitle}
            </Typography>
            <IconButton
              onClick={() => setIsPdfModalOpen(false)}
              sx={{ color: 'white' }}
            >
              ✕
            </IconButton>
          </DialogTitle>
          <DialogContent sx={{ 
            padding: 0, 
            backgroundColor: '#f5f5f5',
            height: '70vh'
          }}>
            {pdfUrl && (
              <iframe
                src={pdfUrl}
                width="100%"
                height="100%"
                style={{
                  border: 'none',
                  borderRadius: '0 0 8px 8px'
                }}
                title={pdfTitle}
                onError={(e) => {
                  console.error('PDF yükleme hatası:', e);
                  // PDF yüklenemezse alternatif gösterim
                }}
              />
            )}
          </DialogContent>
          <DialogActions sx={{ 
            backgroundColor: '#f5f5f5',
            padding: 2,
            gap: 1
          }}>
            <Button
              onClick={() => window.open(pdfUrl, '_blank')}
              variant="outlined"
              startIcon={<span>🔗</span>}
            >
              Yeni Sekmede Aç
            </Button>
            <Button
              onClick={() => setIsPdfModalOpen(false)}
              variant="contained"
              color="primary"
            >
              Kapat
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </Box>
  );
};

export default AgentDashboard;