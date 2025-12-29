import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { signalRService } from '../services/signalRService';
import {
  Box,
  Container,
  Grid,
  Paper,
  Typography,
  Card,
  CardContent,
  CardActions,
  Button,
  AppBar,
  Toolbar,
  IconButton,
  Drawer,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  useTheme,
  useMediaQuery,
  Avatar,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Badge,
  CircularProgress,
  Alert,
  Tabs,
  Tab,
  TextField,
  InputAdornment,
  FormControl,
  Select,
  MenuItem,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormHelperText,
  InputLabel,
  TableSortLabel,
} from '@mui/material';
import {
  Menu,
  Dashboard,
  People,
  Assignment,
  Payment,
  Assessment,
  Settings,
  Notifications,
  AccountCircle,
  TrendingUp,
  TrendingDown,
  Visibility,
  Edit,
  Delete,
  Add,
  Logout,
  Shield,
  Business,
  Search,
  Email,
  Phone,
  LocationOn,
  Description,
  Image,
  CheckCircle,
  Cancel,
  OpenInNew,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import Header from './Header';
import { apiService } from '../services/api';
import { Customer, User, Claim, Agent } from '../types';

interface Offer {
  offerId: number;
  customerId?: number;
  agentId?: number;
  insuranceTypeId?: number;
  description: string;
  basePrice: number;
  discountRate: number;
  finalPrice: number;
  status: string;
  department: string;
  coverageAmount?: number;
  requestedStartDate?: string;
  customerAdditionalInfo?: string;
  additionalData?: string;
  validUntil: string;
  isCustomerApproved: boolean;
  customerApprovedAt?: string;
  agentNotes?: string;
  adminNotes?: string;
  rejectionReason?: string;
  policyPdfUrl?: string;
  createdAt: string;
  updatedAt?: string;
  approvedAt?: string;
  processedAt?: string;
  reviewedAt?: string;
  reviewedByAgentId?: number;
  
  // Flat properties from backend
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  customerAddress: string;
  customerType: string;
  idNo: string;
  userId: number;
  agentName: string;
  agentEmail: string;
  agentPhone: string;
  agentAddress: string;
  agentCode: string;
  agentUserId: number;
  insuranceTypeName: string;
  insuranceTypeCategory: string;
  
  // Navigation properties
  customer?: Customer;
  agent?: Agent;
  insuranceType?: {
    id: number;
    name: string;
    category: string;
    description: string;
  };
}

interface AgentFormData {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  // agentCode artık departmana göre otomatik oluşturuluyor
  department: string;
  address: string;
  phone: string;
}

interface CustomerFormData {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  tcNo: string;
  address: string;
  phone: string;
}

interface AdminFormData {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  phone: string;
}

interface OfferFormData {
  customerId: number;
  agentId: number;
  insuranceTypeId: number;
  description: string;
  validUntil: string;
  coverageAmount: number;
}

interface OfferFormErrors {
  customerId?: string;
  agentId?: string;
  insuranceTypeId?: string;
  description?: string;
  validUntil?: string;
  coverageAmount?: string;
}

type SortOrder = 'asc' | 'desc';

const normalizeSortValue = (value: any) => {
  if (value === null || value === undefined) return '';
  if (typeof value === 'string') return value.toLowerCase();
  if (typeof value === 'number') return value;
  if (typeof value === 'boolean') return value ? 1 : 0;
  if (value instanceof Date) return value.getTime();
  if (typeof value === 'object' && typeof value?.getTime === 'function') {
    return value.getTime();
  }
  return value;
};

const getDateValue = (value?: string | Date | null) => {
  if (!value) return 0;
  const date = value instanceof Date ? value : new Date(value);
  return Number.isNaN(date.getTime()) ? 0 : date.getTime();
};

const AdminDashboard: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const navigate = useNavigate();
  const { user, token } = useAuth();
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [activeTab, setActiveTab] = useState(0);

  const [customers, setCustomers] = useState<Customer[]>([]);
  const [agents, setAgents] = useState<Agent[]>([]);
  const [offers, setOffers] = useState<Offer[]>([]);
  const [claims, setClaims] = useState<Claim[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Search and sort states for all panels
  const [offerSearch, setOfferSearch] = useState('');
  const [offerSortBy, setOfferSortBy] = useState<'offerId' | 'description' | 'customer' | 'customerEmail' | 'insuranceType' | 'basePrice' | 'discountRate' | 'price' | 'coverageAmount' | 'requestedStartDate' | 'status' | 'isCustomerApproved' | 'date'>('date');
  const [offerSortOrder, setOfferSortOrder] = useState<'asc' | 'desc'>('desc');
  
  const [agentSearch, setAgentSearch] = useState('');
  const [agentSortBy, setAgentSortBy] = useState<'name' | 'code' | 'department' | 'email' | 'phone' | 'address'>('name');
  const [agentSortOrder, setAgentSortOrder] = useState<'asc' | 'desc'>('asc');
  
  const [customerSearch, setCustomerSearch] = useState('');
  const [customerSortBy, setCustomerSortBy] = useState<'name' | 'date' | 'email' | 'phone' | 'idNo' | 'address'>('name');
  const [customerSortOrder, setCustomerSortOrder] = useState<'asc' | 'desc'>('asc');
  
  const [claimSearch, setClaimSearch] = useState('');
  const [claimFilterStatus, setClaimFilterStatus] = useState<'all' | 'Pending' | 'Approved' | 'Rejected'>('all');
  const [claimSortBy, setClaimSortBy] = useState<'claimId' | 'policyNumber' | 'customer' | 'customerEmail' | 'type' | 'description' | 'incidentDate' | 'status' | 'amount' | 'notes' | 'processedBy' | 'processedByEmail' | 'processedByPhone' | 'date'>('date');
  const [claimSortOrder, setClaimSortOrder] = useState<'asc' | 'desc'>('desc');
  
  // Offer management states
  const [selectedOffer, setSelectedOffer] = useState<Offer | null>(null);
  
  // Agent registration modal state
  const [openAgentModal, setOpenAgentModal] = useState(false);
  const [agentFormData, setAgentFormData] = useState<AgentFormData>({
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    department: '',
    address: '',
    phone: ''
  });
  const [agentFormErrors, setAgentFormErrors] = useState<Partial<AgentFormData>>({});
  const [isSubmittingAgent, setIsSubmittingAgent] = useState(false);
  
  // Agent Edit Form State
  const [agentEditFormData, setAgentEditFormData] = useState({
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    department: '',
    address: '',
    phone: ''
  });

  // Customer Edit Form State
  const [customerEditFormData, setCustomerEditFormData] = useState({
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    idNo: '',
    address: '',
    phone: ''
  });

  // Customer modal states
  const [openCustomerModal, setOpenCustomerModal] = useState(false);
  const [customerFormData, setCustomerFormData] = useState<CustomerFormData>({
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    tcNo: '',
    address: '',
    phone: ''
  });
  const [customerFormErrors, setCustomerFormErrors] = useState<Partial<CustomerFormData>>({});
  const [isSubmittingCustomer, setIsSubmittingCustomer] = useState(false);

  // Agent detail/edit/delete states
  const [selectedAgent, setSelectedAgent] = useState<Agent | null>(null);
  const [openAgentDetailModal, setOpenAgentDetailModal] = useState(false);
  const [openAgentEditModal, setOpenAgentEditModal] = useState(false);
  const [openAgentDeleteDialog, setOpenAgentDeleteDialog] = useState(false);
  const [agentToDelete, setAgentToDelete] = useState<Agent | null>(null);

  // Customer detail/edit/delete states
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [openCustomerDetailModal, setOpenCustomerDetailModal] = useState(false);
  const [openCustomerEditModal, setOpenCustomerEditModal] = useState(false);
  const [openCustomerDeleteDialog, setOpenCustomerDeleteDialog] = useState(false);
  const [customerToDelete, setCustomerToDelete] = useState<Customer | null>(null);

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
    coverageAmount: number;
    policyPdfFile: File | null;
  }>({
    basePrice: '',
    discountRate: '',
    status: '',
    coverageAmount: 0,
    policyPdfFile: null
  });
  const [savingOffer, setSavingOffer] = useState<number | null>(null);

  // Claim inline edit states
  const [editingClaim, setEditingClaim] = useState<number | null>(null);
  const [inlineClaimEditData, setInlineClaimEditData] = useState<{
    status: string;
    approvedAmount: string;
    notes: string;
  }>({
    status: '',
    approvedAmount: '',
    notes: ''
  });
  const [savingClaim, setSavingClaim] = useState<number | null>(null);
  const [editingClaimNotes, setEditingClaimNotes] = useState<number | null>(null);

  // Inline edit functions
  const handleEditOffer = (offer: Offer) => {
    setEditingOffer(offer.offerId);
    setInlineEditData({
      basePrice: offer.basePrice.toString(),
      discountRate: offer.discountRate ? offer.discountRate.toString() : '',
      status: offer.status,
      coverageAmount: offer.coverageAmount || 0,
      policyPdfFile: null
    });
  };

  const handleCancelEdit = () => {
    setEditingOffer(null);
    setInlineEditData({
      basePrice: '',
      discountRate: '',
      status: '',
      coverageAmount: 0,
      policyPdfFile: null
    });
  };

  // Claim inline edit functions
  const handleEditClaim = (claim: Claim) => {
    setEditingClaim(claim.claimId);
    setInlineClaimEditData({
      status: claim.status,
      approvedAmount: claim.approvedAmount?.toString() || '',
      notes: claim.notes || ''
    });
  };

  const handleCancelClaimEdit = () => {
    setEditingClaim(null);
    setInlineClaimEditData({
      status: '',
      approvedAmount: '',
      notes: ''
    });
  };

  const handleSaveClaim = async (claimId: number) => {
    try {
      setSavingClaim(claimId);
      
      const updateData = {
        status: inlineClaimEditData.status,
        approvedAmount: inlineClaimEditData.approvedAmount ? parseFloat(inlineClaimEditData.approvedAmount) : undefined,
        notes: inlineClaimEditData.notes
      };

      console.log('💾 Saving claim inline:', { claimId, updateData });
      
      await apiService.updateClaim(claimId, updateData);
      
      // Claims'leri yeniden yükle
      await fetchAdminData();
      
      setEditingClaim(null);
      setInlineClaimEditData({
        status: '',
        approvedAmount: '',
        notes: ''
      });
      
      console.log('✅ Claim updated successfully');
    } catch (error) {
      console.error('❌ Error updating claim:', error);
      alert('Olay bildirimi güncellenirken hata oluştu');
    } finally {
      setSavingClaim(null);
    }
  };

  const handleSaveOffer = async (offerId: number) => {
    try {
      setSavingOffer(offerId);
      
      // Final fiyatı hesapla
      const basePrice = parseFloat(inlineEditData.basePrice) || 0;
      const discountRate = parseFloat(inlineEditData.discountRate) || 0;
      const coverageAmount = inlineEditData.coverageAmount || 0;
      const finalPrice = calculateFinalPrice(basePrice, coverageAmount, discountRate);
      
      const updateData = {
        basePrice: basePrice,
        discountRate: discountRate,
        status: inlineEditData.status,
        coverageAmount: coverageAmount,
        finalPrice: finalPrice
      };

      console.log('💾 Saving offer inline:', { offerId, updateData });
      console.log('📤 Sending to backend:', {
        basePrice: updateData.basePrice,
        discountRate: updateData.discountRate,
        status: updateData.status,
        finalPrice: updateData.finalPrice
      });
      
      await apiService.updateOffer(offerId, updateData);
      
      // PDF dosyası varsa yükle
      if (inlineEditData.policyPdfFile) {
        console.log('📄 Uploading policy PDF:', (inlineEditData.policyPdfFile as File).name);
        await apiService.uploadPolicyPdf(offerId, inlineEditData.policyPdfFile as File);
        console.log('✅ Policy PDF uploaded successfully');
      }
      
      // Teklifleri yeniden yükle
      await fetchAdminData();
      
      setEditingOffer(null);
      setInlineEditData({
        basePrice: '',
        discountRate: '',
        status: '',
        coverageAmount: 0,
        policyPdfFile: null
      });
      
      console.log('✅ Offer updated successfully');
    } catch (error) {
      console.error('❌ Error updating offer:', error);
      alert('Teklif güncellenirken hata oluştu');
    } finally {
      setSavingOffer(null);
    }
  };

  // View offer handler
  const handleViewOffer = (offer: Offer) => {
    console.log('🔍 Viewing Offer:', offer);
    console.log('🔍 Additional Data:', offer.additionalData);
    
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
      console.log('✅ Offer deleted successfully');
      
      // Refresh offers list
      await fetchAdminData();
      
      setDeleteModalOpen(false);
      setOfferToDelete(null);
    } catch (error) {
      console.error('❌ Error deleting offer:', error);
      alert('Teklif silinirken hata oluştu');
    }
  };

  // Offer modal states
  const [openOfferModal, setOpenOfferModal] = useState(false);
  const [offerFormData, setOfferFormData] = useState<OfferFormData>({
    customerId: 0,
    agentId: 0,
    insuranceTypeId: 0,
    description: '',
    validUntil: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0], // 30 gün sonra
    coverageAmount: 0 // Temel kapsam (0%)
  });
  const [offerFormErrors, setOfferFormErrors] = useState<OfferFormErrors>({});
  const [isSubmittingOffer, setIsSubmittingOffer] = useState(false);

  // Admin registration modal states
  const [openAdminModal, setOpenAdminModal] = useState(false);
  const [adminFormData, setAdminFormData] = useState<AdminFormData>({
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    phone: ''
  });
  const [adminFormErrors, setAdminFormErrors] = useState<Partial<AdminFormData>>({});
  const [isSubmittingAdmin, setIsSubmittingAdmin] = useState(false);

  // Dashboard view modal states
  const [dashboardModalOpen, setDashboardModalOpen] = useState(false);
  const [dashboardViewType, setDashboardViewType] = useState<'customer' | 'agent' | null>(null);
  const [selectedDashboardUser, setSelectedDashboardUser] = useState<Customer | Agent | null>(null);
  const [dashboardData, setDashboardData] = useState<{
    policies?: any[];
    claims?: any[];
    offers?: any[];
    documents?: any[];
    stats?: any;
  }>({});
  const [dashboardLoading, setDashboardLoading] = useState(false);
  const [dashboardTabValue, setDashboardTabValue] = useState(0);
  
  // Document view modal states
  const [documentViewModalOpen, setDocumentViewModalOpen] = useState(false);
  const [selectedDocument, setSelectedDocument] = useState<any>(null);

  // Dashboard modal search states
  const [dashboardPolicySearch, setDashboardPolicySearch] = useState('');
  const [dashboardOfferSearch, setDashboardOfferSearch] = useState('');
  const [dashboardClaimSearch, setDashboardClaimSearch] = useState('');
  const [dashboardDocumentSearch, setDashboardDocumentSearch] = useState('');

  // Dashboard modal sort states
  const [dashboardPolicySortBy, setDashboardPolicySortBy] = useState<'policyNumber' | 'insuranceType' | 'startDate' | 'endDate' | 'premium' | 'status'>('policyNumber');
  const [dashboardPolicySortOrder, setDashboardPolicySortOrder] = useState<SortOrder>('asc');
  const [dashboardOfferSortBy, setDashboardOfferSortBy] = useState<'offerId' | 'customer' | 'insuranceType' | 'price' | 'status' | 'date'>('date');
  const [dashboardOfferSortOrder, setDashboardOfferSortOrder] = useState<SortOrder>('desc');
  const [dashboardClaimSortBy, setDashboardClaimSortBy] = useState<'claimId' | 'policyNumber' | 'type' | 'status' | 'date'>('date');
  const [dashboardClaimSortOrder, setDashboardClaimSortOrder] = useState<SortOrder>('desc');
  const [dashboardDocumentSortBy, setDashboardDocumentSortBy] = useState<'fileName' | 'category' | 'size' | 'date'>('date');
  const [dashboardDocumentSortOrder, setDashboardDocumentSortOrder] = useState<SortOrder>('desc');

  const handleDashboardPolicySort = (field: typeof dashboardPolicySortBy) => {
    if (dashboardPolicySortBy === field) {
      setDashboardPolicySortOrder(prev => (prev === 'asc' ? 'desc' : 'asc'));
    } else {
      setDashboardPolicySortBy(field);
      setDashboardPolicySortOrder('asc');
    }
  };

  const handleDashboardOfferSort = (field: typeof dashboardOfferSortBy) => {
    if (dashboardOfferSortBy === field) {
      setDashboardOfferSortOrder(prev => (prev === 'asc' ? 'desc' : 'asc'));
    } else {
      setDashboardOfferSortBy(field);
      setDashboardOfferSortOrder('asc');
    }
  };

  const handleDashboardClaimSort = (field: typeof dashboardClaimSortBy) => {
    if (dashboardClaimSortBy === field) {
      setDashboardClaimSortOrder(prev => (prev === 'asc' ? 'desc' : 'asc'));
    } else {
      setDashboardClaimSortBy(field);
      setDashboardClaimSortOrder('asc');
    }
  };

  const handleDashboardDocumentSort = (field: typeof dashboardDocumentSortBy) => {
    if (dashboardDocumentSortBy === field) {
      setDashboardDocumentSortOrder(prev => (prev === 'asc' ? 'desc' : 'asc'));
    } else {
      setDashboardDocumentSortBy(field);
      setDashboardDocumentSortOrder('asc');
    }
  };

  const getPolicySortValue = (policy: any, field: typeof dashboardPolicySortBy) => {
    switch (field) {
      case 'insuranceType':
        return policy.insuranceTypeName || '';
      case 'startDate':
        return getDateValue(policy.startDate);
      case 'endDate':
        return getDateValue(policy.endDate);
      case 'premium':
        return policy.totalPremium ?? 0;
      case 'status':
        return policy.status || '';
      case 'policyNumber':
      default:
        return policy.policyNumber || '';
    }
  };

  const getOfferSortValue = (offer: any, field: typeof dashboardOfferSortBy) => {
    switch (field) {
      case 'customer':
        return offer.customerName || offer.customer?.user?.name || '';
      case 'insuranceType':
        return offer.insuranceTypeName || '';
      case 'price':
        return offer.finalPrice ?? 0;
      case 'status':
        return offer.status || '';
      case 'date':
        return getDateValue(offer.createdAt);
      case 'offerId':
      default:
        return offer.offerId ?? 0;
    }
  };

  const getClaimSortValue = (claim: any, field: typeof dashboardClaimSortBy) => {
    switch (field) {
      case 'policyNumber':
        return claim.policyNumber || '';
      case 'type':
        return claim.type || '';
      case 'status':
        return claim.status || '';
      case 'date':
        return getDateValue(claim.createdAt);
      case 'claimId':
      default:
        return claim.claimId ?? 0;
    }
  };

  const getDocumentSortValue = (document: any, field: typeof dashboardDocumentSortBy) => {
    switch (field) {
      case 'category':
        return document.category || '';
      case 'size':
        return document.fileSize ?? 0;
      case 'date':
        return getDateValue(document.uploadedAt);
      case 'fileName':
      default:
        return document.fileName || document.name || '';
    }
  };

  const filteredPolicies = useMemo(() => {
    if (!dashboardData.policies) return [];
    const searchTerm = dashboardPolicySearch.trim().toLowerCase();
    const filtered = dashboardData.policies.filter((policy: any) => {
      if (!searchTerm) return true;
      const matches = [
        policy.policyNumber,
        policy.insuranceTypeName,
        policy.status,
        policy.policyId,
        policy.policyNumber,
        policy.totalPremium,
        policy.policyNumber,
        policy.policyNumber,
        policy.customerName,
      ].some(value => (value ?? '').toString().toLowerCase().includes(searchTerm));
      if (matches) return true;
      const dateTexts = [policy.startDate, policy.endDate]
        .filter(Boolean)
        .map((dateValue: string) => new Date(dateValue).toLocaleDateString('tr-TR').toLowerCase());
      return dateTexts.some(text => text.includes(searchTerm));
    });

    return filtered.sort((a: any, b: any) => {
      const valueA = normalizeSortValue(getPolicySortValue(a, dashboardPolicySortBy));
      const valueB = normalizeSortValue(getPolicySortValue(b, dashboardPolicySortBy));
      if (valueA < valueB) return dashboardPolicySortOrder === 'asc' ? -1 : 1;
      if (valueA > valueB) return dashboardPolicySortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }, [dashboardData.policies, dashboardPolicySearch, dashboardPolicySortBy, dashboardPolicySortOrder]);

  const filteredOffers = useMemo(() => {
    if (!dashboardData.offers) return [];
    const searchTerm = dashboardOfferSearch.trim().toLowerCase();
    const filtered = dashboardData.offers.filter((offer: any) => {
      if (!searchTerm) return true;
      const matches = [
        offer.offerId,
        offer.insuranceTypeName,
        offer.status,
        offer.finalPrice,
        offer.customerName,
        offer.department,
      ].some(value => (value ?? '').toString().toLowerCase().includes(searchTerm));
      if (matches) return true;
      if (offer.createdAt) {
        const createdAtText = new Date(offer.createdAt).toLocaleDateString('tr-TR').toLowerCase();
        return createdAtText.includes(searchTerm);
      }
      return false;
    });

    return filtered.sort((a: any, b: any) => {
      const valueA = normalizeSortValue(getOfferSortValue(a, dashboardOfferSortBy));
      const valueB = normalizeSortValue(getOfferSortValue(b, dashboardOfferSortBy));
      if (valueA < valueB) return dashboardOfferSortOrder === 'asc' ? -1 : 1;
      if (valueA > valueB) return dashboardOfferSortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }, [dashboardData.offers, dashboardOfferSearch, dashboardOfferSortBy, dashboardOfferSortOrder]);

  const filteredClaims = useMemo(() => {
    if (!dashboardData.claims) return [];
    const searchTerm = dashboardClaimSearch.trim().toLowerCase();
    const filtered = dashboardData.claims.filter((claim: any) => {
      if (!searchTerm) return true;
      const matches = [
        claim.claimId,
        claim.policyNumber,
        claim.type,
        claim.status,
        claim.description,
      ].some(value => (value ?? '').toString().toLowerCase().includes(searchTerm));
      if (matches) return true;
      if (claim.createdAt) {
        const createdAtText = new Date(claim.createdAt).toLocaleDateString('tr-TR').toLowerCase();
        return createdAtText.includes(searchTerm);
      }
      return false;
    });

    return filtered.sort((a: any, b: any) => {
      const valueA = normalizeSortValue(getClaimSortValue(a, dashboardClaimSortBy));
      const valueB = normalizeSortValue(getClaimSortValue(b, dashboardClaimSortBy));
      if (valueA < valueB) return dashboardClaimSortOrder === 'asc' ? -1 : 1;
      if (valueA > valueB) return dashboardClaimSortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }, [dashboardData.claims, dashboardClaimSearch, dashboardClaimSortBy, dashboardClaimSortOrder]);

  const filteredDocuments = useMemo(() => {
    if (!dashboardData.documents) return [];
    const searchTerm = dashboardDocumentSearch.trim().toLowerCase();
    const filtered = dashboardData.documents.filter((doc: any) => {
      if (!searchTerm) return true;
      const matches = [
        doc.fileName,
        doc.name,
        doc.category,
        doc.fileType,
      ].some(value => (value ?? '').toString().toLowerCase().includes(searchTerm));
      if (matches) return true;
      if (doc.uploadedAt) {
        const uploadedText = new Date(doc.uploadedAt).toLocaleDateString('tr-TR').toLowerCase();
        return uploadedText.includes(searchTerm);
      }
      return false;
    });

    return filtered.sort((a: any, b: any) => {
      const valueA = normalizeSortValue(getDocumentSortValue(a, dashboardDocumentSortBy));
      const valueB = normalizeSortValue(getDocumentSortValue(b, dashboardDocumentSortBy));
      if (valueA < valueB) return dashboardDocumentSortOrder === 'asc' ? -1 : 1;
      if (valueA > valueB) return dashboardDocumentSortOrder === 'asc' ? 1 : -1;
      return 0;
    });
  }, [dashboardData.documents, dashboardDocumentSearch, dashboardDocumentSortBy, dashboardDocumentSortOrder]);

  // Stats state
  const [stats, setStats] = useState([
    { title: 'Toplam Müşteri', value: '0', change: '+0%', trend: 'up', color: '#4caf50' },
    { title: 'Toplam Acenta', value: '0', change: '+0%', trend: 'up', color: '#2196f3' },
    { title: 'Aktif Teklifler', value: '0', change: '+0%', trend: 'up', color: '#ff9800' },
  ]);

  const fetchAdminData = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      console.log('🔍 Fetching admin data...');
      console.log('🔑 Token:', token);
      console.log('👤 User:', user);

      // Fetch all customers using API service
      console.log('📞 Fetching customers...');
      const customersData = await apiService.getCustomers();
      console.log('✅ Customers fetched:', customersData);
      setCustomers(customersData);

      // Fetch all agents using API service
      console.log('👥 Fetching agents...');
      const agentsData = await apiService.getAgents();
      console.log('✅ Agents fetched:', agentsData);
      setAgents(agentsData);

      // Fetch all offers using API service
      console.log('📋 Fetching offers...');
      const offersData = await apiService.getOffers();
      console.log('✅ Offers fetched:', offersData);
      setOffers(offersData);

      // Fetch all claims using API service
      console.log('📝 Fetching claims...');
      const claimsData = await apiService.getClaims();
      console.log('✅ Claims fetched:', claimsData);
      setClaims(claimsData);

      // Update stats with real data
      setStats(prevStats => [
        { ...prevStats[0], value: customersData.length.toString() },
        { ...prevStats[1], value: agentsData.length.toString() },
        { ...prevStats[2], value: offersData.filter((o: Offer) => o.status === 'pending').length.toString() }
      ]);

      console.log('🎯 Stats updated');

    } catch (err) {
      console.error('❌ Error in fetchAdminData:', err);
      setError(err instanceof Error ? err.message : 'Bir hata oluştu');
    } finally {
      setLoading(false);
    }
  }, [token, user]);

  useEffect(() => {
    if (token && user) {
      fetchAdminData();
      
      // SignalR bağlantısını başlat (hata yönetimi ile)
      signalRService.startConnection().catch((error) => {
        console.warn('⚠️ SignalR connection failed (non-critical):', error.message);
        // SignalR hatası kritik değil, uygulama çalışmaya devam eder
      });
      
      // Real-time event listeners
      signalRService.onNotificationReceived((message) => {
        console.log('📢 Notification received:', message);
        // Bildirim göster
      });
      
      signalRService.onOfferStatusChanged((data) => {
        console.log('📊 Offer status changed:', data);
        // Teklif durumu güncelle
        fetchAdminData(); // Verileri yenile
      });
      
      signalRService.onNewOffer((data) => {
        console.log('🆕 New offer:', data);
        // Yeni teklif bildirimi
        fetchAdminData(); // Verileri yenile
      });
      
      signalRService.onStatsUpdate((stats) => {
        console.log('📈 Stats updated:', stats);
        // İstatistikleri güncelle
        setStats(stats);
      });
    }
    
    return () => {
      // Cleanup
      try {
      signalRService.stopConnection();
      } catch (error) {
        console.warn('⚠️ SignalR disconnect warning (non-critical):', error);
      }
    };
  }, [token, user, fetchAdminData]);

  const handleLogout = () => {
    navigate('/customer-login');
  };

  const toggleDrawer = () => {
    setDrawerOpen(!drawerOpen);
  };

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  // Agent form handling functions
  const handleAgentInputChange = (field: keyof AgentFormData, value: string) => {
    setAgentFormData(prev => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (agentFormErrors[field]) {
      setAgentFormErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  const handleCustomerInputChange = (field: keyof CustomerFormData, value: string) => {
    setCustomerFormData(prev => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (customerFormErrors[field]) {
      setCustomerFormErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  // Fiyat hesaplama fonksiyonu - temel fiyat 0 olsa bile kapsam oranı ile çarpılır
  const calculateFinalPrice = (basePrice: number, coverageAmount: number, discountRate: number = 0): number => {
    // Temel fiyat 0 olsa bile kapsam oranı ile çarpılır
    const priceWithCoverage = basePrice * (1 + coverageAmount / 100);
    
    // İndirim oranını uygula
    const finalPrice = priceWithCoverage * (1 - discountRate / 100);
    
    return Math.max(0, finalPrice);
  };

  const handleOfferInputChange = (field: keyof OfferFormData, value: string | number) => {
    setOfferFormData(prev => ({ ...prev, [field]: value }));
    
    // Clear error when user starts typing
    if (offerFormErrors[field]) {
      setOfferFormErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  // Admin form handling functions
  const handleAdminInputChange = (field: keyof AdminFormData, value: string) => {
    setAdminFormData(prev => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (adminFormErrors[field]) {
      setAdminFormErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  const validateAgentForm = (): boolean => {
    const errors: Partial<AgentFormData> = {};
    
    if (!agentFormData.name.trim()) errors.name = 'Ad alanı zorunludur';
    if (!agentFormData.email.trim()) errors.email = 'Email alanı zorunludur';
    if (!agentFormData.password.trim()) errors.password = 'Şifre alanı zorunludur';
    if (agentFormData.password.length < 6) errors.password = 'Şifre en az 6 karakter olmalıdır';
    if (!agentFormData.confirmPassword.trim()) errors.confirmPassword = 'Şifre tekrarı zorunludur';
    if (agentFormData.password !== agentFormData.confirmPassword) errors.confirmPassword = 'Şifreler eşleşmiyor';
    // agentCode artık otomatik oluşturulduğu için kontrol edilmiyor
    if (!agentFormData.department.trim()) errors.department = 'Departman alanı zorunludur';
    if (!agentFormData.address.trim()) errors.address = 'Adres alanı zorunludur';
    if (!agentFormData.phone.trim()) errors.phone = 'Telefon alanı zorunludur';
    
    setAgentFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const validateCustomerForm = (): boolean => {
    const errors: Partial<CustomerFormData> = {};
    
    if (!customerFormData.name.trim()) errors.name = 'Ad Soyad alanı zorunludur';
    if (!customerFormData.email.trim()) errors.email = 'Email alanı zorunludur';
    if (!customerFormData.password.trim()) errors.password = 'Şifre alanı zorunludur';
    if (customerFormData.password.length < 6) errors.password = 'Şifre en az 6 karakter olmalıdır';
    if (customerFormData.password !== customerFormData.confirmPassword) errors.confirmPassword = 'Şifreler eşleşmiyor';
    if (!customerFormData.tcNo.trim()) errors.tcNo = 'TC Kimlik No alanı zorunludur';
    if (customerFormData.tcNo.length !== 11) errors.tcNo = 'TC Kimlik No 11 haneli olmalıdır';
    if (!customerFormData.address.trim()) errors.address = 'Adres alanı zorunludur';
    if (!customerFormData.phone.trim()) errors.phone = 'Telefon alanı zorunludur';
    
    setCustomerFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const validateAdminForm = (): boolean => {
    const errors: Partial<AdminFormData> = {};
    
    if (!adminFormData.name.trim()) errors.name = 'Ad Soyad alanı zorunludur';
    if (!adminFormData.email.trim()) errors.email = 'Email alanı zorunludur';
    if (!adminFormData.password.trim()) errors.password = 'Şifre alanı zorunludur';
    if (adminFormData.password.length < 6) errors.password = 'Şifre en az 6 karakter olmalıdır';
    if (!adminFormData.confirmPassword.trim()) errors.confirmPassword = 'Şifre tekrarı zorunludur';
    if (adminFormData.password !== adminFormData.confirmPassword) errors.confirmPassword = 'Şifreler eşleşmiyor';
    if (!adminFormData.phone.trim()) errors.phone = 'Telefon alanı zorunludur';
    
    setAdminFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  // Offer management functions

  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'pending': return '#ff9800';
      case 'approved': return '#4caf50';
      case 'rejected': return '#f44336';
      case 'processed': return '#2196f3';
      default: return '#757575';
    }
  };

  const getStatusText = (status: string) => {
    switch (status?.toLowerCase()) {
      case 'pending': return 'Beklemede';
      case 'approved': return 'Onaylandı';
      case 'rejected': return 'Reddedildi';
      case 'processed': return 'İşlendi';
      default: return 'Bilinmiyor';
    }
  };

  const validateOfferForm = (): boolean => {
    const errors: OfferFormErrors = {};
    
    if (!offerFormData.customerId) errors.customerId = 'Müşteri seçimi zorunludur';
    if (!offerFormData.agentId) errors.agentId = 'Acenta seçimi zorunludur';
    if (!offerFormData.insuranceTypeId) errors.insuranceTypeId = 'Sigorta türü seçimi zorunludur';
    if (!offerFormData.validUntil) errors.validUntil = 'Geçerlilik tarihi zorunludur';
    
    setOfferFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmitAgent = async () => {
    if (!validateAgentForm()) return;
    
    setIsSubmittingAgent(true);
    try {
      // Backend'in beklediği formata dönüştür
      // agentCode artık backend'de departmana göre otomatik oluşturuluyor
      const agentData = {
        name: agentFormData.name,
        email: agentFormData.email,
        password: agentFormData.password,
        confirmPassword: agentFormData.confirmPassword,
        address: agentFormData.address,
        phone: agentFormData.phone,
        department: agentFormData.department // Departmana göre kod otomatik oluşturulacak
      };

      console.log('Sending agent data:', agentData);

      const response = await fetch('/api/Auth/register/agent', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(agentData)
      });

      console.log('Response status:', response.status);
      console.log('Response headers:', Object.fromEntries(response.headers.entries()));

      if (response.ok) {
        // Reset form and close modal
        setAgentFormData({
          name: '',
          email: '',
          password: '',
          confirmPassword: '',
          department: '',
          address: '',
          phone: ''
        });
        setOpenAgentModal(false);
        
        // Refresh agents list
        fetchAdminData();
        
        // Show success message (you can add a snackbar here)
        alert('Acenta başarıyla eklendi!');
      } else {
        console.error('Agent registration error response:', response.status);
        
        let errorMessage = 'Acenta eklenirken bir hata oluştu';
        
        try {
          const errorData = await response.json();
          if (errorData.message) {
            errorMessage = `Sunucu Hatası: ${errorData.message}`;
          }
        } catch (parseError) {
          // JSON parse edilemiyorsa, status code'a göre hata mesajı
          switch (response.status) {
            case 400:
              errorMessage = 'Geçersiz veri formatı veya eksik bilgi';
              break;
            case 401:
              errorMessage = 'Yetkilendirme hatası - Lütfen tekrar giriş yapın';
              break;
            case 403:
              errorMessage = 'Bu işlem için yetkiniz bulunmuyor';
              break;
            case 404:
              errorMessage = 'API endpoint bulunamadı - Lütfen sistem yöneticisi ile iletişime geçin';
              break;
            case 500:
              errorMessage = 'Sunucu hatası - Lütfen daha sonra tekrar deneyin';
              break;
            default:
              errorMessage = `Beklenmeyen hata (${response.status})`;
          }
        }
        
        alert(`❌ ${errorMessage}\n\nStatus Code: ${response.status}\nEndpoint: /api/Auth/register/agent`);
      }
    } catch (error) {
      console.error('Agent registration error:', error);
      console.error('Error details:', {
        message: error instanceof Error ? error.message : 'Unknown error',
        stack: error instanceof Error ? error.stack : 'No stack trace'
      });
      
      let errorMessage = 'Acenta eklenirken bir hata oluştu';
      
      if (error instanceof TypeError && error.message.includes('fetch')) {
        errorMessage = 'Ağ bağlantısı hatası - Backend sunucusuna ulaşılamıyor';
      } else if (error instanceof Error) {
        errorMessage = `Sistem Hatası: ${error.message}`;
      }
      
      alert(`❌ ${errorMessage}\n\nHata Detayı: ${error instanceof Error ? error.message : 'Bilinmeyen hata'}`);
    } finally {
      setIsSubmittingAgent(false);
    }
  };

  const handleCloseAgentModal = () => {
    setOpenAgentModal(false);
    setAgentFormData({
      name: '',
      email: '',
      password: '',
      confirmPassword: '',
      department: '',
      address: '',
      phone: ''
    });
    setAgentFormErrors({});
  };

  const handleCloseCustomerModal = () => {
    setOpenCustomerModal(false);
    setCustomerFormData({
      name: '',
      email: '',
      password: '',
      confirmPassword: '',
      tcNo: '',
      address: '',
      phone: ''
    });
    setCustomerFormErrors({});
  };

  // Agent Detail/Edit/Delete Handlers
  const handleViewAgent = async (agent: Agent) => {
    setSelectedDashboardUser(agent);
    setDashboardViewType('agent');
    setDashboardTabValue(0); // Reset to first tab
    setDashboardModalOpen(true);
    await fetchAgentDashboardData(agent);
  };

  const handleEditAgent = (agent: Agent) => {
    console.log('🔍 handleEditAgent - Agent:', agent);
    console.log('🔍 user.name:', agent.user?.name);
    console.log('🔍 user.email:', agent.user?.email);
    
    setSelectedAgent(agent);
    // Pre-fill form with agent data
    const formData = {
      name: agent.user?.name || '',
      email: agent.user?.email || '',
      password: '',
      confirmPassword: '',
      department: agent.department || '',
      address: agent.address || '',
      phone: agent.phone || ''
    };
    
    console.log('🔍 Setting form data:', formData);
    setAgentEditFormData(formData);
    setOpenAgentEditModal(true);
  };

  const handleDeleteAgentClick = (agent: Agent) => {
    setAgentToDelete(agent);
    setOpenAgentDeleteDialog(true);
  };

  const handleDeleteAgentConfirm = async () => {
    if (!agentToDelete) return;
    
    try {
      await apiService.deleteAgent(agentToDelete.id);
      alert('Acenta başarıyla silindi!');
      setOpenAgentDeleteDialog(false);
      setAgentToDelete(null);
      fetchAdminData(); // Refresh data
    } catch (error) {
      console.error('Agent deletion error:', error);
      alert('Acenta silinirken hata oluştu!');
    }
  };

  // Customer Detail/Edit/Delete Handlers
  const handleViewCustomer = async (customer: Customer) => {
    setSelectedDashboardUser(customer);
    setDashboardViewType('customer');
    setDashboardTabValue(0); // Reset to first tab
    setDashboardModalOpen(true);
    await fetchCustomerDashboardData(customer);
  };

  const handleEditCustomer = (customer: Customer) => {
    console.log('🔍 handleEditCustomer - Customer:', customer);
    console.log('🔍 user.name:', customer.user?.name);
    console.log('🔍 user.email:', customer.user?.email);
    
    setSelectedCustomer(customer);
    // Pre-fill form with customer data
    const formData = {
      name: customer.user?.name || '',
      email: customer.user?.email || '',
      password: '',
      confirmPassword: '',
      idNo: customer.idNo || '',
      address: customer.address || '',
      phone: customer.phone || ''
    };
    
    console.log('🔍 Setting form data:', formData);
    setCustomerEditFormData(formData);
    setOpenCustomerEditModal(true);
  };

  const handleDeleteCustomerClick = (customer: Customer) => {
    setCustomerToDelete(customer);
    setOpenCustomerDeleteDialog(true);
  };

  const handleDeleteCustomerConfirm = async () => {
    if (!customerToDelete) return;
    
    try {
      await apiService.deleteCustomer(customerToDelete.id);
      alert('Müşteri başarıyla silindi!');
      setOpenCustomerDeleteDialog(false);
      setCustomerToDelete(null);
      fetchAdminData(); // Refresh data
    } catch (error) {
      console.error('Customer deletion error:', error);
      alert('Müşteri silinirken hata oluştu!');
    }
  };

  // Dashboard data fetching functions
  const fetchCustomerDashboardData = async (customer: Customer) => {
    setDashboardLoading(true);
    try {
      // Use admin-accessible endpoints - get all data and filter by customer
      const [allOffers, allClaims, allPolicies, allDocuments] = await Promise.all([
        apiService.getOffers().catch(() => []),
        apiService.getClaims().catch(() => []),
        apiService.getPolicies().catch(() => []),
        apiService.getDocuments().catch(() => [])
      ]);
      
      console.log('🔍 Customer Dashboard Data Fetch:', {
        customerId: customer.customerId,
        customerEmail: customer.user?.email,
        totalOffers: allOffers.length,
        totalClaims: allClaims.length,
        totalPolicies: allPolicies.length
      });
      
      // Filter offers by customer
      const offers = allOffers.filter((o: any) => {
        const matches = o.customerId === customer.customerId || 
                       o.customer?.user?.email === customer.user?.email ||
                       o.customerEmail === customer.user?.email;
        return matches;
      });
      
      console.log('✅ Filtered offers:', offers.length);
      
      // Get offer IDs for this customer
      const offerIds = offers.map((o: any) => o.offerId);
      
      // Filter policies - match by customer's offers or user
      const policies = allPolicies.filter((p: any) => {
        return offerIds.includes(p.offerId) || 
               p.userId === customer.userId ||
               p.customerId === customer.customerId;
      });
      
      console.log('✅ Filtered policies:', policies.length);
      
      // Get policy numbers from customer's policies
      const policyNumbers = policies.map((p: any) => p.policyNumber);
      
      // Filter claims - match by policy number or customer email
      const customerClaims = allClaims.filter((c: any) => {
        // Match by policy number
        if (c.policyNumber && policyNumbers.includes(c.policyNumber)) return true;
        // Match by customer email
        if (c.customerEmail === customer.user?.email) return true;
        // Match by customer ID if available
        if (c.customerId === customer.customerId) return true;
        // Match by policy's offer ID
        if (c.offerId && offerIds.includes(c.offerId)) return true;
        return false;
      });
      
      console.log('✅ Filtered claims:', customerClaims.length);
      
      const policyIds = policies.map((p: any) => p.policyId);
      const claimIds = customerClaims.map((c: any) => c.claimId);

      const documents = allDocuments.filter((doc: any) => {
        if (doc.customerId === customer.customerId) return true;
        if (doc.userId === customer.userId) return true;
        if (doc.policyId && policyIds.includes(doc.policyId)) return true;
        if (doc.policyNumber && policyNumbers.includes(doc.policyNumber)) return true;
        if (doc.claimId && claimIds.includes(doc.claimId)) return true;
        if (doc.customerName && doc.customerName === customer.user?.name) return true;
        return false;
      });

      console.log('✅ Filtered documents:', documents.length);
      
      // Calculate stats
      const stats = {
        totalPolicies: policies.length,
        activePolicies: policies.filter((p: any) => p.status === 'active' || p.status === 'Active').length,
        totalClaims: customerClaims.length,
        pendingClaims: customerClaims.filter((c: any) => c.status === 'Pending' || c.status === 'pending').length,
        approvedClaims: customerClaims.filter((c: any) => c.status === 'Approved' || c.status === 'approved').length,
        totalOffers: offers.length,
        pendingOffers: offers.filter((o: any) => o.status === 'pending' || o.status === 'Pending').length,
        approvedOffers: offers.filter((o: any) => o.status === 'approved' || o.status === 'Approved').length
      };
      
      console.log('📊 Stats calculated:', stats);
      
      setDashboardData({ policies, claims: customerClaims, documents, offers, stats });
    } catch (error) {
      console.error('Error fetching customer dashboard data:', error);
      setDashboardData({ policies: [], claims: [], documents: [], offers: [], stats: {} });
    } finally {
      setDashboardLoading(false);
    }
  };

  const fetchAgentDashboardData = async (agent: Agent) => {
    setDashboardLoading(true);
    try {
      // Use admin-accessible endpoints - get all data and filter by agent's department
      const [allOffers, allClaims, allPolicies] = await Promise.all([
        apiService.getOffers().catch(() => []),
        apiService.getClaims().catch(() => []),
        apiService.getPolicies().catch(() => [])
      ]);
      
      console.log('🔍 Agent Dashboard Data Fetch:', {
        agentId: agent.id,
        department: agent.department,
        totalOffers: allOffers.length,
        totalClaims: allClaims.length,
        totalPolicies: allPolicies.length
      });
      
      // Filter offers by agent's department
      const offers = allOffers.filter((o: any) => o.department === agent.department);
      
      console.log('✅ Filtered offers by department:', offers.length);
      
      // Get offer IDs for this department
      const offerIds = offers.map((o: any) => o.offerId);
      
      // Filter policies by offer IDs
      const departmentPolicies = allPolicies.filter((p: any) => offerIds.includes(p.offerId));
      const policyNumbers = departmentPolicies.map((p: any) => p.policyNumber);
      
      console.log('✅ Filtered policies:', departmentPolicies.length);
      
      // Filter claims - match by policy number or offer ID
      const claims = allClaims.filter((c: any) => {
        // Match by policy number
        if (c.policyNumber && policyNumbers.includes(c.policyNumber)) return true;
        // Match by offer ID
        if (c.offerId && offerIds.includes(c.offerId)) return true;
        // Match by department if available in claim
        if (c.department === agent.department) return true;
        return false;
      });
      
      console.log('✅ Filtered claims:', claims.length);
      
      // Calculate stats
      const stats = {
        department: agent.department,
        totalOffers: offers.length,
        pendingOffers: offers.filter((o: any) => o.status === 'pending' || o.status === 'Pending').length,
        approvedOffers: offers.filter((o: any) => o.status === 'approved' || o.status === 'Approved').length,
        totalClaims: claims.length,
        pendingClaims: claims.filter((c: any) => c.status === 'Pending' || c.status === 'pending').length,
        approvedClaims: claims.filter((c: any) => c.status === 'Approved' || c.status === 'approved').length
      };
      
      console.log('📊 Stats calculated:', stats);
      
      setDashboardData({ offers, claims, stats });
    } catch (error) {
      console.error('Error fetching agent dashboard data:', error);
      setDashboardData({ offers: [], claims: [], stats: {} });
    } finally {
      setDashboardLoading(false);
    }
  };

  const handleSubmitAdmin = async () => {
    if (!validateAdminForm()) return;
    
    setIsSubmittingAdmin(true);
    try {
      const adminData = {
        name: adminFormData.name,
        email: adminFormData.email,
        password: adminFormData.password,
        confirmPassword: adminFormData.confirmPassword,
        phone: adminFormData.phone
      };

      console.log('Sending admin data:', adminData);

      const result = await apiService.registerAdmin(adminData);
      console.log('Admin registration successful:', result);
        alert('✅ Admin kullanıcısı başarıyla eklendi!');
        
        // Reset form
        setAdminFormData({
          name: '',
          email: '',
          password: '',
          confirmPassword: '',
          phone: ''
        });
        setAdminFormErrors({});
        setOpenAdminModal(false);
        
        // Refresh data
        fetchAdminData();
    } catch (error) {
      console.error('Admin registration error:', error);
      
      let errorMessage = 'Admin eklenirken bir hata oluştu';
      
      if (error instanceof TypeError && error.message.includes('fetch')) {
        errorMessage = 'Ağ bağlantısı hatası - Backend sunucusuna ulaşılamıyor';
      } else if (error instanceof Error) {
        errorMessage = `Sistem Hatası: ${error.message}`;
      }
      
      alert(`❌ ${errorMessage}\n\nHata Detayı: ${error instanceof Error ? error.message : 'Bilinmeyen hata'}`);
    } finally {
      setIsSubmittingAdmin(false);
    }
  };

  const handleCloseAdminModal = () => {
    setOpenAdminModal(false);
    setAdminFormData({
      name: '',
      email: '',
      password: '',
      confirmPassword: '',
      phone: ''
    });
    setAdminFormErrors({});
  };

  const handleSubmitCustomer = async () => {
    if (!validateCustomerForm()) return;
    
    setIsSubmittingCustomer(true);
    
    try {
      const customerData = {
        name: customerFormData.name,
        email: customerFormData.email,
        password: customerFormData.password,
        confirmPassword: customerFormData.confirmPassword,
        tcNo: customerFormData.tcNo,
        address: customerFormData.address,
        phone: customerFormData.phone
      };

      console.log('Sending customer data:', customerData);

      const response = await fetch('/api/Auth/register/customer', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(customerData)
      });

      console.log('Response status:', response.status);
      console.log('Response headers:', response.headers);

      if (response.ok) {
        const result = await response.json();
        console.log('Customer registration successful:', result);
        alert('✅ Müşteri başarıyla eklendi!');
        handleCloseCustomerModal();
        fetchAdminData(); // Refresh data
      } else {
        let errorMessage = 'Müşteri eklenirken bir hata oluştu';
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorMessage;
        } catch {
          // If response is not JSON, use default message
        }
        alert(`❌ ${errorMessage}\n\nStatus Code: ${response.status}\nEndpoint: /api/Auth/register/customer`);
      }
    } catch (error) {
      console.error('Customer registration error:', error);
      alert(`❌ Müşteri eklenirken bir hata oluştu\n\nStatus Code: 0\nEndpoint: /api/Auth/register/customer`);
    } finally {
      setIsSubmittingCustomer(false);
    }
  };

  const handleSubmitOffer = async () => {
    if (!validateOfferForm()) return;
    
    setIsSubmittingOffer(true);
    
    try {
      const offerData = {
        customerId: offerFormData.customerId,
        agentId: offerFormData.agentId,
        insuranceTypeId: offerFormData.insuranceTypeId,
        description: offerFormData.description,
        discountRate: 0, // Admin yeni teklif oluştururken indirim 0
        validUntil: offerFormData.validUntil,
        coverageAmount: offerFormData.coverageAmount,
        basePrice: 0 // Temel fiyat her zaman 0
      };

      console.log('Sending offer data:', offerData);

      const response = await fetch('/api/Offer', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(offerData)
      });

      console.log('Response status:', response.status);
      console.log('Response headers:', response.headers);

      if (response.ok) {
        const result = await response.json();
        console.log('Offer creation successful:', result);
        alert('✅ Teklif başarıyla oluşturuldu!');
        handleCloseOfferModal();
        fetchAdminData(); // Refresh data
      } else {
        let errorMessage = 'Teklif oluşturulurken bir hata oluştu';
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorMessage;
        } catch {
          // If response is not JSON, use default message
        }
        alert(`❌ ${errorMessage}\n\nStatus Code: ${response.status}\nEndpoint: /api/Offer`);
      }
    } catch (error) {
      console.error('Offer creation error:', error);
      alert(`❌ Teklif oluşturulurken bir hata oluştu\n\nStatus Code: 0\nEndpoint: /api/Offer`);
    } finally {
      setIsSubmittingOffer(false);
    }
  };

  const handleCloseOfferModal = () => {
    setOpenOfferModal(false);
    setOfferFormData({
      customerId: 0,
      agentId: 0,
      insuranceTypeId: 0,
      description: '',
      validUntil: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
      coverageAmount: 0
    });
    setOfferFormErrors({});
  };

  // Sort handler functions
  const handleOfferSort = (sortBy: 'offerId' | 'description' | 'customer' | 'customerEmail' | 'insuranceType' | 'basePrice' | 'discountRate' | 'price' | 'coverageAmount' | 'requestedStartDate' | 'status' | 'isCustomerApproved' | 'date') => {
    if (offerSortBy === sortBy) {
      setOfferSortOrder(offerSortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setOfferSortBy(sortBy);
      setOfferSortOrder('asc');
    }
  };

  const handleAgentSort = (sortBy: 'name' | 'code' | 'department' | 'email' | 'phone' | 'address') => {
    if (agentSortBy === sortBy) {
      setAgentSortOrder(agentSortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setAgentSortBy(sortBy);
      setAgentSortOrder('asc');
    }
  };

  const handleCustomerSort = (sortBy: 'name' | 'date' | 'email' | 'phone' | 'idNo' | 'address') => {
    if (customerSortBy === sortBy) {
      setCustomerSortOrder(customerSortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setCustomerSortBy(sortBy);
      setCustomerSortOrder('asc');
    }
  };

  const handleClaimSort = (sortBy: 'claimId' | 'policyNumber' | 'customer' | 'customerEmail' | 'type' | 'description' | 'incidentDate' | 'status' | 'amount' | 'notes' | 'processedBy' | 'processedByEmail' | 'processedByPhone' | 'date') => {
    if (claimSortBy === sortBy) {
      setClaimSortOrder(claimSortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setClaimSortBy(sortBy);
      setClaimSortOrder('asc');
    }
  };

  // Filter and sort functions
  const getFilteredAndSortedOffers = () => {
    let filtered = [...offers];
    
    // Search filter
    if (offerSearch) {
      filtered = filtered.filter(offer => 
        offer.customerName?.toLowerCase().includes(offerSearch.toLowerCase()) ||
        offer.customerEmail?.toLowerCase().includes(offerSearch.toLowerCase()) ||
        offer.agentName?.toLowerCase().includes(offerSearch.toLowerCase()) ||
        offer.insuranceTypeName?.toLowerCase().includes(offerSearch.toLowerCase()) ||
        offer.offerId?.toString().includes(offerSearch)
      );
    }
    
    // Sort
    filtered.sort((a, b) => {
      let compareValue = 0;
      
      switch (offerSortBy) {
        case 'offerId':
          compareValue = (a.offerId || 0) - (b.offerId || 0);
          break;
        case 'description':
          compareValue = (a.description || '').localeCompare(b.description || '');
          break;
        case 'customer':
          compareValue = (a.customerName || '').localeCompare(b.customerName || '');
          break;
        case 'customerEmail':
          compareValue = (a.customerEmail || '').localeCompare(b.customerEmail || '');
          break;
        case 'insuranceType':
          compareValue = (a.insuranceTypeName || '').localeCompare(b.insuranceTypeName || '');
          break;
        case 'basePrice':
          compareValue = (a.basePrice || 0) - (b.basePrice || 0);
          break;
        case 'discountRate':
          compareValue = (a.discountRate || 0) - (b.discountRate || 0);
          break;
        case 'price':
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
        case 'date':
          compareValue = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
          break;
      }
      
      return offerSortOrder === 'asc' ? compareValue : -compareValue;
    });
    
    return filtered;
  };

  const getFilteredAndSortedAgents = () => {
    let filtered = [...agents].filter(a => a.user);
    
    // Search filter
    if (agentSearch) {
      filtered = filtered.filter(agent => 
        agent.user?.name?.toLowerCase().includes(agentSearch.toLowerCase()) ||
        agent.user?.email?.toLowerCase().includes(agentSearch.toLowerCase()) ||
        agent.agentCode?.toLowerCase().includes(agentSearch.toLowerCase()) ||
        agent.department?.toLowerCase().includes(agentSearch.toLowerCase()) ||
        agent.phone?.toLowerCase().includes(agentSearch.toLowerCase()) ||
        agent.address?.toLowerCase().includes(agentSearch.toLowerCase())
      );
    }
    
    // Sort
    filtered.sort((a, b) => {
      let compareValue = 0;
      
      switch (agentSortBy) {
        case 'name':
          compareValue = (a.user?.name || '').localeCompare(b.user?.name || '');
          break;
        case 'code':
          compareValue = (a.agentCode || '').localeCompare(b.agentCode || '');
          break;
        case 'department':
          compareValue = (a.department || '').localeCompare(b.department || '');
          break;
        case 'email':
          compareValue = (a.user?.email || '').localeCompare(b.user?.email || '');
          break;
        case 'phone':
          compareValue = (a.phone || '').localeCompare(b.phone || '');
          break;
        case 'address':
          compareValue = (a.address || '').localeCompare(b.address || '');
          break;
      }
      
      return agentSortOrder === 'asc' ? compareValue : -compareValue;
    });
    
    return filtered;
  };

  const getFilteredAndSortedCustomers = () => {
    let filtered = [...customers].filter(c => c.user);
    
    // Search filter
    if (customerSearch) {
      filtered = filtered.filter(customer => 
        customer.user?.name?.toLowerCase().includes(customerSearch.toLowerCase()) ||
        customer.user?.email?.toLowerCase().includes(customerSearch.toLowerCase()) ||
        customer.idNo?.toLowerCase().includes(customerSearch.toLowerCase()) ||
        customer.phone?.toLowerCase().includes(customerSearch.toLowerCase()) ||
        customer.address?.toLowerCase().includes(customerSearch.toLowerCase())
      );
    }
    
    // Sort
    filtered.sort((a, b) => {
      let compareValue = 0;
      
      switch (customerSortBy) {
        case 'name':
          compareValue = (a.user?.name || '').localeCompare(b.user?.name || '');
          break;
        case 'date':
          compareValue = new Date(a.user?.createdAt || 0).getTime() - new Date(b.user?.createdAt || 0).getTime();
          break;
        case 'email':
          compareValue = (a.user?.email || '').localeCompare(b.user?.email || '');
          break;
        case 'phone':
          compareValue = (a.phone || '').localeCompare(b.phone || '');
          break;
        case 'idNo':
          compareValue = (a.idNo || '').localeCompare(b.idNo || '');
          break;
        case 'address':
          compareValue = (a.address || '').localeCompare(b.address || '');
          break;
      }
      
      return customerSortOrder === 'asc' ? compareValue : -compareValue;
    });
    
    return filtered;
  };

  const getFilteredAndSortedClaims = () => {
    let filtered = [...claims];
    
    // Status filter
    if (claimFilterStatus !== 'all') {
      filtered = filtered.filter(claim => claim.status === claimFilterStatus);
    }
    
    // Search filter
    if (claimSearch) {
      filtered = filtered.filter(claim => 
        claim.claimId?.toString().includes(claimSearch) ||
        claim.policyNumber?.toLowerCase().includes(claimSearch.toLowerCase()) ||
        claim.createdByUserName?.toLowerCase().includes(claimSearch.toLowerCase()) ||
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

  const menuItems = [
    { text: 'Dashboard', icon: <Dashboard />, active: true },
    { text: 'Müşteriler', icon: <People />, active: false },
    { text: 'Poliçeler', icon: <Assignment />, active: false },
    { text: 'Ödemeler', icon: <Payment />, active: false },
    { text: 'Raporlar', icon: <Assessment />, active: false },
    { text: 'Ayarlar', icon: <Settings />, active: false },
  ];

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <CircularProgress size={60} />
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ minHeight: '100vh', backgroundColor: theme.palette.background.default }}>
        <Header />
        <Container maxWidth="xl" sx={{ mt: 3, mb: 3 }}>
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
          <Button variant="contained" onClick={fetchAdminData}>
            Tekrar Dene
          </Button>
        </Container>
      </Box>
    );
  }

  return (
    <Box sx={{ minHeight: '100vh', backgroundColor: theme.palette.background.default }}>
      <Header />
      
      {/* Mobile Drawer */}
      <Drawer
        anchor="left"
        open={drawerOpen}
        onClose={toggleDrawer}
        sx={{
          width: 240,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: 240,
            boxSizing: 'border-box',
          },
        }}
      >
        <Box sx={{ overflow: 'auto' }}>
        <List>
            {menuItems.map((item, index) => (
              <ListItem key={index} button>
                <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItem>
          ))}
        </List>
        </Box>
      </Drawer>

      {/* Main Content */}
      <Container maxWidth={false} sx={{ mt: 3, mb: 3, px: 2, width: '95%' }}>
        {/* Header Section */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box>
            <Typography variant="h4" component="h1" fontWeight={700} gutterBottom>
              Admin Dashboard
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Sistem yönetimi ve genel bakış
            </Typography>
          </Box>
          
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <Button
              startIcon={<span>👑</span>}
              onClick={() => setOpenAdminModal(true)}
              variant="contained"
              color="primary"
              sx={{ borderRadius: 2 }}
            >
              Yeni Admin Ekle
            </Button>
            <IconButton color="inherit">
              <Badge badgeContent={5} color="error">
                <Notifications />
              </Badge>
            </IconButton>
            <Avatar sx={{ bgcolor: theme.palette.primary.main }}>
              <AccountCircle />
            </Avatar>
            <Button
              startIcon={<Logout />}
              onClick={handleLogout}
              variant="outlined"
              color="primary"
            >
              Çıkış
            </Button>
          </Box>
        </Box>

        {/* Statistics Cards */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          {stats.map((stat, index) => (
            <Grid item xs={12} sm={6} md={4} key={index}>
              <Card
                sx={{
                  height: '100%',
                  background: `linear-gradient(135deg, ${stat.color}15 0%, ${stat.color}25 100%)`,
                  border: `1px solid ${stat.color}30`,
                }}
              >
                <CardContent>
                  <Typography variant="h6" color="text.secondary" gutterBottom>
                    {stat.title}
                  </Typography>
                  <Typography variant="h4" component="div" fontWeight={700} sx={{ mb: 1 }}>
                    {stat.value}
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center' }}>
                    {stat.trend === 'up' ? (
                      <TrendingUp sx={{ color: '#4caf50', mr: 1, fontSize: 20 }} />
                    ) : (
                      <TrendingDown sx={{ color: '#f44336', mr: 1, fontSize: 20 }} />
                    )}
                    <Typography
                      variant="body2"
                      sx={{
                        color: stat.trend === 'up' ? '#4caf50' : '#f44336',
                        fontWeight: 600,
                      }}
                    >
                      {stat.change}
                    </Typography>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        {/* Tabs for different panels */}
        <Paper sx={{ mb: 3, width: '100%', maxWidth: 'none' }}>
          <Tabs value={activeTab} onChange={handleTabChange} sx={{ px: 2 }}>
            <Tab label="Tüm Teklifler" />
            <Tab label="Tüm Acentalar" />
            <Tab label="Tüm Müşteriler" />
            <Tab label="Olay Bildirimleri" />
          </Tabs>
        </Paper>

        {/* Tab Panels */}
        {activeTab === 0 && (
            <Paper sx={{ p: 3, width: '100%', maxWidth: 'none' }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6" fontWeight={600}>
                  Tüm Teklifler ({getFilteredAndSortedOffers().length})
                </Typography>
                <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                  {/* Arama */}
                  <TextField
                    size="small"
                    placeholder="Teklif ara..."
                    value={offerSearch}
                    onChange={(e) => setOfferSearch(e.target.value)}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <Search />
                        </InputAdornment>
                      ),
                    }}
                    sx={{ width: 250 }}
                  />
                <Button
                  startIcon={<Add />}
                  variant="contained"
                  size="small"
                  sx={{ borderRadius: 2 }}
                  onClick={() => setOpenOfferModal(true)}
                >
                Yeni Teklif
                </Button>
                </Box>
              </Box>
              
            {offers.length === 0 ? (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <Typography variant="body1" color="text.secondary">
                  Henüz teklif bulunmuyor.
                </Typography>
              </Box>
            ) : (
            <TableContainer sx={{ width: '100%', overflowX: 'auto', maxWidth: 'none' }}>
              <Table sx={{ minWidth: 1400, width: '100%' }}>
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ minWidth: 70, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'offerId'}
                          direction={offerSortBy === 'offerId' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('offerId')}
                        >
                          Teklif ID
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'description'}
                          direction={offerSortBy === 'description' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('description')}
                        >
                          Açıklama
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'customer'}
                          direction={offerSortBy === 'customer' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('customer')}
                        >
                          Müşteri
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 150, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'customerEmail'}
                          direction={offerSortBy === 'customerEmail' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('customerEmail')}
                        >
                          Müşteri E-posta
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 110, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'insuranceType'}
                          direction={offerSortBy === 'insuranceType' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('insuranceType')}
                        >
                          Sigorta Türü
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 90, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'basePrice'}
                          direction={offerSortBy === 'basePrice' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('basePrice')}
                        >
                          Temel Fiyat
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 90, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'discountRate'}
                          direction={offerSortBy === 'discountRate' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('discountRate')}
                        >
                          İndirim Oranı
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 90, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'price'}
                          direction={offerSortBy === 'price' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('price')}
                        >
                          Final Fiyat
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 110, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'coverageAmount'}
                          direction={offerSortBy === 'coverageAmount' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('coverageAmount')}
                        >
                          Kapsam Seviyesi
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 110, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'requestedStartDate'}
                          direction={offerSortBy === 'requestedStartDate' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('requestedStartDate')}
                        >
                          Başlangıç Tarihi
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 80, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'status'}
                          direction={offerSortBy === 'status' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('status')}
                        >
                          Durum
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 110, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'isCustomerApproved'}
                          direction={offerSortBy === 'isCustomerApproved' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('isCustomerApproved')}
                        >
                          Müşteri Onayı
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 90, padding: '8px 6px' }}>
                        <TableSortLabel
                          active={offerSortBy === 'date'}
                          direction={offerSortBy === 'date' ? offerSortOrder : 'asc'}
                          onClick={() => handleOfferSort('date')}
                        >
                          Oluşturulma
                        </TableSortLabel>
                      </TableCell>
                      <TableCell sx={{ minWidth: 120, padding: '8px 6px' }}>Poliçe PDF</TableCell>
                      <TableCell sx={{ minWidth: 100, padding: '8px 6px' }}>İşlemler</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {getFilteredAndSortedOffers().slice(0, 10).map((offer) => (
                      <TableRow key={offer.offerId} hover>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" fontWeight={600} sx={{ fontSize: '0.8rem' }}>
                            #{offer.offerId}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" sx={{ fontSize: '0.8rem' }}>
                            {offer.description || 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            <Avatar sx={{ width: 28, height: 28, mr: 1, bgcolor: theme.palette.primary.main }}>
                               {offer.customerName?.charAt(0) || '?'}
                            </Avatar>
                            <Typography variant="body2" fontWeight={500} sx={{ fontSize: '0.8rem' }}>
                               {offer.customerName || 'N/A'}
                            </Typography>
                          </Box>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" sx={{ fontSize: '0.8rem' }}>
                            {offer.customerEmail || 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" sx={{ fontSize: '0.8rem' }}>
                            {offer.insuranceTypeName || 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
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
                            <Typography variant="body2" fontWeight={600} sx={{ fontSize: '0.8rem' }}>
                              ₺{offer.basePrice || '0'}
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
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
                            <Typography variant="body2" sx={{ fontSize: '0.8rem' }}>
                              %{offer.discountRate || '0'}
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="body2" fontWeight={600} sx={{ fontSize: '0.8rem' }}>
                            {editingOffer === offer.offerId ? (
                              (() => {
                                const basePrice = parseFloat(inlineEditData.basePrice) || 0;
                                const discountRate = parseFloat(inlineEditData.discountRate) || 0;
                                const coverageAmount = inlineEditData.coverageAmount || 0;
                                const calculatedFinalPrice = calculateFinalPrice(basePrice, coverageAmount, discountRate);
                                return `₺${calculatedFinalPrice.toLocaleString()}`;
                              })()
                            ) : (
                              `₺${offer.finalPrice || '0'}`
                            )}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          {editingOffer === offer.offerId ? (
                            <FormControl size="small" sx={{ minWidth: 120 }}>
                              <Select
                                value={inlineEditData.coverageAmount}
                                onChange={(e) => setInlineEditData(prev => ({ ...prev, coverageAmount: Number(e.target.value) }))}
                              >
                                <MenuItem key="0" value={0}>Temel (+0%)</MenuItem>
                                <MenuItem key="25" value={25}>Orta (+25%)</MenuItem>
                                <MenuItem key="40" value={40}>Premium (+40%)</MenuItem>
                              </Select>
                            </FormControl>
                          ) : (
                            <Typography variant="body2" fontWeight={600} sx={{ fontSize: '0.8rem' }}>
                              {offer.coverageAmount === 0 ? 'Temel (+0%)' :
                               offer.coverageAmount === 25 ? 'Orta (+25%)' :
                               offer.coverageAmount === 40 ? 'Premium (+40%)' :
                               'Bilinmeyen'}
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="caption" sx={{ fontSize: '0.75rem' }}>
                            {offer.requestedStartDate ? new Date(offer.requestedStartDate).toLocaleDateString('tr-TR') : 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
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
                              label={getStatusText(offer.status || 'pending')}
                              size="small"
                              sx={{ 
                                backgroundColor: getStatusColor(offer.status || 'pending'),
                                color: 'white',
                                fontWeight: 500,
                                fontSize: '0.7rem',
                                height: '24px'
                              }}
                            />
                          )}
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Chip
                            label={offer.isCustomerApproved ? 'Onaylandı' : 'Beklemede'}
                            size="small"
                            color={offer.isCustomerApproved ? 'success' : 'warning'}
                            sx={{ fontSize: '0.7rem', height: '24px' }}
                          />
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Typography variant="caption" sx={{ fontSize: '0.75rem' }}>
                            {offer.createdAt ? new Date(offer.createdAt).toLocaleDateString('tr-TR') : 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          {editingOffer === offer.offerId ? (
                            <input
                              type="file"
                              accept=".pdf"
                              onChange={(e) => {
                                const file = e.target.files?.[0];
                                if (file) {
                                  setInlineEditData(prev => ({ ...prev, policyPdfFile: file }));
                                }
                              }}
                              style={{ fontSize: '0.7rem' }}
                            />
                          ) : (
                            <Typography variant="caption" sx={{ fontSize: '0.7rem', color: 'text.secondary' }}>
                              {(offer as any).policyPdfUrl ? 'PDF Yüklü' : 'PDF Yok'}
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell sx={{ padding: '8px 6px' }}>
                          <Box sx={{ display: 'flex', gap: 0.5 }}>
                            {editingOffer === offer.offerId ? (
                              <>
                                <IconButton 
                                  size="small" 
                                  color="success"
                                  title="Kaydet"
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
                                <IconButton 
                                  size="small" 
                                  color="error"
                                  title="İptal"
                                  onClick={handleCancelEdit}
                                  sx={{ padding: '4px' }}
                                >
                                  <Cancel sx={{ fontSize: '1rem' }} />
                                </IconButton>
                              </>
                            ) : (
                              <>
                                {/* Düzenle butonu sadece müşteri onaylamadıysa gösterilir */}
                                {!offer.isCustomerApproved && (
                                <IconButton 
                                  size="small" 
                                  color="primary" 
                                  title="Düzenle"
                                  onClick={() => handleEditOffer(offer)}
                                  sx={{ padding: '4px' }}
                                >
                                  <Edit sx={{ fontSize: '1rem' }} />
                                </IconButton>
                                )}
                                <IconButton 
                                  size="small" 
                                  color="info" 
                                  title="Detaylar"
                                  onClick={() => handleViewOffer(offer)}
                                  sx={{ padding: '4px' }}
                                >
                                  <Visibility sx={{ fontSize: '1rem' }} />
                                </IconButton>
                                <IconButton 
                                  size="small" 
                                  color="error" 
                                  title="Sil"
                                  onClick={() => handleDeleteOffer(offer)}
                                  sx={{ padding: '4px' }}
                                >
                                  <Delete sx={{ fontSize: '1rem' }} />
                                </IconButton>
                              </>
                            )}
                          </Box>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
            </Paper>
        )}

        {activeTab === 1 && (
          <Paper sx={{ p: 3, width: '100%', maxWidth: 'none' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
              <Typography variant="h6" fontWeight={600}>
                Tüm Acentalar ({getFilteredAndSortedAgents().length})
              </Typography>
              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                {/* Arama */}
                <TextField
                  size="small"
                  placeholder="Acenta ara..."
                  value={agentSearch}
                  onChange={(e) => setAgentSearch(e.target.value)}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Search />
                      </InputAdornment>
                    ),
                  }}
                  sx={{ width: 250 }}
                />
                <Button
                  startIcon={<Add />}
                variant="contained"
                size="small"
                sx={{ borderRadius: 2 }}
                onClick={() => setOpenAgentModal(true)}
                >
                Yeni Acenta
                </Button>
              </Box>
            </Box>
            
            {agents.length === 0 ? (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <Typography variant="body1" color="text.secondary">
                  Henüz acenta bulunmuyor.
                </Typography>
              </Box>
            ) : (
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>
                        <TableSortLabel
                          active={agentSortBy === 'name'}
                          direction={agentSortBy === 'name' ? agentSortOrder : 'asc'}
                          onClick={() => handleAgentSort('name')}
                        >
                          Acenta
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={agentSortBy === 'code'}
                          direction={agentSortBy === 'code' ? agentSortOrder : 'asc'}
                          onClick={() => handleAgentSort('code')}
                        >
                          Kod
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={agentSortBy === 'department'}
                          direction={agentSortBy === 'department' ? agentSortOrder : 'asc'}
                          onClick={() => handleAgentSort('department')}
                        >
                          Departman
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={agentSortBy === 'email'}
                          direction={agentSortBy === 'email' ? agentSortOrder : 'asc'}
                          onClick={() => handleAgentSort('email')}
                        >
                          E-posta
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={agentSortBy === 'phone'}
                          direction={agentSortBy === 'phone' ? agentSortOrder : 'asc'}
                          onClick={() => handleAgentSort('phone')}
                        >
                          Telefon
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={agentSortBy === 'address'}
                          direction={agentSortBy === 'address' ? agentSortOrder : 'asc'}
                          onClick={() => handleAgentSort('address')}
                        >
                          Adres
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>İşlemler</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {getFilteredAndSortedAgents().map((agent) => (
                      <TableRow key={agent.id} hover>
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            <Avatar sx={{ width: 32, height: 32, mr: 2, bgcolor: theme.palette.secondary.main }}>
                              {agent.user?.name?.charAt(0) || '?'}
                            </Avatar>
                            <Typography variant="body2" fontWeight={500}>
                              {agent.user?.name || 'N/A'}
                            </Typography>
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={agent.agentCode}
                            size="small"
                            color="primary"
                          />
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={agent.department}
                            size="small"
                            color="info"
                          />
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" color="text.secondary">
                              {agent.user?.email || 'N/A'}
                            </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" color="text.secondary">
                            {agent.phone || '-'}
                            </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" color="text.secondary">
                            {agent.address || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', gap: 1 }}>
                            <IconButton 
                              size="small" 
                              color="primary"
                              onClick={() => handleViewAgent(agent)}
                              title="Detayları Görüntüle"
                            >
                              <Visibility />
                            </IconButton>
                            <IconButton 
                              size="small" 
                              color="secondary"
                              onClick={() => handleEditAgent(agent)}
                              title="Düzenle"
                            >
                              <Edit />
                            </IconButton>
                            <IconButton 
                              size="small" 
                              color="error"
                              onClick={() => handleDeleteAgentClick(agent)}
                              title="Sil"
                            >
                              <Delete />
                            </IconButton>
                          </Box>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
            </Paper>
        )}

        {activeTab === 2 && (
          <Paper sx={{ p: 3, width: '100%', maxWidth: 'none' }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6" fontWeight={600}>
                  Tüm Müşteriler ({getFilteredAndSortedCustomers().length})
              </Typography>
                <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                  {/* Arama Kutusu */}
                  <TextField
                    size="small"
                    placeholder="Müşteri ara..."
                    value={customerSearch}
                    onChange={(e) => setCustomerSearch(e.target.value)}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <Search />
                        </InputAdornment>
                      ),
                    }}
                    sx={{ width: 250 }}
                  />
                <Button
                    startIcon={<Add />}
                  variant="contained"
                    size="small"
                    sx={{ borderRadius: 2 }}
                    onClick={() => setOpenCustomerModal(true)}
                >
                    Yeni Müşteri
                </Button>
                </Box>
              </Box>
              
              {customers.length === 0 ? (
                <Box sx={{ textAlign: 'center', py: 8 }}>
                  <AccountCircle sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
                  <Typography variant="h6" color="text.secondary" gutterBottom>
                    Henüz müşteri bulunmuyor
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    İlk müşterinizi ekleyerek başlayın
                  </Typography>
                </Box>
              ) : (
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>
                          <TableSortLabel
                            active={customerSortBy === 'name'}
                            direction={customerSortBy === 'name' ? customerSortOrder : 'asc'}
                            onClick={() => handleCustomerSort('name')}
                          >
                            Müşteri
                          </TableSortLabel>
                        </TableCell>
                        <TableCell>
                          <TableSortLabel
                            active={customerSortBy === 'idNo'}
                            direction={customerSortBy === 'idNo' ? customerSortOrder : 'asc'}
                            onClick={() => handleCustomerSort('idNo')}
                          >
                            TC/Vergi No
                          </TableSortLabel>
                        </TableCell>
                        <TableCell>
                          <TableSortLabel
                            active={customerSortBy === 'email'}
                            direction={customerSortBy === 'email' ? customerSortOrder : 'asc'}
                            onClick={() => handleCustomerSort('email')}
                          >
                            E-posta
                          </TableSortLabel>
                        </TableCell>
                        <TableCell>
                          <TableSortLabel
                            active={customerSortBy === 'phone'}
                            direction={customerSortBy === 'phone' ? customerSortOrder : 'asc'}
                            onClick={() => handleCustomerSort('phone')}
                          >
                            Telefon
                          </TableSortLabel>
                        </TableCell>
                        <TableCell>
                          <TableSortLabel
                            active={customerSortBy === 'address'}
                            direction={customerSortBy === 'address' ? customerSortOrder : 'asc'}
                            onClick={() => handleCustomerSort('address')}
                          >
                            Adres
                          </TableSortLabel>
                        </TableCell>
                        <TableCell>İşlemler</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {getFilteredAndSortedCustomers().map((customer) => (
                        <TableRow key={customer.id} hover>
                          <TableCell>
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            <Avatar 
                              sx={{ 
                                  width: 32, 
                                  height: 32, 
                                mr: 2, 
                                  bgcolor: theme.palette.primary.main
                              }}
                            >
                              {customer.user?.name?.charAt(0) || '?'}
                            </Avatar>
                              <Typography variant="body2" fontWeight={500}>
                                {customer.user?.name || 'N/A'}
                              </Typography>
                            </Box>
                          </TableCell>
                          <TableCell>
                            <Typography variant="body2" color="text.secondary">
                              {customer.idNo || '-'}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Typography variant="body2" color="text.secondary">
                                {customer.user?.email || 'N/A'}
                              </Typography>
                          </TableCell>
                          <TableCell>
                            <Typography variant="body2" color="text.secondary">
                              {customer.phone || '-'}
                              </Typography>
                          </TableCell>
                          <TableCell>
                            <Typography variant="body2" color="text.secondary">
                              {customer.address || '-'}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Box sx={{ display: 'flex', gap: 1 }}>
                              <IconButton 
                                size="small" 
                                color="primary"
                                onClick={() => handleViewCustomer(customer)}
                                title="Detayları Görüntüle"
                              >
                                <Visibility />
                              </IconButton>
                              <IconButton 
                                size="small" 
                                color="secondary"
                                onClick={() => handleEditCustomer(customer)}
                                title="Düzenle"
                              >
                                <Edit />
                              </IconButton>
                              <IconButton 
                                size="small" 
                                color="error"
                                onClick={() => handleDeleteCustomerClick(customer)}
                                title="Sil"
                              >
                                <Delete />
                              </IconButton>
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </Paper>
        )}

        {/* Claims Tab */}
        {activeTab === 3 && (
          <Paper sx={{ p: 3, width: '100%', maxWidth: 'none' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
              <Typography variant="h6" fontWeight={600}>
                Olay Bildirimleri ({getFilteredAndSortedClaims().length})
                              </Typography>
              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
                {/* Arama */}
                <TextField
                  size="small"
                  placeholder="Bildirim ara..."
                  value={claimSearch}
                  onChange={(e) => setClaimSearch(e.target.value)}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Search />
                      </InputAdornment>
                    ),
                  }}
                  sx={{ width: 250 }}
                />
                {/* Durum Filtresi */}
                <FormControl size="small" sx={{ minWidth: 120 }}>
                  <InputLabel>Durum</InputLabel>
                  <Select
                    value={claimFilterStatus}
                    label="Durum"
                    onChange={(e) => setClaimFilterStatus(e.target.value as 'all' | 'Pending' | 'Approved' | 'Rejected')}
                  >
                    <MenuItem key="all" value="all">Tümü</MenuItem>
                    <MenuItem key="Pending" value="Pending">Beklemede</MenuItem>
                    <MenuItem key="Approved" value="Approved">Onaylandı</MenuItem>
                    <MenuItem key="Rejected" value="Rejected">Reddedildi</MenuItem>
                  </Select>
                </FormControl>
                            </Box>
                          </Box>

            {claims.length === 0 ? (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <Typography variant="body1" color="text.secondary">
                  Henüz olay bildirimi bulunmuyor.
                            </Typography>
              </Box>
            ) : (
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'claimId'}
                          direction={claimSortBy === 'claimId' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('claimId')}
                        >
                          Claim ID
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'policyNumber'}
                          direction={claimSortBy === 'policyNumber' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('policyNumber')}
                        >
                          Poliçe No
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'customer'}
                          direction={claimSortBy === 'customer' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('customer')}
                        >
                          Müşteri
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'customerEmail'}
                          direction={claimSortBy === 'customerEmail' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('customerEmail')}
                        >
                          Müşteri E-posta
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'type'}
                          direction={claimSortBy === 'type' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('type')}
                        >
                          Olay Türü
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'description'}
                          direction={claimSortBy === 'description' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('description')}
                        >
                          Açıklama
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'incidentDate'}
                          direction={claimSortBy === 'incidentDate' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('incidentDate')}
                        >
                          Olay Tarihi
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'status'}
                          direction={claimSortBy === 'status' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('status')}
                        >
                          Durum
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'amount'}
                          direction={claimSortBy === 'amount' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('amount')}
                        >
                          Onaylanan Tutar
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'notes'}
                          direction={claimSortBy === 'notes' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('notes')}
                        >
                          Yetkili Notu
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'processedBy'}
                          direction={claimSortBy === 'processedBy' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('processedBy')}
                        >
                          İşleyen Yetkili
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'processedByEmail'}
                          direction={claimSortBy === 'processedByEmail' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('processedByEmail')}
                        >
                          Yetkili E-posta
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'processedByPhone'}
                          direction={claimSortBy === 'processedByPhone' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('processedByPhone')}
                        >
                          Yetkili Telefon
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>
                        <TableSortLabel
                          active={claimSortBy === 'date'}
                          direction={claimSortBy === 'date' ? claimSortOrder : 'asc'}
                          onClick={() => handleClaimSort('date')}
                        >
                          Oluşturma Tarihi
                        </TableSortLabel>
                      </TableCell>
                      <TableCell>İşlemler</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {getFilteredAndSortedClaims().map((claim) => (
                      <TableRow key={claim.claimId} hover>
                        <TableCell>
                          <Typography variant="body2" fontWeight={600}>
                            #{claim.claimId}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {claim.policyNumber || 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" fontWeight={500}>
                            {claim.createdByUserName || 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {claim.createdByUserEmail || 'N/A'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={claim.type}
                            size="small"
                            color="primary"
                            variant="outlined"
                          />
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" sx={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                            {claim.description}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption">
                            {claim.incidentDate ? new Date(claim.incidentDate).toLocaleDateString('tr-TR') : '-'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          {editingClaim === claim.claimId ? (
                            <FormControl size="small" sx={{ minWidth: 120 }}>
                              <Select
                                value={inlineClaimEditData.status}
                                onChange={(e) => setInlineClaimEditData(prev => ({ ...prev, status: e.target.value }))}
                              >
                                <MenuItem key="Pending" value="Pending">Beklemede</MenuItem>
                                <MenuItem key="Approved" value="Approved">Onaylandı</MenuItem>
                                <MenuItem key="Rejected" value="Rejected">Reddedildi</MenuItem>
                              </Select>
                            </FormControl>
                          ) : (
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
                            />
                          )}
                        </TableCell>
                        <TableCell>
                          {editingClaim === claim.claimId ? (
                            <TextField
                              size="small"
                              type="number"
                              value={inlineClaimEditData.approvedAmount}
                              onChange={(e) => setInlineClaimEditData(prev => ({ ...prev, approvedAmount: e.target.value }))}
                              sx={{ width: '100px' }}
                              inputProps={{ min: 0, step: 0.01 }}
                              placeholder="Tutar"
                            />
                          ) : (
                            <Typography variant="body2" fontWeight={600}>
                              {claim.approvedAmount ? `₺${claim.approvedAmount.toLocaleString()}` : '-'}
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell>
                          {editingClaim === claim.claimId ? (
                            <TextField
                              size="small"
                              multiline
                              rows={2}
                              value={inlineClaimEditData.notes}
                              onChange={(e) => setInlineClaimEditData(prev => ({ ...prev, notes: e.target.value }))}
                              sx={{ width: '200px' }}
                              placeholder="Yetkili notu..."
                            />
                          ) : (
                            <Typography variant="body2" sx={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                              {claim.notes || '-'}
                            </Typography>
                          )}
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" fontWeight={500}>
                            {claim.processedByUserName || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {claim.processedByUserEmail || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {claim.processedByUserPhone || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption">
                            {new Date(claim.createdAt).toLocaleDateString('tr-TR')}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Box sx={{ display: 'flex', gap: 1 }}>
                            {editingClaim === claim.claimId ? (
                              <>
                                <IconButton 
                                  size="small" 
                                  color="success"
                                  title="Kaydet"
                                  onClick={() => handleSaveClaim(claim.claimId)}
                                  disabled={savingClaim === claim.claimId}
                                >
                                  {savingClaim === claim.claimId ? (
                                    <CircularProgress size={16} />
                                  ) : (
                                    <CheckCircle />
                                  )}
                              </IconButton>
                                <IconButton 
                                  size="small" 
                                  color="error"
                                  title="İptal"
                                  onClick={handleCancelClaimEdit}
                                >
                                  <Cancel />
                              </IconButton>
                              </>
                            ) : (
                              <>
                                <IconButton 
                                  size="small" 
                                  color="primary" 
                                  title="Detaylar"
                                  onClick={() => {
                                    // TODO: View claim details modal
                                    console.log('View claim:', claim);
                                  }}
                                >
                                  <Visibility />
                              </IconButton>
                                <IconButton 
                                  size="small" 
                                  color="secondary" 
                                  title="Düzenle"
                                  onClick={() => handleEditClaim(claim)}
                                >
                                  <Edit />
                                </IconButton>
                              </>
                            )}
                            </Box>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
              )}
            </Paper>
        )}
      </Container>

      {/* Agent Registration Modal */}
      <Dialog 
        open={openAgentModal} 
        onClose={handleCloseAgentModal}
        maxWidth="sm"
                  fullWidth
      >
        <DialogTitle sx={{ pb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <Business sx={{ mr: 1, color: 'primary.main' }} />
            <Typography variant="h6">Yeni Acenta Ekle</Typography>
          </Box>
        </DialogTitle>
        
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            {/* Ad */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Ad Soyad"
                value={agentFormData.name}
                onChange={(e) => handleAgentInputChange('name', e.target.value)}
                error={!!agentFormErrors.name}
                helperText={agentFormErrors.name}
                size="small"
              />
            </Grid>
            
            {/* Email */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Email"
                type="email"
                value={agentFormData.email}
                onChange={(e) => handleAgentInputChange('email', e.target.value)}
                error={!!agentFormErrors.email}
                helperText={agentFormErrors.email}
                size="small"
              />
            </Grid>
            
            {/* Şifre */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Şifre"
                type="password"
                value={agentFormData.password}
                onChange={(e) => handleAgentInputChange('password', e.target.value)}
                error={!!agentFormErrors.password}
                helperText={agentFormErrors.password}
                size="small"
              />
            </Grid>
            
            {/* Şifre Tekrarı */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Şifre Tekrarı"
                type="password"
                value={agentFormData.confirmPassword}
                onChange={(e) => handleAgentInputChange('confirmPassword', e.target.value)}
                error={!!agentFormErrors.confirmPassword}
                helperText={agentFormErrors.confirmPassword}
                size="small"
              />
            </Grid>
            
            {/* Departman - Acenta Kodu otomatik oluşturulacak */}
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small" error={!!agentFormErrors.department}>
                <InputLabel id="department-label">Departman</InputLabel>
                <Select
                  labelId="department-label"
                  value={agentFormData.department}
                  onChange={(e) => handleAgentInputChange('department', e.target.value)}
                  label="Departman"
                >
                  <MenuItem key="Trafik" value="Trafik Sigortası">Trafik Sigortası</MenuItem>
                  <MenuItem key="Sağlık" value="Sağlık Sigortası">Sağlık Sigortası</MenuItem>
                  <MenuItem key="Konut" value="Konut Sigortası">Konut Sigortası</MenuItem>
                  <MenuItem key="İşyeri" value="İşyeri Sigortası">İşyeri Sigortası</MenuItem>
                  <MenuItem key="Hayat" value="Hayat Sigortası">Hayat Sigortası</MenuItem>
                  <MenuItem key="Seyahat" value="Seyahat Sigortası">Seyahat Sigortası</MenuItem>
                </Select>
                {agentFormErrors.department && (
                  <FormHelperText>{agentFormErrors.department}</FormHelperText>
                )}
              </FormControl>
            </Grid>
            
            {/* Telefon */}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Telefon"
                value={agentFormData.phone}
                onChange={(e) => handleAgentInputChange('phone', e.target.value)}
                error={!!agentFormErrors.phone}
                helperText={agentFormErrors.phone}
                size="small"
              />
            </Grid>
            
            {/* Adres */}
            <Grid item xs={12}>
              <TextField
                  fullWidth
                label="Adres"
                multiline
                rows={3}
                value={agentFormData.address}
                onChange={(e) => handleAgentInputChange('address', e.target.value)}
                error={!!agentFormErrors.address}
                helperText={agentFormErrors.address}
                size="small"
              />
          </Grid>
        </Grid>
        </DialogContent>
        
        <DialogActions sx={{ px: 3, pb: 3 }}>
                <Button
            onClick={handleCloseAgentModal}
            disabled={isSubmittingAgent}
          >
            İptal
          </Button>
          <Button
            onClick={handleSubmitAgent}
            variant="contained"
            disabled={isSubmittingAgent}
            startIcon={isSubmittingAgent ? <CircularProgress size={16} /> : <Add />}
          >
            {isSubmittingAgent ? 'Ekleniyor...' : 'Acenta Ekle'}
          </Button>
        </DialogActions>
              </Dialog>

        {/* Customer Registration Modal */}
        <Dialog open={openCustomerModal} onClose={handleCloseCustomerModal} maxWidth="sm" fullWidth>
          <DialogTitle sx={{ pb: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <AccountCircle sx={{ color: 'primary.main' }} />
              <Typography variant="h6">Yeni Müşteri Ekle</Typography>
            </Box>
          </DialogTitle>
          
          <DialogContent>
            <Grid container spacing={2} sx={{ mt: 1 }}>
              {/* Ad Soyad */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Ad Soyad"
                  value={customerFormData.name}
                  onChange={(e) => handleCustomerInputChange('name', e.target.value)}
                  error={!!customerFormErrors.name}
                  helperText={customerFormErrors.name}
                  size="small"
                />
              </Grid>
              
              {/* Email */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Email"
                  type="email"
                  value={customerFormData.email}
                  onChange={(e) => handleCustomerInputChange('email', e.target.value)}
                  error={!!customerFormErrors.email}
                  helperText={customerFormErrors.email}
                  size="small"
                />
              </Grid>
              
              {/* Şifre */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Şifre"
                  type="password"
                  value={customerFormData.password}
                  onChange={(e) => handleCustomerInputChange('password', e.target.value)}
                  error={!!customerFormErrors.password}
                  helperText={customerFormErrors.password}
                  size="small"
                />
              </Grid>
              
              {/* Şifre Tekrarı */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Şifre Tekrarı"
                  type="password"
                  value={customerFormData.confirmPassword}
                  onChange={(e) => handleCustomerInputChange('confirmPassword', e.target.value)}
                  error={!!customerFormErrors.confirmPassword}
                  helperText={customerFormErrors.confirmPassword}
                  size="small"
                />
              </Grid>
              
              {/* TC Kimlik No */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="TC Kimlik No"
                  value={customerFormData.tcNo}
                  onChange={(e) => handleCustomerInputChange('tcNo', e.target.value)}
                  error={!!customerFormErrors.tcNo}
                  helperText={customerFormErrors.tcNo}
                  size="small"
                  inputProps={{ maxLength: 11 }}
                />
              </Grid>
              
              {/* Telefon */}
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Telefon"
                  value={customerFormData.phone}
                  onChange={(e) => handleCustomerInputChange('phone', e.target.value)}
                  error={!!customerFormErrors.phone}
                  helperText={customerFormErrors.phone}
                  size="small"
                />
              </Grid>
              
              {/* Adres */}
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Adres"
                  multiline
                  rows={3}
                  value={customerFormData.address}
                  onChange={(e) => handleCustomerInputChange('address', e.target.value)}
                  error={!!customerFormErrors.address}
                  helperText={customerFormErrors.address}
                  size="small"
                />
              </Grid>
            </Grid>
          </DialogContent>
          
          <DialogActions sx={{ px: 3, pb: 3 }}>
            <Button onClick={handleCloseCustomerModal} disabled={isSubmittingCustomer}>
              İptal
                </Button>
            <Button
              onClick={handleSubmitCustomer}
              variant="contained"
              disabled={isSubmittingCustomer}
              startIcon={isSubmittingCustomer ? <CircularProgress size={16} /> : <Add />}
            >
              {isSubmittingCustomer ? 'Ekleniyor...' : 'Müşteri Ekle'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Offer Creation Modal */}
        <Dialog open={openOfferModal} onClose={handleCloseOfferModal} maxWidth="md" fullWidth>
          <DialogTitle sx={{ pb: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Description sx={{ color: 'primary.main' }} />
              <Typography variant="h6">Yeni Teklif Oluştur</Typography>
              </Box>
          </DialogTitle>
          
          <DialogContent>
            <Grid container spacing={2} sx={{ mt: 1 }}>
              {/* Müşteri Seçimi */}
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth size="small" error={!!offerFormErrors.customerId}>
                  <InputLabel>Müşteri Seçin</InputLabel>
                  <Select
                    value={offerFormData.customerId}
                    onChange={(e) => handleOfferInputChange('customerId', Number(e.target.value))}
                    label="Müşteri Seçin"
                  >
                    {customers?.filter(c => c.user).map((customer) => (
                      <MenuItem key={customer.id} value={customer.id}>
                        {customer.user?.name || 'N/A'}
                      </MenuItem>
                    )) || []}
                  </Select>
                  {offerFormErrors.customerId && (
                    <FormHelperText>{offerFormErrors.customerId}</FormHelperText>
                  )}
                </FormControl>
          </Grid>
              
              {/* Acenta Seçimi */}
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth size="small" error={!!offerFormErrors.agentId}>
                  <InputLabel>Acenta Seçin</InputLabel>
                  <Select
                    value={offerFormData.agentId}
                    onChange={(e) => handleOfferInputChange('agentId', Number(e.target.value))}
                    label="Acenta Seçin"
                  >
                    {agents?.filter(a => a.user).map((agent) => (
                      <MenuItem key={agent.id} value={agent.id}>
                        {agent.user?.name || 'N/A'} - {agent.department}
                      </MenuItem>
                    )) || []}
                  </Select>
                  {offerFormErrors.agentId && (
                    <FormHelperText>{offerFormErrors.agentId}</FormHelperText>
                  )}
                </FormControl>
        </Grid>
              
              {/* Sigorta Türü Seçimi */}
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth size="small" error={!!offerFormErrors.insuranceTypeId}>
                  <InputLabel>Sigorta Türü Seçin</InputLabel>
                  <Select
                    value={offerFormData.insuranceTypeId}
                    onChange={(e) => handleOfferInputChange('insuranceTypeId', Number(e.target.value))}
                    label="Sigorta Türü Seçin"
                  >
                    <MenuItem key="1" value={1}>Trafik Sigortası</MenuItem>
                    <MenuItem key="2" value={2}>Sağlık Sigortası</MenuItem>
                    <MenuItem key="3" value={3}>Konut Sigortası</MenuItem>
                    <MenuItem key="4" value={4}>İşyeri Sigortası</MenuItem>
                    <MenuItem key="5" value={5}>Hayat Sigortası</MenuItem>
                  </Select>
                  {offerFormErrors.insuranceTypeId && (
                    <FormHelperText>{offerFormErrors.insuranceTypeId}</FormHelperText>
                  )}
                </FormControl>
              </Grid>

              {/* Kapsam Seviyesi Seçimi */}
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth size="small">
                  <InputLabel>Kapsam Seviyesi</InputLabel>
                  <Select
                    value={offerFormData.coverageAmount}
                    onChange={(e) => handleOfferInputChange('coverageAmount', Number(e.target.value))}
                    label="Kapsam Seviyesi"
                  >
                    <MenuItem key="0" value={0}>Temel Kapsam (+0%)</MenuItem>
                    <MenuItem key="25" value={25}>Orta Kapsam (+25%)</MenuItem>
                    <MenuItem key="40" value={40}>Premium Kapsam (+40%)</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              
              
              {/* Geçerlilik Tarihi */}
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Geçerlilik Tarihi"
                  type="date"
                  value={offerFormData.validUntil}
                  onChange={(e) => handleOfferInputChange('validUntil', e.target.value)}
                  error={!!offerFormErrors.validUntil}
                  helperText={offerFormErrors.validUntil}
                  size="small"
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              
              {/* Açıklama */}
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Açıklama"
                  multiline
                  rows={3}
                  value={offerFormData.description}
                  onChange={(e) => handleOfferInputChange('description', e.target.value)}
                  error={!!offerFormErrors.description}
                  helperText={offerFormErrors.description}
                  size="small"
                  placeholder="Teklif detayları ve özel koşullar..."
                />
              </Grid>
            </Grid>
          </DialogContent>
          
          <DialogActions sx={{ px: 3, pb: 3 }}>
            <Button onClick={handleCloseOfferModal} disabled={isSubmittingOffer}>
              İptal
            </Button>
            <Button
              onClick={handleSubmitOffer}
              variant="contained"
              disabled={isSubmittingOffer}
              startIcon={isSubmittingOffer ? <CircularProgress size={16} /> : <Add />}
            >
              {isSubmittingOffer ? 'Oluşturuluyor...' : 'Teklif Oluştur'}
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
                    {selectedViewOffer.insuranceTypeName} - {selectedViewOffer.customerName}
                  </Typography>
                </Box>

                {/* Basic Details Grid */}
                <Grid container spacing={2} sx={{ mb: 3 }}>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="primary" gutterBottom>
                      Müşteri Bilgileri
                    </Typography>
                    <Typography variant="body2"><strong>Ad:</strong> {selectedViewOffer.customerName || 'N/A'}</Typography>
                    <Typography variant="body2"><strong>Departman:</strong> {selectedViewOffer.department || 'N/A'}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="subtitle2" color="primary" gutterBottom>
                      Teklif Bilgileri
                    </Typography>
                    <Typography variant="body2"><strong>Sigorta Türü:</strong> {selectedViewOffer.insuranceTypeName || 'N/A'}</Typography>
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

                {/* Coverage Amount */}
                <Box sx={{ mb: 3, p: 2, backgroundColor: 'primary.50', borderRadius: 1 }}>
                  <Typography variant="subtitle2" color="primary.dark" gutterBottom>
                    Teminat Bilgileri
                  </Typography>
                  <Typography variant="h6" color="primary">
                    ₺{selectedViewOffer.coverageAmount?.toLocaleString() || '0'}
                  </Typography>
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
                <Typography variant="body2"><strong>Müşteri:</strong> {offerToDelete.customerName || 'N/A'}</Typography>
                <Typography variant="body2"><strong>Sigorta Türü:</strong> {offerToDelete.insuranceTypeName || 'N/A'}</Typography>
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

        {/* Admin Registration Modal */}
        <Dialog
          open={openAdminModal}
          onClose={handleCloseAdminModal}
          maxWidth="sm"
          fullWidth
          PaperProps={{
            sx: {
              borderRadius: 2,
              boxShadow: '0 10px 30px rgba(0,0,0,0.1)',
              maxHeight: '90vh',
              minHeight: '480px', // %20 daha yüksek (400px * 1.2 = 480px)
            }
          }}
        >
          <DialogTitle sx={{
            backgroundColor: theme.palette.primary.main,
            color: 'white',
            display: 'flex',
            alignItems: 'center',
            gap: 1,
            fontWeight: 600,
            py: 2
          }}>
            <span>👑</span>
            Yeni Admin Kullanıcısı Ekle
          </DialogTitle>
          
          <DialogContent sx={{ 
            p: 2.5, 
            pt: 2, 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            minHeight: '300px'
          }}>
            <Box sx={{ width: '100%', maxWidth: '500px' }}>
              <Grid container spacing={1.5}>
                {/* Ad Soyad */}
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Ad Soyad"
                    value={adminFormData.name}
                    onChange={(e) => handleAdminInputChange('name', e.target.value)}
                    error={!!adminFormErrors.name}
                    helperText={adminFormErrors.name}
                    variant="outlined"
                    size="small"
                    sx={{ 
                      '& .MuiOutlinedInput-root': { 
                        height: '40px' 
                      },
                      '& .MuiInputLabel-root': { 
                        fontSize: '0.875rem' 
                      }
                    }}
                  />
                </Grid>
                
                {/* E-posta */}
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="E-posta"
                    type="email"
                    value={adminFormData.email}
                    onChange={(e) => {
                      // E-posta formatını kontrol et ve düzenle
                      const emailValue = e.target.value.toLowerCase().trim();
                      handleAdminInputChange('email', emailValue);
                    }}
                    error={!!adminFormErrors.email}
                    helperText={adminFormErrors.email || "Örnek: admin@sirket.com"}
                    variant="outlined"
                    size="small"
                    placeholder="admin@sirket.com"
                    inputProps={{
                      autoComplete: "email",
                      inputMode: "email"
                    }}
                    sx={{ 
                      '& .MuiOutlinedInput-root': { 
                        height: '40px' 
                      },
                      '& .MuiInputLabel-root': { 
                        fontSize: '0.875rem' 
                      }
                    }}
                  />
                </Grid>
                
                {/* Telefon */}
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Telefon"
                    type="tel"
                    value={adminFormData.phone}
                    onChange={(e) => {
                      // Sadece rakam, + ve boşluk karakterlerine izin ver
                      const phoneValue = e.target.value.replace(/[^0-9+\s]/g, '');
                      handleAdminInputChange('phone', phoneValue);
                    }}
                    error={!!adminFormErrors.phone}
                    helperText={adminFormErrors.phone || "Örnek: 0555 123 45 67"}
                    variant="outlined"
                    size="small"
                    placeholder="0555 123 45 67"
                    inputProps={{
                      pattern: "[0-9+\s]*",
                      inputMode: "tel",
                      maxLength: 15
                    }}
                    sx={{ 
                      '& .MuiOutlinedInput-root': { 
                        height: '40px' 
                      },
                      '& .MuiInputLabel-root': { 
                        fontSize: '0.875rem' 
                      }
                    }}
                  />
                </Grid>
                
                {/* Şifre */}
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Şifre"
                    type="password"
                    value={adminFormData.password}
                    onChange={(e) => handleAdminInputChange('password', e.target.value)}
                    error={!!adminFormErrors.password}
                    helperText={adminFormErrors.password}
                    variant="outlined"
                    size="small"
                    sx={{ 
                      '& .MuiOutlinedInput-root': { 
                        height: '40px' 
                      },
                      '& .MuiInputLabel-root': { 
                        fontSize: '0.875rem' 
                      }
                    }}
                  />
                </Grid>
                
                {/* Şifre Tekrar */}
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Şifre Tekrar"
                    type="password"
                    value={adminFormData.confirmPassword}
                    onChange={(e) => handleAdminInputChange('confirmPassword', e.target.value)}
                    error={!!adminFormErrors.confirmPassword}
                    helperText={adminFormErrors.confirmPassword}
                    variant="outlined"
                    size="small"
                    sx={{ 
                      '& .MuiOutlinedInput-root': { 
                        height: '40px' 
                      },
                      '& .MuiInputLabel-root': { 
                        fontSize: '0.875rem' 
                      }
                    }}
                  />
                </Grid>
              </Grid>
            </Box>
          </DialogContent>
          
          <DialogActions sx={{ p: 2, backgroundColor: '#f5f5f5', gap: 1 }}>
            <Button
              onClick={handleCloseAdminModal}
              variant="outlined"
              disabled={isSubmittingAdmin}
              size="small"
              sx={{ minWidth: '80px' }}
            >
              İptal
            </Button>
            <Button
              onClick={handleSubmitAdmin}
              variant="contained"
              color="primary"
              disabled={isSubmittingAdmin}
              startIcon={isSubmittingAdmin ? <CircularProgress size={16} /> : <span>👑</span>}
              size="small"
              sx={{ minWidth: '120px' }}
            >
              {isSubmittingAdmin ? 'Ekleniyor...' : 'Admin Ekle'}
            </Button>
          </DialogActions>
        </Dialog>

      {/* Agent Detail Modal */}
      <Dialog open={openAgentDetailModal} onClose={() => setOpenAgentDetailModal(false)} maxWidth="md" fullWidth>
        <DialogTitle>Acenta Detayları</DialogTitle>
        <DialogContent>
          {selectedAgent && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Ad Soyad</Typography>
                <Typography variant="body1">{selectedAgent.user?.name || 'N/A'}</Typography>
              </Grid>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Email</Typography>
                <Typography variant="body1">{selectedAgent.user?.email || 'N/A'}</Typography>
              </Grid>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Acenta Kodu</Typography>
                <Chip label={selectedAgent.agentCode} color="primary" size="small" />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Departman</Typography>
                <Chip label={selectedAgent.department} color="info" size="small" />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Telefon</Typography>
                <Typography variant="body1">{selectedAgent.phone || 'N/A'}</Typography>
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="text.secondary">Adres</Typography>
                <Typography variant="body1">{selectedAgent.address || 'N/A'}</Typography>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenAgentDetailModal(false)}>Kapat</Button>
        </DialogActions>
      </Dialog>

      {/* Agent Edit Modal */}
      <Dialog 
        open={openAgentEditModal} 
        onClose={() => {
          setOpenAgentEditModal(false);
          // Reset form states
          setAgentEditFormData({
            name: '',
            email: '',
            password: '',
            confirmPassword: '',
            department: '',
            address: '',
            phone: ''
          });
        }} 
        maxWidth="md" 
        fullWidth
      >
        <DialogTitle>Acenta Düzenle</DialogTitle>
        <DialogContent>
          {selectedAgent && (
            <Box sx={{ mt: 2 }}>
              <Grid container spacing={2}>
                {/* User Bilgileri */}
                <Grid item xs={12}>
                  <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                    Kullanıcı Bilgileri
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Ad Soyad"
                    value={agentEditFormData.name}
                    onChange={(e) => setAgentEditFormData(prev => ({ ...prev, name: e.target.value }))}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="E-posta"
                    type="email"
                    value={agentEditFormData.email}
                    onChange={(e) => setAgentEditFormData(prev => ({ ...prev, email: e.target.value }))}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Yeni Şifre"
                    type="password"
                    value={agentEditFormData.password}
                    onChange={(e) => setAgentEditFormData(prev => ({ ...prev, password: e.target.value }))}
                    helperText="Şifreyi değiştirmek istemiyorsanız boş bırakın"
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Şifre Tekrar"
                    type="password"
                    value={agentEditFormData.confirmPassword}
                    onChange={(e) => setAgentEditFormData(prev => ({ ...prev, confirmPassword: e.target.value }))}
                    error={agentEditFormData.password !== '' && agentEditFormData.password !== agentEditFormData.confirmPassword}
                    helperText={agentEditFormData.password !== '' && agentEditFormData.password !== agentEditFormData.confirmPassword ? 'Şifreler eşleşmiyor' : ''}
                  />
                </Grid>

                {/* Agent Bilgileri */}
                <Grid item xs={12} sx={{ mt: 2 }}>
                  <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                    Acenta Bilgileri
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <FormControl fullWidth>
                    <InputLabel>Departman</InputLabel>
                    <Select
                      value={agentEditFormData.department || selectedAgent.department}
                      label="Departman"
                      onChange={(e) => {
                        const dept = e.target.value;
                        setAgentEditFormData(prev => ({ ...prev, department: dept }));
                      }}
                    >
                      <MenuItem key="Konut-edit" value="Konut Sigortası">Konut Sigortası</MenuItem>
                      <MenuItem key="Seyahat-edit" value="Seyahat Sigortası">Seyahat Sigortası</MenuItem>
                      <MenuItem key="İşYeri-edit" value="İş Yeri Sigortası">İş Yeri Sigortası</MenuItem>
                      <MenuItem key="Trafik-edit" value="Trafik Sigortası">Trafik Sigortası</MenuItem>
                      <MenuItem key="Sağlık-edit" value="Sağlık Sigortası">Sağlık Sigortası</MenuItem>
                      <MenuItem key="Hayat-edit" value="Hayat Sigortası">Hayat Sigortası</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Acenta Kodu (Otomatik)"
                    value={
                      (() => {
                        const dept = agentEditFormData.department || selectedAgent.department;
                        const deptMap: Record<string, string> = {
                          'Konut Sigortası': 'KON',
                          'Seyahat Sigortası': 'SEY',
                          'İş Yeri Sigortası': 'İŞ',
                          'Trafik Sigortası': 'TRA',
                          'Sağlık Sigortası': 'SAĞ',
                          'Hayat Sigortası': 'HAY'
                        };
                        return deptMap[dept] || dept.substring(0, 3).toUpperCase();
                      })()
                    }
                    disabled
                    helperText="Departman değiştiğinde otomatik güncellenir"
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Adres"
                    value={agentEditFormData.address || selectedAgent.address || ''}
                    onChange={(e) => setAgentEditFormData(prev => ({ ...prev, address: e.target.value }))}
                    multiline
                    rows={2}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Telefon"
                    value={agentEditFormData.phone || selectedAgent.phone || ''}
                    onChange={(e) => setAgentEditFormData(prev => ({ ...prev, phone: e.target.value }))}
                  />
                </Grid>
              </Grid>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => {
            setOpenAgentEditModal(false);
            setAgentEditFormData({
              name: '',
              email: '',
              password: '',
              confirmPassword: '',
              department: '',
              address: '',
              phone: ''
            });
          }}>İptal</Button>
          <Button 
            variant="contained" 
            disabled={agentEditFormData.password !== '' && agentEditFormData.password !== agentEditFormData.confirmPassword}
            onClick={async () => {
              if (!selectedAgent) return;
              
              // Şifre kontrolü
              if (agentEditFormData.password && agentEditFormData.password !== agentEditFormData.confirmPassword) {
                alert('Şifreler eşleşmiyor!');
                return;
              }
              
              try {
                const updateData: any = {};
                
                // Sadece değişen alanları gönder
                if (agentEditFormData.name && agentEditFormData.name !== selectedAgent.userName) {
                  updateData.name = agentEditFormData.name;
                }
                if (agentEditFormData.email && agentEditFormData.email !== selectedAgent.userEmail) {
                  updateData.email = agentEditFormData.email;
                }
                if (agentEditFormData.password) {
                  updateData.password = agentEditFormData.password;
                }
                if (agentEditFormData.department && agentEditFormData.department !== selectedAgent.department) {
                  updateData.department = agentEditFormData.department;
                  // Departman değiştiğinde agentCode'u da gönder
                  const deptMap: Record<string, string> = {
                    'Konut Sigortası': 'KON',
                    'Seyahat Sigortası': 'SEY',
                    'İş Yeri Sigortası': 'İŞ',
                    'Trafik Sigortası': 'TRA',
                    'Sağlık Sigortası': 'SAĞ',
                    'Hayat Sigortası': 'HAY'
                  };
                  updateData.agentCode = deptMap[agentEditFormData.department] || agentEditFormData.department.substring(0, 3).toUpperCase();
                }
                if (agentEditFormData.address && agentEditFormData.address !== selectedAgent.address) {
                  updateData.address = agentEditFormData.address;
                }
                if (agentEditFormData.phone && agentEditFormData.phone !== selectedAgent.phone) {
                  updateData.phone = agentEditFormData.phone;
                }

                console.log('Updating agent with data:', updateData);
                await apiService.updateAgent(selectedAgent.agentId || selectedAgent.id, updateData);
                
                setOpenAgentEditModal(false);
                setSelectedAgent(null);
                setAgentEditFormData({
                  name: '',
                  email: '',
                  password: '',
                  confirmPassword: '',
                  department: '',
                  address: '',
                  phone: ''
                });
                fetchAdminData(); // Refresh data
                alert('Acenta başarıyla güncellendi!');
              } catch (error: any) {
                console.error('Agent update error:', error);
                alert(error.message || 'Acenta güncellenirken bir hata oluştu');
              }
            }}
          >
            Kaydet
          </Button>
        </DialogActions>
      </Dialog>

      {/* Agent Delete Confirmation Dialog */}
      <Dialog open={openAgentDeleteDialog} onClose={() => setOpenAgentDeleteDialog(false)}>
        <DialogTitle>Acentayı Sil</DialogTitle>
        <DialogContent>
          <Typography>
            <strong>{agentToDelete?.user?.name}</strong> isimli acentayı silmek istediğinizden emin misiniz?
          </Typography>
          <Alert severity="warning" sx={{ mt: 2 }}>
            Bu işlem geri alınamaz!
          </Alert>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenAgentDeleteDialog(false)}>İptal</Button>
          <Button onClick={handleDeleteAgentConfirm} color="error" variant="contained">Sil</Button>
        </DialogActions>
      </Dialog>

      {/* Customer Detail Modal */}
      <Dialog open={openCustomerDetailModal} onClose={() => setOpenCustomerDetailModal(false)} maxWidth="md" fullWidth>
        <DialogTitle>Müşteri Detayları</DialogTitle>
        <DialogContent>
          {selectedCustomer && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Ad Soyad</Typography>
                <Typography variant="body1">{selectedCustomer.user?.name || 'N/A'}</Typography>
              </Grid>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Email</Typography>
                <Typography variant="body1">{selectedCustomer.user?.email || 'N/A'}</Typography>
              </Grid>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">TC Kimlik No</Typography>
                <Typography variant="body1">{selectedCustomer.idNo || 'N/A'}</Typography>
              </Grid>
              <Grid item xs={12} sm={6}>
                <Typography variant="subtitle2" color="text.secondary">Telefon</Typography>
                <Typography variant="body1">{selectedCustomer.phone || 'N/A'}</Typography>
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2" color="text.secondary">Adres</Typography>
                <Typography variant="body1">{selectedCustomer.address || 'N/A'}</Typography>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenCustomerDetailModal(false)}>Kapat</Button>
        </DialogActions>
      </Dialog>

      {/* Customer Edit Modal */}
      <Dialog 
        open={openCustomerEditModal} 
        onClose={() => {
          setOpenCustomerEditModal(false);
          setCustomerEditFormData({
            name: '',
            email: '',
            password: '',
            confirmPassword: '',
            idNo: '',
            address: '',
            phone: ''
          });
        }} 
        maxWidth="md" 
        fullWidth
      >
        <DialogTitle>Müşteri Düzenle</DialogTitle>
        <DialogContent>
          {selectedCustomer && (
            <Box sx={{ mt: 2 }}>
              <Grid container spacing={2}>
                {/* User Bilgileri */}
                <Grid item xs={12}>
                  <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                    Kullanıcı Bilgileri
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Ad Soyad"
                    value={customerEditFormData.name}
                    onChange={(e) => setCustomerEditFormData(prev => ({ ...prev, name: e.target.value }))}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="E-posta"
                    type="email"
                    value={customerEditFormData.email}
                    onChange={(e) => setCustomerEditFormData(prev => ({ ...prev, email: e.target.value }))}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Yeni Şifre"
                    type="password"
                    value={customerEditFormData.password}
                    onChange={(e) => setCustomerEditFormData(prev => ({ ...prev, password: e.target.value }))}
                    helperText="Şifreyi değiştirmek istemiyorsanız boş bırakın"
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <TextField
                    fullWidth
                    label="Şifre Tekrar"
                    type="password"
                    value={customerEditFormData.confirmPassword}
                    onChange={(e) => setCustomerEditFormData(prev => ({ ...prev, confirmPassword: e.target.value }))}
                    error={customerEditFormData.password !== '' && customerEditFormData.password !== customerEditFormData.confirmPassword}
                    helperText={customerEditFormData.password !== '' && customerEditFormData.password !== customerEditFormData.confirmPassword ? 'Şifreler eşleşmiyor' : ''}
                  />
                </Grid>

                {/* Customer Bilgileri */}
                <Grid item xs={12} sx={{ mt: 2 }}>
                  <Typography variant="subtitle1" fontWeight="bold" gutterBottom>
                    Müşteri Bilgileri
                  </Typography>
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="TC/Vergi No"
                    value={customerEditFormData.idNo || selectedCustomer.idNo || ''}
                    onChange={(e) => setCustomerEditFormData(prev => ({ ...prev, idNo: e.target.value }))}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Adres"
                    value={customerEditFormData.address || selectedCustomer.address || ''}
                    onChange={(e) => setCustomerEditFormData(prev => ({ ...prev, address: e.target.value }))}
                    multiline
                    rows={2}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Telefon"
                    value={customerEditFormData.phone || selectedCustomer.phone || ''}
                    onChange={(e) => setCustomerEditFormData(prev => ({ ...prev, phone: e.target.value }))}
                  />
                </Grid>
              </Grid>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => {
            setOpenCustomerEditModal(false);
            setCustomerEditFormData({
              name: '',
              email: '',
              password: '',
              confirmPassword: '',
              idNo: '',
              address: '',
              phone: ''
            });
          }}>İptal</Button>
          <Button 
            variant="contained" 
            disabled={customerEditFormData.password !== '' && customerEditFormData.password !== customerEditFormData.confirmPassword}
            onClick={async () => {
              if (!selectedCustomer) return;
              
              // Şifre kontrolü
              if (customerEditFormData.password && customerEditFormData.password !== customerEditFormData.confirmPassword) {
                alert('Şifreler eşleşmiyor!');
                return;
              }
              
              try {
                const updateData: any = {};
                
                // Sadece değişen alanları gönder
                if (customerEditFormData.name && customerEditFormData.name !== selectedCustomer.userName) {
                  updateData.name = customerEditFormData.name;
                }
                if (customerEditFormData.email && customerEditFormData.email !== selectedCustomer.userEmail) {
                  updateData.email = customerEditFormData.email;
                }
                if (customerEditFormData.password) {
                  updateData.password = customerEditFormData.password;
                }
                if (customerEditFormData.idNo && customerEditFormData.idNo !== selectedCustomer.idNo) {
                  updateData.idNo = customerEditFormData.idNo;
                }
                if (customerEditFormData.address && customerEditFormData.address !== selectedCustomer.address) {
                  updateData.address = customerEditFormData.address;
                }
                if (customerEditFormData.phone && customerEditFormData.phone !== selectedCustomer.phone) {
                  updateData.phone = customerEditFormData.phone;
                }

                console.log('Updating customer with data:', updateData);
                await apiService.updateCustomer(selectedCustomer.customerId, updateData);
                
                setOpenCustomerEditModal(false);
                setSelectedCustomer(null);
                setCustomerEditFormData({
                  name: '',
                  email: '',
                  password: '',
                  confirmPassword: '',
                  idNo: '',
                  address: '',
                  phone: ''
                });
                fetchAdminData(); // Refresh data
                alert('Müşteri başarıyla güncellendi!');
              } catch (error: any) {
                console.error('Customer update error:', error);
                alert(error.message || 'Müşteri güncellenirken bir hata oluştu');
              }
            }}
          >
            Kaydet
          </Button>
        </DialogActions>
      </Dialog>

      {/* Customer Delete Confirmation Dialog */}
      <Dialog open={openCustomerDeleteDialog} onClose={() => setOpenCustomerDeleteDialog(false)}>
        <DialogTitle>Müşteriyi Sil</DialogTitle>
        <DialogContent>
          <Typography>
            <strong>{customerToDelete?.user?.name}</strong> isimli müşteriyi silmek istediğinizden emin misiniz?
          </Typography>
          <Alert severity="warning" sx={{ mt: 2 }}>
            Bu işlem geri alınamaz!
          </Alert>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenCustomerDeleteDialog(false)}>İptal</Button>
          <Button onClick={handleDeleteCustomerConfirm} color="error" variant="contained">Sil</Button>
          </DialogActions>
        </Dialog>

        {/* Dashboard View Modal */}
        <Dialog 
          open={dashboardModalOpen} 
          onClose={() => setDashboardModalOpen(false)}
          maxWidth="xl"
          fullWidth
          PaperProps={{
            sx: { minHeight: '80vh', maxHeight: '90vh' }
          }}
        >
          <DialogTitle>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="h6">
                {dashboardViewType === 'customer' ? 'Müşteri Dashboard' : 'Acenta Dashboard'} - 
                {selectedDashboardUser && (
                  dashboardViewType === 'customer' 
                    ? (selectedDashboardUser as Customer).user?.name 
                    : (selectedDashboardUser as Agent).user?.name
                )}
              </Typography>
              <IconButton onClick={() => setDashboardModalOpen(false)}>
                <Cancel />
              </IconButton>
            </Box>
          </DialogTitle>
          
          <DialogContent dividers>
            {dashboardLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
                <CircularProgress />
              </Box>
            ) : (
              <>
                {/* Stats Cards */}
                <Grid container spacing={2} sx={{ mb: 3 }}>
                  {dashboardViewType === 'customer' && dashboardData.stats && (
                    <>
                      <Grid item xs={12} md={3}>
                        <Card>
                          <CardContent>
                            <Typography color="textSecondary" gutterBottom>Toplam Poliçe</Typography>
                            <Typography variant="h4">{dashboardData.stats.totalPolicies}</Typography>
                            <Typography variant="caption" color="success.main">
                              {dashboardData.stats.activePolicies} Aktif
                            </Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                      <Grid item xs={12} md={3}>
                        <Card>
                          <CardContent>
                            <Typography color="textSecondary" gutterBottom>Olay Bildirimleri</Typography>
                            <Typography variant="h4">{dashboardData.stats.totalClaims}</Typography>
                            <Typography variant="caption" color="warning.main">
                              {dashboardData.stats.pendingClaims} Bekleyen
                            </Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                      <Grid item xs={12} md={3}>
                        <Card>
                          <CardContent>
                            <Typography color="textSecondary" gutterBottom>Teklif Talepleri</Typography>
                            <Typography variant="h4">{dashboardData.stats.totalOffers}</Typography>
                            <Typography variant="caption" color="info.main">
                              {dashboardData.stats.pendingOffers} Bekleyen
                            </Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                      <Grid item xs={12} md={3}>
                        <Card>
                          <CardContent>
                            <Typography color="textSecondary" gutterBottom>Belgeler</Typography>
                            <Typography variant="h4">{dashboardData.documents?.length || 0}</Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                    </>
                  )}
                  
                  {dashboardViewType === 'agent' && dashboardData.stats && (
                    <>
                      <Grid item xs={12} md={4}>
                        <Card>
                          <CardContent>
                            <Typography color="textSecondary" gutterBottom>Departman</Typography>
                            <Typography variant="h6">{dashboardData.stats.department}</Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                      <Grid item xs={12} md={4}>
                        <Card>
                          <CardContent>
                            <Typography color="textSecondary" gutterBottom>Departman Teklifleri</Typography>
                            <Typography variant="h4">{dashboardData.stats.totalOffers}</Typography>
                            <Typography variant="caption" color="warning.main">
                              {dashboardData.stats.pendingOffers} Bekleyen / {dashboardData.stats.approvedOffers} Onaylanmış
                            </Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                      <Grid item xs={12} md={4}>
                        <Card>
                          <CardContent>
                            <Typography color="textSecondary" gutterBottom>Olay Bildirimleri</Typography>
                            <Typography variant="h4">{dashboardData.stats.totalClaims}</Typography>
                            <Typography variant="caption" color="warning.main">
                              {dashboardData.stats.pendingClaims} Bekleyen / {dashboardData.stats.approvedClaims} Onaylanmış
                            </Typography>
                          </CardContent>
                        </Card>
                      </Grid>
                    </>
                  )}
                </Grid>

                {/* Tabs for different sections */}
                <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
                  {dashboardViewType === 'customer' && (
                    <Tabs 
                      value={dashboardTabValue} 
                      onChange={(event: React.SyntheticEvent, newValue: number) => {
                        console.log('🔄 Customer Tab changed:', newValue, 'from', dashboardTabValue);
                        setDashboardTabValue(newValue);
                      }}
                    >
                      <Tab value={0} label={`Poliçeler (${dashboardData.policies?.length || 0})`} />
                      <Tab value={1} label={`Teklif Talepleri (${dashboardData.offers?.length || 0})`} />
                      <Tab value={2} label={`Olay Bildirimleri (${dashboardData.claims?.length || 0})`} />
                      <Tab value={3} label={`Belgeler (${dashboardData.documents?.length || 0})`} />
                    </Tabs>
                  )}
                  {dashboardViewType === 'agent' && (
                    <Tabs 
                      value={dashboardTabValue} 
                      onChange={(event: React.SyntheticEvent, newValue: number) => {
                        console.log('🔄 Agent Tab changed:', newValue, 'from', dashboardTabValue);
                        setDashboardTabValue(newValue);
                      }}
                    >
                      <Tab value={0} label={`Teklifler (${dashboardData.offers?.length || 0})`} />
                      <Tab value={1} label={`Olay Bildirimleri (${dashboardData.claims?.length || 0})`} />
                    </Tabs>
                  )}
                </Box>

                {/* Data Tables - Show content based on selected tab */}
                {dashboardViewType === 'customer' && (
                  <Box>
                    {/* Tab 0: Poliçeler */}
                    {dashboardTabValue === 0 && (
                      dashboardData.policies && dashboardData.policies.length > 0 ? (
                        <>
                          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                            <TextField
                              size="small"
                              placeholder="Poliçelerde ara"
                              value={dashboardPolicySearch}
                              onChange={(event: React.ChangeEvent<HTMLInputElement>) => setDashboardPolicySearch(event.target.value)}
                              InputProps={{
                                startAdornment: (
                                  <InputAdornment position="start">
                                    <Search fontSize="small" />
                                  </InputAdornment>
                                )
                              }}
                            />
                          </Box>
                          {filteredPolicies.length > 0 ? (
                            <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
                              <Table stickyHeader size="small">
                                <TableHead>
                                  <TableRow>
                                    <TableCell sortDirection={dashboardPolicySortBy === 'policyNumber' ? dashboardPolicySortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardPolicySortBy === 'policyNumber'}
                                        direction={dashboardPolicySortBy === 'policyNumber' ? dashboardPolicySortOrder : 'asc'}
                                        onClick={() => handleDashboardPolicySort('policyNumber')}
                                      >
                                        Poliçe No
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardPolicySortBy === 'insuranceType' ? dashboardPolicySortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardPolicySortBy === 'insuranceType'}
                                        direction={dashboardPolicySortBy === 'insuranceType' ? dashboardPolicySortOrder : 'asc'}
                                        onClick={() => handleDashboardPolicySort('insuranceType')}
                                      >
                                        Sigorta Türü
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardPolicySortBy === 'startDate' ? dashboardPolicySortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardPolicySortBy === 'startDate'}
                                        direction={dashboardPolicySortBy === 'startDate' ? dashboardPolicySortOrder : 'asc'}
                                        onClick={() => handleDashboardPolicySort('startDate')}
                                      >
                                        Başlangıç
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardPolicySortBy === 'endDate' ? dashboardPolicySortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardPolicySortBy === 'endDate'}
                                        direction={dashboardPolicySortBy === 'endDate' ? dashboardPolicySortOrder : 'asc'}
                                        onClick={() => handleDashboardPolicySort('endDate')}
                                      >
                                        Bitiş
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardPolicySortBy === 'premium' ? dashboardPolicySortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardPolicySortBy === 'premium'}
                                        direction={dashboardPolicySortBy === 'premium' ? dashboardPolicySortOrder : 'asc'}
                                        onClick={() => handleDashboardPolicySort('premium')}
                                      >
                                        Prim
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardPolicySortBy === 'status' ? dashboardPolicySortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardPolicySortBy === 'status'}
                                        direction={dashboardPolicySortBy === 'status' ? dashboardPolicySortOrder : 'asc'}
                                        onClick={() => handleDashboardPolicySort('status')}
                                      >
                                        Durum
                                      </TableSortLabel>
                                    </TableCell>
                                  </TableRow>
                                </TableHead>
                                <TableBody>
                                  {filteredPolicies.map((policy: any) => (
                                    <TableRow key={policy.policyId}>
                                      <TableCell>{policy.policyNumber}</TableCell>
                                      <TableCell>{policy.insuranceTypeName || 'N/A'}</TableCell>
                                      <TableCell>{policy.startDate ? new Date(policy.startDate).toLocaleDateString('tr-TR') : 'N/A'}</TableCell>
                                      <TableCell>{policy.endDate ? new Date(policy.endDate).toLocaleDateString('tr-TR') : 'N/A'}</TableCell>
                                      <TableCell>₺{policy.totalPremium?.toLocaleString('tr-TR')}</TableCell>
                                      <TableCell>
                                        <Chip 
                                          label={policy.status} 
                                          size="small"
                                          color={policy.status?.toLowerCase() === 'active' ? 'success' : 'default'}
                                        />
                                      </TableCell>
                                    </TableRow>
                                  ))}
                                </TableBody>
                              </Table>
                            </TableContainer>
                          ) : (
                            <Box sx={{ textAlign: 'center', py: 4 }}>
                              <Typography variant="body2" color="text.secondary">
                                Arama kriterine uygun poliçe bulunamadı.
                              </Typography>
                            </Box>
                          )}
                        </>
                      ) : (
                        <Box sx={{ textAlign: 'center', py: 4 }}>
                          <Typography variant="body2" color="text.secondary">
                            Henüz poliçe bulunmuyor.
                          </Typography>
                        </Box>
                      )
                    )}

                    {/* Tab 1: Teklif Talepleri */}
                    {dashboardTabValue === 1 && (
                      dashboardData.offers && dashboardData.offers.length > 0 ? (
                        <>
                          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                            <TextField
                              size="small"
                              placeholder="Tekliflerde ara"
                              value={dashboardOfferSearch}
                              onChange={(event: React.ChangeEvent<HTMLInputElement>) => setDashboardOfferSearch(event.target.value)}
                              InputProps={{
                                startAdornment: (
                                  <InputAdornment position="start">
                                    <Search fontSize="small" />
                                  </InputAdornment>
                                )
                              }}
                            />
                          </Box>
                          {filteredOffers.length > 0 ? (
                            <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
                              <Table stickyHeader size="small">
                                <TableHead>
                                  <TableRow>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'offerId' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'offerId'}
                                        direction={dashboardOfferSortBy === 'offerId' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('offerId')}
                                      >
                                        Teklif ID
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'insuranceType' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'insuranceType'}
                                        direction={dashboardOfferSortBy === 'insuranceType' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('insuranceType')}
                                      >
                                        Sigorta Türü
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'price' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'price'}
                                        direction={dashboardOfferSortBy === 'price' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('price')}
                                      >
                                        Tutar
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'status' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'status'}
                                        direction={dashboardOfferSortBy === 'status' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('status')}
                                      >
                                        Durum
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'date' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'date'}
                                        direction={dashboardOfferSortBy === 'date' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('date')}
                                      >
                                        Tarih
                                      </TableSortLabel>
                                    </TableCell>
                                  </TableRow>
                                </TableHead>
                                <TableBody>
                                  {filteredOffers.map((offer: any) => (
                                    <TableRow key={offer.offerId}>
                                      <TableCell>{offer.offerId}</TableCell>
                                      <TableCell>{offer.insuranceTypeName || 'N/A'}</TableCell>
                                      <TableCell>₺{offer.finalPrice?.toLocaleString('tr-TR')}</TableCell>
                                      <TableCell>
                                        <Chip 
                                          label={offer.status} 
                                          size="small"
                                          color={offer.status === 'approved' ? 'success' : 'warning'}
                                        />
                                      </TableCell>
                                      <TableCell>{offer.createdAt ? new Date(offer.createdAt).toLocaleDateString('tr-TR') : 'N/A'}</TableCell>
                                    </TableRow>
                                  ))}
                                </TableBody>
                              </Table>
                            </TableContainer>
                          ) : (
                            <Box sx={{ textAlign: 'center', py: 4 }}>
                              <Typography variant="body2" color="text.secondary">
                                Arama kriterine uygun teklif bulunamadı.
                              </Typography>
                            </Box>
                          )}
                        </>
                      ) : (
                        <Box sx={{ textAlign: 'center', py: 4 }}>
                          <Typography variant="body2" color="text.secondary">
                            Henüz teklif talebi bulunmuyor.
                          </Typography>
                        </Box>
                      )
                    )}

                    {/* Tab 2: Olay Bildirimleri */}
                    {dashboardTabValue === 2 && (
                      dashboardData.claims && dashboardData.claims.length > 0 ? (
                        <>
                          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                            <TextField
                              size="small"
                              placeholder="Olay bildirimlerinde ara"
                              value={dashboardClaimSearch}
                              onChange={(event: React.ChangeEvent<HTMLInputElement>) => setDashboardClaimSearch(event.target.value)}
                              InputProps={{
                                startAdornment: (
                                  <InputAdornment position="start">
                                    <Search fontSize="small" />
                                  </InputAdornment>
                                )
                              }}
                            />
                          </Box>
                          {filteredClaims.length > 0 ? (
                            <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
                              <Table stickyHeader size="small">
                                <TableHead>
                                  <TableRow>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'claimId' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'claimId'}
                                        direction={dashboardClaimSortBy === 'claimId' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('claimId')}
                                      >
                                        Bildirim ID
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'policyNumber' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'policyNumber'}
                                        direction={dashboardClaimSortBy === 'policyNumber' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('policyNumber')}
                                      >
                                        Poliçe No
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'type' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'type'}
                                        direction={dashboardClaimSortBy === 'type' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('type')}
                                      >
                                        Tür
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell>Açıklama</TableCell>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'status' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'status'}
                                        direction={dashboardClaimSortBy === 'status' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('status')}
                                      >
                                        Durum
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'date' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'date'}
                                        direction={dashboardClaimSortBy === 'date' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('date')}
                                      >
                                        Tarih
                                      </TableSortLabel>
                                    </TableCell>
                                  </TableRow>
                                </TableHead>
                                <TableBody>
                                  {filteredClaims.map((claim: any) => (
                                    <TableRow key={claim.claimId}>
                                      <TableCell>{claim.claimId}</TableCell>
                                      <TableCell>{claim.policyNumber || 'N/A'}</TableCell>
                                      <TableCell>{claim.type || 'N/A'}</TableCell>
                                      <TableCell>{claim.description || 'N/A'}</TableCell>
                                      <TableCell>
                                        <Chip 
                                          label={claim.status} 
                                          size="small"
                                          color={
                                            claim.status === 'Approved'
                                              ? 'success'
                                              : claim.status === 'Pending'
                                                ? 'warning'
                                                : 'error'
                                          }
                                        />
                                      </TableCell>
                                      <TableCell>{claim.createdAt ? new Date(claim.createdAt).toLocaleDateString('tr-TR') : 'N/A'}</TableCell>
                                    </TableRow>
                                  ))}
                                </TableBody>
                              </Table>
                            </TableContainer>
                          ) : (
                            <Box sx={{ textAlign: 'center', py: 4 }}>
                              <Typography variant="body2" color="text.secondary">
                                Arama kriterine uygun olay bildirimi bulunamadı.
                              </Typography>
                            </Box>
                          )}
                        </>
                      ) : (
                        <Box sx={{ textAlign: 'center', py: 4 }}>
                          <Typography variant="body2" color="text.secondary">
                            Henüz olay bildirimi bulunmuyor.
                          </Typography>
                        </Box>
                      )
                    )}

                    {/* Tab 3: Belgeler */}
                    {dashboardTabValue === 3 && (
                      dashboardData.documents && dashboardData.documents.length > 0 ? (
                        <>
                          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                            <TextField
                              size="small"
                              placeholder="Belgelerde ara"
                              value={dashboardDocumentSearch}
                              onChange={(event: React.ChangeEvent<HTMLInputElement>) => setDashboardDocumentSearch(event.target.value)}
                              InputProps={{
                                startAdornment: (
                                  <InputAdornment position="start">
                                    <Search fontSize="small" />
                                  </InputAdornment>
                                )
                              }}
                            />
                          </Box>
                          {filteredDocuments.length > 0 ? (
                            <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
                              <Table stickyHeader size="small">
                                <TableHead>
                                  <TableRow>
                                    <TableCell sortDirection={dashboardDocumentSortBy === 'fileName' ? dashboardDocumentSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardDocumentSortBy === 'fileName'}
                                        direction={dashboardDocumentSortBy === 'fileName' ? dashboardDocumentSortOrder : 'asc'}
                                        onClick={() => handleDashboardDocumentSort('fileName')}
                                      >
                                        Dosya Adı
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardDocumentSortBy === 'category' ? dashboardDocumentSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardDocumentSortBy === 'category'}
                                        direction={dashboardDocumentSortBy === 'category' ? dashboardDocumentSortOrder : 'asc'}
                                        onClick={() => handleDashboardDocumentSort('category')}
                                      >
                                        Kategori
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardDocumentSortBy === 'size' ? dashboardDocumentSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardDocumentSortBy === 'size'}
                                        direction={dashboardDocumentSortBy === 'size' ? dashboardDocumentSortOrder : 'asc'}
                                        onClick={() => handleDashboardDocumentSort('size')}
                                      >
                                        Boyut
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardDocumentSortBy === 'date' ? dashboardDocumentSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardDocumentSortBy === 'date'}
                                        direction={dashboardDocumentSortBy === 'date' ? dashboardDocumentSortOrder : 'asc'}
                                        onClick={() => handleDashboardDocumentSort('date')}
                                      >
                                        Yükleme Tarihi
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell>İşlemler</TableCell>
                                  </TableRow>
                                </TableHead>
                                <TableBody>
                                  {filteredDocuments.map((doc: any, index: number) => {
                                    const documentKey = doc.id ?? doc.documentId ?? doc.documentID ?? `doc-${index}`;
                                    const formattedSize = doc.fileSize ? `${(doc.fileSize / 1024).toFixed(2)} KB` : 'N/A';
                                    const uploadedAt = doc.uploadedAt ? new Date(doc.uploadedAt).toLocaleDateString('tr-TR') : 'N/A';
                                    return (
                                      <TableRow key={documentKey}>
                                        <TableCell>{doc.fileName || doc.name || 'N/A'}</TableCell>
                                        <TableCell>{doc.category || 'N/A'}</TableCell>
                                        <TableCell>{formattedSize}</TableCell>
                                        <TableCell>{uploadedAt}</TableCell>
                                        <TableCell>
                                          <IconButton
                                            size="small"
                                            color="primary"
                                            onClick={() => {
                                              setSelectedDocument(doc);
                                              setDocumentViewModalOpen(true);
                                            }}
                                            title="Belgeyi Görüntüle"
                                          >
                                            <Visibility />
                                          </IconButton>
                                        </TableCell>
                                      </TableRow>
                                    );
                                  })}
                                </TableBody>
                              </Table>
                            </TableContainer>
                          ) : (
                            <Box sx={{ textAlign: 'center', py: 4 }}>
                              <Typography variant="body2" color="text.secondary">
                                Arama kriterine uygun belge bulunamadı.
                              </Typography>
                            </Box>
                          )}
                        </>
                      ) : (
                        <Box sx={{ textAlign: 'center', py: 4 }}>
                          <Typography variant="body2" color="text.secondary">
                            Henüz belge bulunmuyor.
                          </Typography>
                        </Box>
                      )
                    )}
                  </Box>
                )}

                {/* Agent Dashboard Tabs */}
                {dashboardViewType === 'agent' && (
                  <>
                    {/* Tab 0: Teklifler */}
                    {dashboardTabValue === 0 && (
                      dashboardData.offers && dashboardData.offers.length > 0 ? (
                        <>
                          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                            <TextField
                              size="small"
                              placeholder="Tekliflerde ara"
                              value={dashboardOfferSearch}
                              onChange={(event: React.ChangeEvent<HTMLInputElement>) => setDashboardOfferSearch(event.target.value)}
                              InputProps={{
                                startAdornment: (
                                  <InputAdornment position="start">
                                    <Search fontSize="small" />
                                  </InputAdornment>
                                )
                              }}
                            />
                          </Box>
                          {filteredOffers.length > 0 ? (
                            <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
                              <Table stickyHeader size="small">
                                <TableHead>
                                  <TableRow>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'offerId' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'offerId'}
                                        direction={dashboardOfferSortBy === 'offerId' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('offerId')}
                                      >
                                        Teklif ID
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'customer' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'customer'}
                                        direction={dashboardOfferSortBy === 'customer' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('customer')}
                                      >
                                        Müşteri
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'insuranceType' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'insuranceType'}
                                        direction={dashboardOfferSortBy === 'insuranceType' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('insuranceType')}
                                      >
                                        Sigorta Türü
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'price' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'price'}
                                        direction={dashboardOfferSortBy === 'price' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('price')}
                                      >
                                        Tutar
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'status' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'status'}
                                        direction={dashboardOfferSortBy === 'status' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('status')}
                                      >
                                        Durum
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardOfferSortBy === 'date' ? dashboardOfferSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardOfferSortBy === 'date'}
                                        direction={dashboardOfferSortBy === 'date' ? dashboardOfferSortOrder : 'asc'}
                                        onClick={() => handleDashboardOfferSort('date')}
                                      >
                                        Tarih
                                      </TableSortLabel>
                                    </TableCell>
                                  </TableRow>
                                </TableHead>
                                <TableBody>
                                  {filteredOffers.map((offer: any) => (
                                    <TableRow key={offer.offerId}>
                                      <TableCell>{offer.offerId}</TableCell>
                                      <TableCell>{offer.customerName || offer.customer?.user?.name || 'N/A'}</TableCell>
                                      <TableCell>{offer.insuranceTypeName || offer.insuranceType?.name || 'N/A'}</TableCell>
                                      <TableCell>₺{offer.finalPrice?.toLocaleString('tr-TR')}</TableCell>
                                      <TableCell>
                                        <Chip 
                                          label={offer.status} 
                                          size="small"
                                          color={offer.status === 'approved' ? 'success' : 'warning'}
                                        />
                                      </TableCell>
                                      <TableCell>{offer.createdAt ? new Date(offer.createdAt).toLocaleDateString('tr-TR') : 'N/A'}</TableCell>
                                    </TableRow>
                                  ))}
                                </TableBody>
                              </Table>
                            </TableContainer>
                          ) : (
                            <Box sx={{ textAlign: 'center', py: 4 }}>
                              <Typography variant="body2" color="text.secondary">
                                Arama kriterine uygun teklif bulunamadı.
                              </Typography>
                            </Box>
                          )}
                        </>
                      ) : (
                        <Box sx={{ textAlign: 'center', py: 4 }}>
                          <Typography variant="body2" color="text.secondary">
                            Henüz teklif bulunmuyor.
                          </Typography>
                        </Box>
                      )
                    )}

                    {/* Tab 1: Olay Bildirimleri */}
                    {dashboardTabValue === 1 && (
                      dashboardData.claims && dashboardData.claims.length > 0 ? (
                        <>
                          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
                            <TextField
                              size="small"
                              placeholder="Olay bildirimlerinde ara"
                              value={dashboardClaimSearch}
                              onChange={(event: React.ChangeEvent<HTMLInputElement>) => setDashboardClaimSearch(event.target.value)}
                              InputProps={{
                                startAdornment: (
                                  <InputAdornment position="start">
                                    <Search fontSize="small" />
                                  </InputAdornment>
                                )
                              }}
                            />
                          </Box>
                          {filteredClaims.length > 0 ? (
                            <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
                              <Table stickyHeader size="small">
                                <TableHead>
                                  <TableRow>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'claimId' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'claimId'}
                                        direction={dashboardClaimSortBy === 'claimId' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('claimId')}
                                      >
                                        Bildirim ID
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'policyNumber' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'policyNumber'}
                                        direction={dashboardClaimSortBy === 'policyNumber' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('policyNumber')}
                                      >
                                        Poliçe No
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'type' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'type'}
                                        direction={dashboardClaimSortBy === 'type' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('type')}
                                      >
                                        Tür
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell>Açıklama</TableCell>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'status' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'status'}
                                        direction={dashboardClaimSortBy === 'status' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('status')}
                                      >
                                        Durum
                                      </TableSortLabel>
                                    </TableCell>
                                    <TableCell sortDirection={dashboardClaimSortBy === 'date' ? dashboardClaimSortOrder : false}>
                                      <TableSortLabel
                                        active={dashboardClaimSortBy === 'date'}
                                        direction={dashboardClaimSortBy === 'date' ? dashboardClaimSortOrder : 'asc'}
                                        onClick={() => handleDashboardClaimSort('date')}
                                      >
                                        Tarih
                                      </TableSortLabel>
                                    </TableCell>
                                  </TableRow>
                                </TableHead>
                                <TableBody>
                                  {filteredClaims.map((claim: any) => (
                                    <TableRow key={claim.claimId}>
                                      <TableCell>{claim.claimId}</TableCell>
                                      <TableCell>{claim.policyNumber || 'N/A'}</TableCell>
                                      <TableCell>{claim.type || 'N/A'}</TableCell>
                                      <TableCell>{claim.description || 'N/A'}</TableCell>
                                      <TableCell>
                                        <Chip 
                                          label={claim.status} 
                                          size="small"
                                          color={
                                            claim.status === 'Approved'
                                              ? 'success'
                                              : claim.status === 'Pending'
                                                ? 'warning'
                                                : 'error'
                                          }
                                        />
                                      </TableCell>
                                      <TableCell>{claim.createdAt ? new Date(claim.createdAt).toLocaleDateString('tr-TR') : 'N/A'}</TableCell>
                                    </TableRow>
                                  ))}
                                </TableBody>
                              </Table>
                            </TableContainer>
                          ) : (
                            <Box sx={{ textAlign: 'center', py: 4 }}>
                              <Typography variant="body2" color="text.secondary">
                                Arama kriterine uygun olay bildirimi bulunamadı.
                              </Typography>
                            </Box>
                          )}
                        </>
                      ) : (
                        <Box sx={{ textAlign: 'center', py: 4 }}>
                          <Typography variant="body2" color="text.secondary">
                            Henüz olay bildirimi bulunmuyor.
                          </Typography>
                        </Box>
                      )
                    )}
                  </>
                )}
              </>
            )}
          </DialogContent>
          
          <DialogActions>
            <Button onClick={() => setDashboardModalOpen(false)} variant="outlined">
              Kapat
            </Button>
          </DialogActions>
        </Dialog>

        {/* Document View Modal */}
        <Dialog
          open={documentViewModalOpen}
          onClose={() => setDocumentViewModalOpen(false)}
          maxWidth="lg"
          fullWidth
          PaperProps={{
            sx: { minHeight: '70vh', maxHeight: '85vh' }
          }}
        >
          <DialogTitle>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Typography variant="h6">
                {selectedDocument?.fileName || selectedDocument?.name || 'Belge Görüntüle'}
              </Typography>
              <IconButton onClick={() => setDocumentViewModalOpen(false)}>
                <Cancel />
              </IconButton>
            </Box>
          </DialogTitle>

          <DialogContent dividers>
            {selectedDocument && (
              <Box sx={{ width: '100%', height: '60vh', display: 'flex', flexDirection: 'column', gap: 2 }}>
                {/* Document Info */}
                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    <strong>Kategori:</strong> {selectedDocument.category || 'N/A'}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    <strong>Boyut:</strong> {selectedDocument.fileSize ? `${(selectedDocument.fileSize / 1024).toFixed(2)} KB` : 'N/A'}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    <strong>Yükleme Tarihi:</strong> {selectedDocument.uploadedAt ? new Date(selectedDocument.uploadedAt).toLocaleDateString('tr-TR') : 'N/A'}
                  </Typography>
                </Box>

                {/* Document Viewer */}
                {selectedDocument.fileUrl ? (
                  <Box sx={{ flex: 1, border: '1px solid #e0e0e0', borderRadius: 1, overflow: 'hidden' }}>
                    {selectedDocument.fileType?.toLowerCase().includes('pdf') || selectedDocument.fileName?.toLowerCase().endsWith('.pdf') ? (
                      <iframe
                        src={selectedDocument.fileUrl}
                        style={{
                          width: '100%',
                          height: '100%',
                          border: 'none'
                        }}
                        title={selectedDocument.fileName || 'Belge'}
                      />
                    ) : selectedDocument.fileType?.toLowerCase().match(/image\/(jpeg|jpg|png|gif|webp)/) || 
                         selectedDocument.fileName?.toLowerCase().match(/\.(jpg|jpeg|png|gif|webp)$/i) ? (
                      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%', p: 2 }}>
                        <img
                          src={selectedDocument.fileUrl}
                          alt={selectedDocument.fileName || 'Belge'}
                          style={{
                            maxWidth: '100%',
                            maxHeight: '100%',
                            objectFit: 'contain'
                          }}
                        />
                      </Box>
                    ) : (
                      <Box sx={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', height: '100%', p: 4 }}>
                        <Typography variant="body1" color="text.secondary" gutterBottom>
                          Bu belge türü tarayıcıda görüntülenemiyor.
                        </Typography>
                        <Button
                          variant="contained"
                          color="primary"
                          onClick={() => window.open(selectedDocument.fileUrl, '_blank')}
                          sx={{ mt: 2 }}
                        >
                          Belgeyi Yeni Sekmede Aç
                        </Button>
                      </Box>
                    )}
                  </Box>
                ) : (
                  <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%' }}>
                    <Typography variant="body1" color="text.secondary">
                      Belge URL'si bulunamadı.
                    </Typography>
                  </Box>
                )}
              </Box>
            )}
          </DialogContent>

          <DialogActions>
            {selectedDocument?.fileUrl && (
              <Button
                onClick={() => window.open(selectedDocument.fileUrl, '_blank')}
                variant="outlined"
                startIcon={<OpenInNew />}
              >
                Yeni Sekmede Aç
              </Button>
            )}
            <Button onClick={() => setDocumentViewModalOpen(false)} variant="outlined">
              Kapat
            </Button>
          </DialogActions>
        </Dialog>
    </Box>
  );
};

export default AdminDashboard;
