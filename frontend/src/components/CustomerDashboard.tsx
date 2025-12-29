import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Container,
  Grid,
  Paper,
  Typography,
  Card,
  CardContent,
  Button,
  IconButton,
  Avatar,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Badge,
  useTheme,
  useMediaQuery,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Divider,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
  Stepper,
  Step,
  StepLabel,
  Radio,
  RadioGroup,
  FormControlLabel,
  FormLabel,
  Alert,
  Tabs,
  Tab,
  Tooltip,
} from '@mui/material';
import {
  Person,
  Assignment,
  Description,
  Payment,
  Notifications,
  Add,
  Visibility,
  Edit,
  Delete,
  Print,
  Phone,
  Email,
  LocationOn,
  TrendingUp,
  TrendingDown,
  Logout,
  Shield,
  Support,
  DirectionsCar,
  CheckCircle,
  Home,
  LocalHospital,
  Business,
  Image,
  Flight,
  CameraAlt,
  Close,
  Check,
  Error,
  Send,
  Warning,
  Info
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import Header from './Header';
import { apiService } from '../services/api';
import { Customer } from '../types';

// Hizmet kategorileri - 6 Ana Departman
const insuranceServices = [
  {
    id: 'konut',
    name: 'Konut Sigortası',
    description: 'Konutunuz ve eşyalarınız için kapsamlı koruma',
    icon: <Home sx={{ fontSize: 40, color: '#4caf50' }} />,
    features: ['Yangın koruması', 'Hırsızlık koruması', 'Doğal afet koruması'],
    basePrice: 1200
  },
  {
    id: 'seyahat',
    name: 'Seyahat Sigortası',
    description: 'Seyahat sırasında oluşabilecek risklere karşı koruma',
    icon: <Flight sx={{ fontSize: 40, color: '#00bcd4' }} />,
    features: ['Sağlık koruması', 'Bagaj koruması', 'İptal koruması'],
    basePrice: 300
  },
  {
    id: 'isyeri',
    name: 'İş Yeri Sigortası',
    description: 'İş yeriniz için profesyonel koruma',
    icon: <Business sx={{ fontSize: 40, color: '#ff9800' }} />,
    features: ['İş kazası koruması', 'Mal koruması', 'Sorumluluk koruması'],
    basePrice: 2000
  },
  {
    id: 'trafik',
    name: 'Trafik Sigortası',
    description: 'Araç kullanımı sırasında oluşabilecek risklere karşı koruma',
    icon: <DirectionsCar sx={{ fontSize: 40, color: '#2196f3' }} />,
    features: ['Kaza koruması', 'Hasar tazminatı', 'Yasal sorumluluk'],
    basePrice: 800
  },
  {
    id: 'saglik',
    name: 'Sağlık Sigortası',
    description: 'Sağlık harcamalarınız için güvenli koruma',
    icon: <LocalHospital sx={{ fontSize: 40, color: '#f44336' }} />,
    features: ['Hastane masrafları', 'İlaç giderleri', 'Doktor muayeneleri'],
    basePrice: 3000
  },
  {
    id: 'hayat',
    name: 'Hayat Sigortası',
    description: 'Geleceğiniz ve aileniz için güvenli koruma',
    icon: <Shield sx={{ fontSize: 40, color: '#9c27b0' }} />,
    features: ['Hayat koruması', 'Maluliyet koruması', 'Kritik hastalık koruması'],
    basePrice: 5000
  }
];

const CustomerDashboard: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const navigate = useNavigate();
  const { user, token } = useAuth();

  // State for real data
  const [customerData, setCustomerData] = useState<any>(null);
  const [policies, setPolicies] = useState<any[]>([]);
  const [claims, setClaims] = useState<any[]>([]);
  const [offers, setOffers] = useState<any[]>([]); // Teklif talepleri için
  const [documents, setDocuments] = useState<any[]>([]); // Dokümanlar için
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Tab state
  const [activeTab, setActiveTab] = useState(0);
  const [offerTabValue, setOfferTabValue] = useState(0);

  // State for modals
  const [servicesModalOpen, setServicesModalOpen] = useState(false);
  const [offerModalOpen, setOfferModalOpen] = useState(false);
  const [selectedService, setSelectedService] = useState<any>(null);
  
  // View offer modal states
  const [viewModalOpen, setViewModalOpen] = useState(false);
  const [selectedViewOffer, setSelectedViewOffer] = useState<any>(null);
  
  // Policy PDF viewer states
  const [policyPdfModalOpen, setPolicyPdfModalOpen] = useState(false);
  const [selectedPolicy, setSelectedPolicy] = useState<any>(null);
  const [policyPdfUrl, setPolicyPdfUrl] = useState<string>('');
  
  // Edit claim modal states
  const [editClaimModalOpen, setEditClaimModalOpen] = useState(false);
  const [selectedClaim, setSelectedClaim] = useState<any>(null);
  const [editClaimData, setEditClaimData] = useState({
    description: ''
  });
  const [claimDocuments, setClaimDocuments] = useState<any[]>([]);
  const [newClaimFiles, setNewClaimFiles] = useState<File[]>([]);
  
  // PDF Modal states for offer documents
  const [isPdfModalOpen, setIsPdfModalOpen] = useState(false);
  const [pdfUrl, setPdfUrl] = useState<string>('');
  const [pdfTitle, setPdfTitle] = useState<string>('');

  // Document Details Modal states
  const [documentDetailsModalOpen, setDocumentDetailsModalOpen] = useState(false);
  const [selectedDocument, setSelectedDocument] = useState<any>(null);
  
  // Track changes for save button
  const [offerChanges, setOfferChanges] = useState<{ [offerId: number]: boolean }>({});
  // Track dropdown selections locally
  const [dropdownSelections, setDropdownSelections] = useState<{ [offerId: number]: string }>({});
  
  // Payment modal states
  const [paymentModalOpen, setPaymentModalOpen] = useState(false);
  const [selectedPaymentOffer, setSelectedPaymentOffer] = useState<any>(null);
  const [paymentData, setPaymentData] = useState({
    cardNumber: '',
    expiryMonth: '',
    expiryYear: '',
    cvv: '',
    cardName: '',
    email: ''
  });
  const [paymentLoading, setPaymentLoading] = useState(false);
  const [paymentSuccess, setPaymentSuccess] = useState(false);
  const [receiptData, setReceiptData] = useState<any>(null);
  // Form state
  const [offerFormData, setOfferFormData] = useState({
    insuranceTypeId: '',
    finalPrice: '',
    status: 'Pending',
    department: '',
    coverageAmount: '',
    requestedStartDate: ''
  });

  // State for dynamic form fields
  const [dynamicFormData, setDynamicFormData] = useState<{ [key: string]: any }>({});
  const [offerStep, setOfferStep] = useState(0);
  const [offerLoading, setOfferLoading] = useState(false);
  const [offerSuccess, setOfferSuccess] = useState(false);
  
  // Camera modal states
  const [isCameraModalOpen, setIsCameraModalOpen] = useState(false);
  const [cameraStream, setCameraStream] = useState<MediaStream | null>(null);
  const [cameraField, setCameraField] = useState<string>('');
  
  // Profile modal states
  const [profileModalOpen, setProfileModalOpen] = useState(false);
  const [profileData, setProfileData] = useState({
    name: '',
    email: '',
    phone: '',
    address: '',
    password: '',
    confirmPassword: ''
  });

  // Olay Formu states
  const [incidentModalOpen, setIncidentModalOpen] = useState(false);
  const [policySelectionModalOpen, setPolicySelectionModalOpen] = useState(false);
  const [selectedPolicyForIncident, setSelectedPolicyForIncident] = useState<any>(null);
  const [customerInfo, setCustomerInfo] = useState<any>(null);
  const [incidentFormData, setIncidentFormData] = useState({
    policyId: '',
    incidentType: '',
    description: '',
    incidentDate: '',
    contactName: '',
    contactPhone: '',
    contactEmail: '',
    contactAddress: '',
    // Araç sigortası için ek alanlar
    vehiclePlate: '',
    vehicleBrand: '',
    vehicleModel: '',
    vehicleYear: '',
    accidentLocation: '',
    accidentTime: '',
    // Konut sigortası için ek alanlar
    propertyAddress: '',
    propertyFloor: '',
    propertyType: '',
    // Sağlık sigortası için ek alanlar
    patientName: '',
    patientAge: '',
    patientGender: '',
    patientTc: '',
    hospitalName: '',
    doctorName: '',
    // Seyahat sigortası için ek alanlar
    travelStartDate: '',
    travelEndDate: '',
    travelCountry: '',
    travelPurpose: '',
    // Hayat sigortası için ek alanlar
    insuredName: '',
    insuredAge: '',
    insuredGender: '',
    insuredTc: '',
    // İş yeri sigortası için ek alanlar
    workplaceName: '',
    workplaceAddress: '',
    employeeCount: '',
    businessSector: '',
    sgkNumber: '',
    safetyOfficer: '',
    emergencyContact: ''
  });
  const [uploadedFiles, setUploadedFiles] = useState<File[]>([]);
  const [incidentStep, setIncidentStep] = useState(0);
  const [incidentLoading, setIncidentLoading] = useState(false);
  const [incidentSuccess, setIncidentSuccess] = useState(false);

  // Poliçe türü değiştiğinde olay türünü sıfırla
  useEffect(() => {
    if (selectedPolicyForIncident?.insuranceType?.name) {
      setIncidentFormData(prev => ({
        ...prev,
        incidentType: '' // Olay türünü sıfırla
      }));
    }
  }, [selectedPolicyForIncident?.policyId]);

  const handleIncidentTypeChange = (event: any) => {
    const newValue = event.target.value;
    setIncidentFormData(prev => ({
      ...prev,
      incidentType: newValue
    }));
  };



  const stats = [
    { 
      title: 'Aktif Poliçeler', 
      value: policies.length.toString(), 
      change: policies.length > 0 ? `+${policies.length}` : '0', 
      trend: policies.length > 0 ? 'up' : 'neutral', 
      color: '#4caf50' 
    },
    { 
      title: 'Bekleyen Teklifler', 
      value: offers.filter((o: any) => o.status === 'pending').length.toString(), 
      change: offers.filter((o: any) => o.status === 'pending').length > 0 ? `+${offers.filter((o: any) => o.status === 'pending').length}` : '0', 
      trend: offers.filter((o: any) => o.status === 'pending').length > 0 ? 'up' : 'neutral', 
      color: '#ff9800' 
    },
  ];

  // Use real data for policies, claims, and documents
  const activePolicies = policies.map((policy: any) => ({
    id: policy.id,
    type: policy.insuranceType?.name || 'Bilinmiyor',
    company: policy.insuranceCompany || 'Bilinmiyor',
    amount: `₺${policy.premiumAmount?.toLocaleString() || '0'}`,
    startDate: policy.startDate ? new Date(policy.startDate).toLocaleDateString('tr-TR') : 'Bilinmiyor',
    endDate: policy.endDate ? new Date(policy.endDate).toLocaleDateString('tr-TR') : 'Bilinmiyor',
    status: policy.status || 'Bilinmiyor'
  }));

  const recentClaims = claims.map((claim: any) => ({
    id: claim.id,
    type: claim.insuranceType || 'Bilinmiyor',
    description: claim.description || 'Açıklama yok',
    amount: `₺${claim.claimAmount?.toLocaleString() || '0'}`,
    status: claim.status || 'Bilinmiyor',
    date: claim.createdAt ? new Date(claim.createdAt).toLocaleDateString('tr-TR') : 'Bilinmiyor'
  }));

  // Use real documents data
  const documentsData = documents.map((doc: any) => ({
    id: doc.documentId,
    name: doc.fileName || 'Doküman',
    type: doc.fileType || 'PDF',
    size: doc.fileSize ? `${(doc.fileSize / 1024 / 1024).toFixed(1)} MB` : 'Bilinmiyor',
    date: doc.uploadedAt ? new Date(doc.uploadedAt).toLocaleDateString('tr-TR') : 'Bilinmiyor',
    url: doc.fileUrl || ''
  }));

  const handleLogout = () => {
    navigate('/customer-login');
  };

  // Tab change handler
  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  // Offer tab change handler
  const handleOfferTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setOfferTabValue(newValue);
  };

  // Filter offers based on tab
  const getFilteredOffers = () => {
    if (!offers || offers.length === 0) return [];
    
    // Debug: Tüm tekliflerin status'larını logla
    console.log('🔍 All offers status:', offers.map(offer => ({ id: offer.offerId, status: offer.status, isCustomerApproved: offer.isCustomerApproved })));
    
    if (offerTabValue === 0) {
      // Aktif Taleplerim - ödeme yapılmamış, silinebilir (isCustomerApproved = false)
      const activeOffers = offers.filter(offer => 
        !offer.isCustomerApproved && 
        offer.status !== 'paid' && 
        offer.status !== 'active' &&
        offer.status !== 'cancelled'
      );
      console.log('📋 Active offers:', activeOffers.map(offer => ({ id: offer.offerId, status: offer.status, isCustomerApproved: offer.isCustomerApproved })));
      return activeOffers;
    } else {
      // Onaylanmış Taleplerim - müşteri tarafından onaylanmış ve ödeme yapılmış (isCustomerApproved = true)
      const approvedOffers = offers.filter(offer => offer.isCustomerApproved === true);
      console.log('✅ Approved offers:', approvedOffers.map(offer => ({ id: offer.offerId, status: offer.status, isCustomerApproved: offer.isCustomerApproved })));
      return approvedOffers;
    }
  };

  // Offer action handlers
  const handleOfferApproval = (offerId: number, value: string) => {
    try {
      // Update dropdown selection
      setDropdownSelections(prev => ({
        ...prev,
        [offerId]: value
      }));
      
      // Mark this offer as having changes
      setOfferChanges(prev => ({
        ...prev,
        [offerId]: true
      }));
    } catch (error) {
      console.error('Error updating offer approval:', error);
    }
  };

  const handleDeleteOffer = async (offerId: number) => {
    if (window.confirm('Bu teklifi silmek istediğinizden emin misiniz?')) {
      try {
        await apiService.deleteOffer(offerId);
        // Refresh offers data
        const customer = customerData;
        if (customer) {
          const offersData = await apiService.getOffersByCustomer(customer.customerId);
          setOffers(offersData);
        }
      } catch (error) {
        console.error('Error deleting offer:', error);
      }
    }
  };

  const handleViewOffer = (offer: any) => {
    console.log('🔍 CustomerDashboard: Viewing Offer:', offer);
    setSelectedViewOffer(offer);
    setViewModalOpen(true);
  };

  const handleViewPolicyPdf = async (policy: any) => {
    try {
      console.log('📄 CustomerDashboard: Viewing Policy PDF:', policy);
      setSelectedPolicy(policy);
      
      // Önce teklif bilgilerini al ve PDF URL'ini kontrol et
      const offer = offers.find(o => o.offerId === policy.offerId);
      if (offer && offer.policyPdfUrl) {
        // Yüklenen PDF'i göster
        const pdfUrl = `http://localhost:5000${offer.policyPdfUrl}`;
        console.log('🔍 Policy PDF URL from offer:', pdfUrl);
        setPolicyPdfUrl(pdfUrl);
      } else {
        // Otomatik oluşturulan PDF'i göster (eski yöntem)
        const policyDate = new Date(policy.createdAt || policy.startDate || Date.now());
        const pdfFileName = `Poliçe_${policy.policyNumber}_${policyDate.toISOString().split('T')[0].replace(/-/g, '')}.pdf`;
        const pdfUrl = `http://localhost:5000/api/Document/serve/documents/pdfs/${pdfFileName}`;
        console.log('🔍 Policy PDF URL (auto-generated):', pdfUrl);
        setPolicyPdfUrl(pdfUrl);
      }
      console.log('🔍 Policy data:', policy);
      setPolicyPdfModalOpen(true);
    } catch (error) {
      console.error('❌ Error opening policy PDF:', error);
      alert('Poliçe PDF\'i açılırken bir hata oluştu!');
    }
  };

  const handleDownloadPolicyPdf = () => {
    if (selectedPolicy && policyPdfUrl) {
      const link = document.createElement('a');
      link.href = policyPdfUrl;
      link.download = `Poliçe_${selectedPolicy.policyNumber}.pdf`;
      link.target = '_blank';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      console.log('✅ Policy PDF downloaded:', policyPdfUrl);
    }
  };

  const handleViewDocument = (document: any) => {
    console.log('📄 CustomerDashboard: Viewing Document:', document);
    setSelectedDocument(document);
    setDocumentDetailsModalOpen(true);
  };

  const handleDocumentDownload = async (documentId: number) => {
    try {
      console.log('📄 CustomerDashboard: Downloading document:', documentId);
      
      // Gerçek doküman verilerini kullan
      const doc = documents.find(d => d.documentId === documentId);
      if (doc && doc.fileUrl) {
        // URL'yi düzelt - backend'deki Document/serve endpoint'ini kullan
        let downloadUrl;
        if (doc.fileUrl.startsWith('http')) {
          downloadUrl = doc.fileUrl;
        } else if (doc.fileUrl.startsWith('/uploads/')) {
          downloadUrl = `http://localhost:5000/api/Document/serve/${doc.fileUrl.substring(1)}`;
        } else {
          downloadUrl = `http://localhost:5000/api/Document/serve/${doc.fileUrl}`;
        }
        
        console.log('🔍 Download URL:', downloadUrl);
        
        const link = document.createElement('a');
        link.href = downloadUrl;
        link.download = doc.fileName || 'document';
        link.target = '_blank';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        console.log('✅ Document downloaded:', doc.fileName);
      } else {
        console.error('❌ Document not found:', { documentId, doc, fileUrl: doc?.fileUrl });
        alert('Doküman bulunamadı veya URL mevcut değil!');
      }
    } catch (error) {
      console.error('❌ Error downloading document:', error);
      alert('Doküman indirilirken bir hata oluştu!');
    }
  };

  const handlePaymentOffer = (offer: any) => {
    setSelectedPaymentOffer(offer);
    setPaymentModalOpen(true);
    setPaymentSuccess(false);
    setReceiptData(null);
  };

  const handlePaymentSubmit = async () => {
    if (!selectedPaymentOffer) return;
    
    setPaymentLoading(true);
    
    try {
      // Simulate payment processing
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      // Generate receipt data
      const receipt = {
        receiptNumber: `RCP-${Date.now()}`,
        paymentDate: new Date().toLocaleString('tr-TR'),
        offerId: selectedPaymentOffer.offerId,
        customerName: selectedPaymentOffer.customerName,
        customerEmail: selectedPaymentOffer.customerEmail,
        insuranceType: selectedPaymentOffer.insuranceTypeName,
        department: selectedPaymentOffer.department,
        basePrice: selectedPaymentOffer.basePrice,
        discountRate: selectedPaymentOffer.discountRate,
        finalPrice: selectedPaymentOffer.finalPrice,
        coverageAmount: selectedPaymentOffer.coverageAmount,
        startDate: selectedPaymentOffer.requestedStartDate,
        validUntil: selectedPaymentOffer.validUntil,
        paymentMethod: 'Kredi Kartı',
        cardLast4: paymentData.cardNumber.slice(-4),
        transactionId: `TXN-${Date.now()}`,
        paymentAmount: selectedPaymentOffer.finalPrice,
        cardName: paymentData.cardName,
        email: paymentData.email,
        status: 'Başarılı',
        pdfUrl: '' as string
      };
      
      setReceiptData(receipt);
      setPaymentSuccess(true);
      
      // Update offer approval status
      await apiService.updateOfferApproval(selectedPaymentOffer.offerId, true);
      
      // Create policy and document from payment
      const policyData = await apiService.createPolicyFromPayment(selectedPaymentOffer.offerId, {
        paymentAmount: selectedPaymentOffer.finalPrice,
        paymentMethod: 'Kredi Kartı',
        transactionId: receipt.transactionId,
        cardLast4: receipt.cardLast4
      });
      
      console.log('✅ Policy and document created:', policyData);
      
      
      // Refresh offers and policies data
      const customer = customerData;
      if (customer) {
        const offersData = await apiService.getOffersByCustomer(customer.customerId);
        setOffers(offersData);
        
        // Refresh policies data
        const policiesData = await apiService.getMyPolicies();
        setPolicies(policiesData);
        console.log('✅ Policies refreshed after payment:', policiesData);
      }
      
    } catch (error) {
      console.error('Payment error:', error);
      alert('Ödeme işlemi sırasında bir hata oluştu!');
    } finally {
      setPaymentLoading(false);
    }
  };

  const handlePrintReceipt = () => {
    if (!receiptData) return;
    
    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(`
        <html>
          <head>
            <title>Sigorta Makbuzu - ${receiptData.receiptNumber}</title>
            <style>
              body { font-family: Arial, sans-serif; margin: 20px; }
              .header { text-align: center; border-bottom: 2px solid #333; padding-bottom: 10px; }
              .section { margin: 20px 0; }
              .section h3 { color: #2563eb; border-bottom: 1px solid #ddd; padding-bottom: 5px; }
              .info-row { display: flex; justify-content: space-between; margin: 5px 0; }
              .total { font-weight: bold; font-size: 18px; color: #10b981; }
              .footer { text-align: center; margin-top: 30px; font-size: 12px; color: #666; }
            </style>
          </head>
          <body>
            <div class="header">
              <h1>Sigorta Makbuzu</h1>
              <p>Makbuz No: ${receiptData.receiptNumber}</p>
              <p>İşlem Tarihi: ${receiptData.paymentDate}</p>
            </div>
            
            <div class="section">
              <h3>Müşteri Bilgileri</h3>
              <div class="info-row"><span>Ad Soyad:</span><span>${receiptData.customerName}</span></div>
              <div class="info-row"><span>E-posta:</span><span>${receiptData.customerEmail}</span></div>
            </div>
            
            <div class="section">
              <h3>Sigorta Bilgileri</h3>
              <div class="info-row"><span>Teklif No:</span><span>#${receiptData.offerId}</span></div>
              <div class="info-row"><span>Sigorta Türü:</span><span>${receiptData.insuranceType}</span></div>
              <div class="info-row"><span>Departman:</span><span>${receiptData.department}</span></div>
              <div class="info-row"><span>Teminat Tutarı:</span><span>₺${receiptData.coverageAmount}</span></div>
              <div class="info-row"><span>Başlangıç Tarihi:</span><span>${new Date(receiptData.startDate).toLocaleDateString('tr-TR')}</span></div>
              <div class="info-row"><span>Geçerlilik:</span><span>${new Date(receiptData.validUntil).toLocaleDateString('tr-TR')}</span></div>
            </div>
            
            <div class="section">
              <h3>Ödeme Bilgileri</h3>
              <div class="info-row"><span>Temel Fiyat:</span><span>₺${receiptData.basePrice}</span></div>
              <div class="info-row"><span>İndirim Oranı:</span><span>%${receiptData.discountRate}</span></div>
              <div class="info-row total"><span>Final Fiyat:</span><span>₺${receiptData.finalPrice}</span></div>
              <div class="info-row"><span>Ödeme Yöntemi:</span><span>${receiptData.paymentMethod}</span></div>
              <div class="info-row"><span>Kart Son 4 Hanesi:</span><span>****${receiptData.cardLast4}</span></div>
              <div class="info-row"><span>İşlem No:</span><span>${receiptData.transactionId}</span></div>
            </div>
            
            <div class="footer">
              <p>Bu makbuz elektronik ortamda oluşturulmuştur.</p>
              <p>Sigorta şirketi tarafından düzenlenmiştir.</p>
            </div>
          </body>
        </html>
      `);
      printWindow.document.close();
      printWindow.print();
    }
  };


  // Kart numarası formatlaması
  const formatCardNumber = (value: string) => {
    // Sadece rakamları al
    const numbers = value.replace(/\D/g, '');
    // Her 4 rakamdan sonra boşluk ekle
    return numbers.replace(/(\d{4})(?=\d)/g, '$1 ');
  };

  const handleCardNumberChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const formatted = formatCardNumber(e.target.value);
    setPaymentData(prev => ({ ...prev, cardNumber: formatted }));
  };

  const handleNewClaim = () => {
    setServicesModalOpen(true);
  };

  const handlePolicyView = (policyId: number) => {
    console.log('Poliçe görüntüle:', policyId);
  };


  const handleContactSupport = () => {
    // Aktif poliçeleri kontrol et
    const activePolicies = policies.filter(policy => {
      const endDate = new Date(policy.endDate);
      const today = new Date();
      return endDate >= today;
    });

    if (activePolicies.length === 0) {
      alert('Olay bildirimi için aktif poliçeniz bulunmamaktadır.');
      return;
    }

    // Poliçe seçim modal'ını aç
    setPolicySelectionModalOpen(true);
  };

  const handleProfileEdit = () => {
    // Mevcut profil bilgilerini yükle
    setProfileData({
      name: customerInfo.name,
      email: customerInfo.email,
      phone: customerInfo.phone,
      address: customerInfo.address,
      password: '',
      confirmPassword: ''
    });
    setProfileModalOpen(true);
  };

  const handleProfileUpdate = async () => {
    try {
      setLoading(true);
      
      // Şifre kontrolü
      if (profileData.password && profileData.password !== profileData.confirmPassword) {
        setError('Şifreler eşleşmiyor');
        return;
      }
      
      // API çağrısı yapılacak (şimdilik mock)
      console.log('Profil güncelleniyor:', profileData);
      
      // Başarılı güncelleme sonrası
      setProfileModalOpen(false);
      setError(null);
      
      // Customer data'yı güncelle
      if (customerData) {
        const updatedCustomer = {
          ...customerData,
          user: {
            ...customerData.user,
            name: profileData.name,
            email: profileData.email,
            phone: profileData.phone,
            address: profileData.address
          }
        };
        setCustomerData(updatedCustomer);
      }
      
    } catch (error) {
      console.error('Profil güncelleme hatası:', error);
      setError('Profil güncellenirken hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  const handleOfferView = (offerId: number) => {
    console.log('Teklif görüntüle:', offerId);
    // Burada teklif detay modal'ı açılacak
  };

  // Hizmet seçimi
  const handleServiceSelect = (service: any) => {
    setSelectedService(service);
    setOfferFormData(prev => ({
      ...prev,
      insuranceTypeId: getInsuranceTypeId(service.id).toString(),
      department: service.name,
      finalPrice: '' // Temel fiyat artık müşteri tarafından belirlenmez
    }));
    setServicesModalOpen(false);
    setOfferModalOpen(true);
  };

  // Teklif form değişiklikleri
  const handleOfferFormChange = (field: string, value: string) => {
    setOfferFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  // Teklif gönderme
  const handleOfferSubmit = async () => {
    try {
      setOfferLoading(true);
      
      // Dinamik form verilerini CustomerAdditionalInfo'ya ekle
      const additionalInfo = {
        ...dynamicFormData,
        insuranceType: offerFormData.insuranceTypeId
      };
      
      const offerData = {
        customerId: customerData?.customerId,
        insuranceTypeId: parseInt(offerFormData.insuranceTypeId),
        basePrice: 0, // Müşteri için minimum fiyat 0 TL
        discountRate: 0, // Müşteri indirim belirleyemez, default 0
        finalPrice: 0, // Final fiyat da 0 TL (acente/admin belirleyecek)
        status: offerFormData.status,
        department: offerFormData.department,
        coverageAmount: parseFloat(offerFormData.coverageAmount),
        requestedStartDate: offerFormData.requestedStartDate,
        customerAdditionalInfo: JSON.stringify(additionalInfo) // Dinamik verileri JSON olarak gönder
      };
      
      console.log('Teklif verileri:', JSON.stringify(offerData, null, 2));
      
      const response = await apiService.createOffer(offerData);
      
      if (response) {
        console.log('Teklif oluşturma başarılı:', response);
        setOfferSuccess(true);
        setOfferStep(2);
        
        // 3 saniye sonra modal'ı kapat ve form'u sıfırla
      setTimeout(() => {
          handleCloseOfferModal();
          setOfferStep(0);
          setOfferSuccess(false);
          setOfferFormData({
            insuranceTypeId: '',
            finalPrice: '',
            status: 'Pending',
            department: '',
          coverageAmount: '',
            requestedStartDate: ''
        });
          setDynamicFormData({});
      }, 3000);
      }
    } catch (error) {
      console.error('Teklif oluşturma hatası:', error);
    } finally {
      setOfferLoading(false);
    }
  };

  // Service ID'den InsuranceType ID'ye çeviren yardımcı fonksiyon
  const getInsuranceTypeId = (serviceId: string): number => {
    const serviceMap: { [key: string]: number } = {
      'konut': 1,      // Konut Sigortası
      'seyahat': 2,    // Seyahat Sigortası
      'isyeri': 3,     // İş Yeri Sigortası
      'trafik': 4,     // Trafik Sigortası
      'saglik': 5,     // Sağlık Sigortası
      'hayat': 6       // Hayat Sigortası
    };
    return serviceMap[serviceId] || 1; // Varsayılan olarak Konut Sigortası
  };

  // InsuranceType ID'den isim çeviren yardımcı fonksiyon
  const getInsuranceTypeName = (insuranceTypeId: number): string => {
    switch (insuranceTypeId) {
      case 1: return 'Konut Sigortası';
      case 2: return 'Seyahat Sigortası';
      case 3: return 'İş Yeri Sigortası';
      case 4: return 'Trafik Sigortası';
      case 5: return 'Sağlık Sigortası';
      case 6: return 'Hayat Sigortası';
      default: return 'Bilinmeyen Sigorta Türü';
    }
  };

  // Offer'dan sigorta türü ismini alan yardımcı fonksiyon
  const getOfferInsuranceTypeName = (offer: any): string => {
    if (offer?.insuranceType?.name) {
      return offer.insuranceType.name;
    }
    if (offer?.insuranceTypeId) {
      return getInsuranceTypeName(offer.insuranceTypeId);
    }
    return 'Bilinmeyen Sigorta Türü';
  };

  // Sigorta türüne göre form alanlarını döndüren fonksiyon
  const getFormFieldsByInsuranceType = (insuranceTypeId: string) => {
    const fields: { [key: string]: any } = {};
    
    switch (insuranceTypeId) {
      case '1': // Konut Sigortası
        fields.address = '';
        fields.propertyType = '';
        fields.deedDocument = ''; // Tapu belgesi PDF
        break;
      case '2': // Seyahat Sigortası
        fields.destination = '';
        fields.travelDuration = '';
        fields.travelPurpose = '';
        fields.healthReport = ''; // Sağlık raporu PDF
        break;
      case '3': // İş Yeri Sigortası
        fields.businessType = '';
        fields.employeeCount = '';
        fields.annualRevenueReport = ''; // Yıllık ciro PDF
        fields.riskReport = ''; // Risk raporu PDF
        break;
      case '4': // Trafik Sigortası
        fields.vehicleType = '';
        fields.vehicleAge = '';
        fields.driverAge = '';
        fields.claimHistory = '';
        break;
      case '5': // Sağlık Sigortası
        fields.age = '';
        fields.medicalHistoryReport = ''; // Tıbbi geçmiş PDF
        fields.familyHistoryReport = ''; // Aile geçmişi PDF
        fields.coverageType = '';
        break;
      case '6': // Hayat Sigortası
        fields.age = '';
        fields.occupation = '';
        fields.idFrontPhoto = ''; // Kimlik ön yüz fotoğrafı
        fields.idBackPhoto = ''; // Kimlik arka yüz fotoğrafı
        break;
      default:
        break;
    }
    
    return fields;
  };


  // Dinamik form alanlarını render eden fonksiyon
  const renderDynamicFormFields = () => {
    if (!offerFormData.insuranceTypeId) return null;
    
    const fields = getFormFieldsByInsuranceType(offerFormData.insuranceTypeId);
    
    // Tüm sigorta türleri için özel label'lar
    const getFieldLabel = (key: string) => {
      // Seyahat Sigortası (ID: 2)
      if (offerFormData.insuranceTypeId === '2') {
        switch (key) {
          case 'destination': return 'Seyahat Destinasyonu';
          case 'travelDuration': return 'Seyahat Süresi (gün)';
          case 'travelPurpose': return 'Seyahat Amacı';
          case 'healthReport': return 'Sağlık Raporu';
          default: return key.charAt(0).toUpperCase() + key.slice(1).replace(/([A-Z])/g, ' $1');
        }
      }
      
      // İş Yeri Sigortası (ID: 3)
      if (offerFormData.insuranceTypeId === '3') {
        switch (key) {
          case 'businessType': return 'İşletme Türü';
          case 'employeeCount': return 'Çalışan Sayısı';
          case 'annualRevenueReport': return 'Yıllık Ciro Raporu';
          case 'riskReport': return 'Risk Raporu';
          default: return key.charAt(0).toUpperCase() + key.slice(1).replace(/([A-Z])/g, ' $1');
        }
      }
      
      // Trafik Sigortası (ID: 4)
      if (offerFormData.insuranceTypeId === '4') {
        switch (key) {
          case 'vehicleType': return 'Araç Türü';
          case 'vehicleAge': return 'Araç Yaşı';
          case 'driverAge': return 'Sürücü Yaşı';
          case 'claimHistory': return 'Kaza Geçmişi';
          default: return key.charAt(0).toUpperCase() + key.slice(1).replace(/([A-Z])/g, ' $1');
        }
      }
      
      // Sağlık Sigortası (ID: 5)
      if (offerFormData.insuranceTypeId === '5') {
        switch (key) {
          case 'age': return 'Yaş';
          case 'medicalHistoryReport': return 'Tıbbi Geçmiş Raporu';
          case 'familyHistoryReport': return 'Aile Geçmişi Raporu';
          case 'coverageType': return 'Kapsam Türü';
          default: return key.charAt(0).toUpperCase() + key.slice(1).replace(/([A-Z])/g, ' $1');
        }
      }
      
      // Hayat Sigortası (ID: 6)
      if (offerFormData.insuranceTypeId === '6') {
        switch (key) {
          case 'age': return 'Yaş';
          case 'occupation': return 'Meslek';
          case 'idFrontPhoto': return 'Kimlik Ön Yüz Fotoğrafı';
          case 'idBackPhoto': return 'Kimlik Arka Yüz Fotoğrafı';
          default: return key.charAt(0).toUpperCase() + key.slice(1).replace(/([A-Z])/g, ' $1');
        }
      }
      
      // Konut Sigortası (ID: 1)
      if (offerFormData.insuranceTypeId === '1') {
        switch (key) {
          case 'deedDocument': return 'Tapu Belgesi';
          default: return key.charAt(0).toUpperCase() + key.slice(1).replace(/([A-Z])/g, ' $1');
        }
      }
      
      return key.charAt(0).toUpperCase() + key.slice(1).replace(/([A-Z])/g, ' $1');
    };
    
    return Object.entries(fields).map(([key, value]) => {
      // Seyahat amacı için özel select dropdown
      if (offerFormData.insuranceTypeId === '2' && key === 'travelPurpose') {
        return (
          <FormControl key={key} fullWidth required>
            <InputLabel>{getFieldLabel(key)}</InputLabel>
            <Select
              value={dynamicFormData[key] || ''}
              onChange={(e) => setDynamicFormData(prev => ({
                ...prev,
                [key]: e.target.value
              }))}
              label={getFieldLabel(key)}
            >
              <MenuItem key="business" value="business">İş Seyahati</MenuItem>
              <MenuItem key="leisure" value="leisure">Tatil</MenuItem>
              <MenuItem key="education" value="education">Eğitim</MenuItem>
              <MenuItem key="medical" value="medical">Sağlık</MenuItem>
              <MenuItem key="family" value="family">Aile Ziyareti</MenuItem>
              <MenuItem key="other" value="other">Diğer</MenuItem>
            </Select>
          </FormControl>
        );
      }
      
      // İşletme türü için özel select dropdown
      if (offerFormData.insuranceTypeId === '3' && key === 'businessType') {
        return (
          <FormControl key={key} fullWidth required>
            <InputLabel>{getFieldLabel(key)}</InputLabel>
            <Select
              value={dynamicFormData[key] || ''}
              onChange={(e) => setDynamicFormData(prev => ({
                ...prev,
                [key]: e.target.value
              }))}
              label={getFieldLabel(key)}
            >
              <MenuItem key="retail" value="retail">Perakende</MenuItem>
              <MenuItem key="manufacturing" value="manufacturing">Üretim</MenuItem>
              <MenuItem key="service" value="service">Hizmet</MenuItem>
              <MenuItem key="office" value="office">Ofis</MenuItem>
              <MenuItem key="restaurant" value="restaurant">Restoran</MenuItem>
              <MenuItem key="other2" value="other">Diğer</MenuItem>
            </Select>
          </FormControl>
        );
      }
      
      // Araç türü için özel select dropdown
      if (offerFormData.insuranceTypeId === '4' && key === 'vehicleType') {
        return (
          <FormControl key={key} fullWidth required>
            <InputLabel>{getFieldLabel(key)}</InputLabel>
            <Select
              value={dynamicFormData[key] || ''}
              onChange={(e) => setDynamicFormData(prev => ({
                ...prev,
                [key]: e.target.value
              }))}
              label={getFieldLabel(key)}
            >
              <MenuItem key="car" value="car">Otomobil</MenuItem>
              <MenuItem key="suv" value="suv">SUV</MenuItem>
              <MenuItem key="truck" value="truck">Kamyon</MenuItem>
              <MenuItem key="motorcycle" value="motorcycle">Motosiklet</MenuItem>
              <MenuItem key="van" value="van">Minibüs</MenuItem>
            </Select>
          </FormControl>
        );
      }
      
      // Kaza geçmişi için PDF yükleme alanı
      if (offerFormData.insuranceTypeId === '4' && key === 'claimHistory') {
        return (
          <Box key={key} sx={{ width: '100%' }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              {getFieldLabel(key)} (PDF Dosyası)
            </Typography>
            <Box
              sx={{
                border: '2px dashed #ccc',
                borderRadius: 2,
                p: 3,
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.3s',
                '&:hover': {
                  borderColor: theme.palette.primary.main,
                },
                backgroundColor: dynamicFormData[key] ? '#e8f5e8' : '#fafafa'
              }}
              onClick={() => document.getElementById('pdf-upload-input')?.click()}
            >
              {dynamicFormData[key] ? (
                <Box>
                  <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                    ✅ PDF Dosyası Yüklendi
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {dynamicFormData[key]}
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      color="error"
                      onClick={(e) => {
                        e.stopPropagation();
                        setDynamicFormData(prev => ({ ...prev, [key]: '' }));
                      }}
                    >
                      Dosyayı Kaldır
                    </Button>
                  </Box>
                </Box>
              ) : (
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    Kaza geçmişinizi gösteren PDF dosyasını yükleyin
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Maksimum 10MB, sadece PDF formatı
                  </Typography>
                </Box>
              )}
            </Box>
            <input
              id="pdf-upload-input"
              type="file"
              accept=".pdf"
              style={{ display: 'none' }}
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (file) {
                  if (file.type !== 'application/pdf') {
                    alert('Lütfen sadece PDF dosyası seçin.');
                    return;
                  }
                  if (file.size > 10 * 1024 * 1024) {
                    alert('Dosya boyutu 10MB\'dan küçük olmalıdır.');
                    return;
                  }
                  
                  try {
                    // PDF dosyasını backend'e yükle
                    const result = await apiService.uploadPdf(file);
                    console.log('✅ PDF uploaded:', result);
                    setDynamicFormData(prev => ({ 
                      ...prev, 
                      [key]: `${file.name} (${result.fileUrl})` 
                    }));
                  } catch (error: any) {
                    console.error('PDF upload error:', error);
                    alert(`PDF yükleme hatası: ${error.message}`);
                  }
                }
              }}
            />
          </Box>
        );
      }
      
      // Tapu belgesi için PDF yükleme alanı
      if (offerFormData.insuranceTypeId === '1' && key === 'deedDocument') {
        return (
          <Box key={key} sx={{ width: '100%' }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              {getFieldLabel(key)} (PDF Dosyası)
            </Typography>
            <Box
              sx={{
                border: '2px dashed #ccc',
                borderRadius: 2,
                p: 3,
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.3s',
                '&:hover': {
                  borderColor: theme.palette.primary.main,
                },
                backgroundColor: dynamicFormData[key] ? '#e8f5e8' : '#fafafa'
              }}
              onClick={() => document.getElementById('tapu-upload-input')?.click()}
            >
              {dynamicFormData[key] ? (
                <Box>
                  <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                    ✅ Tapu Belgesi Yüklendi
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {dynamicFormData[key]}
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      color="error"
                      onClick={(e) => {
                        e.stopPropagation();
                        setDynamicFormData(prev => ({ ...prev, [key]: '' }));
                      }}
                    >
                      Dosyayı Kaldır
                    </Button>
                  </Box>
                </Box>
              ) : (
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    Tapu belgenizi PDF formatında yükleyin
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Maksimum 10MB, sadece PDF formatı
                  </Typography>
                </Box>
              )}
            </Box>
            <input
              id="tapu-upload-input"
              type="file"
              accept=".pdf"
              style={{ display: 'none' }}
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (file) {
                  if (file.type !== 'application/pdf') {
                    alert('Lütfen sadece PDF dosyası seçin.');
                    return;
                  }
                  if (file.size > 10 * 1024 * 1024) {
                    alert('Dosya boyutu 10MB\'dan küçük olmalıdır.');
                    return;
                  }
                  
                  try {
                    // PDF dosyasını backend'e yükle
                    const formData = new FormData();
                    formData.append('file', file);
                    
                    const response = await fetch('http://localhost:5000/api/Document/upload-tapu', {
                      method: 'POST',
                      headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                      },
                      body: formData
                    });
                    
                    if (response.ok) {
                      const result = await response.json();
                      console.log('✅ Tapu PDF uploaded:', result);
                      setDynamicFormData(prev => ({ 
                        ...prev, 
                        [key]: `${file.name} (${result.fileUrl})` 
                      }));
                    } else {
                      const error = await response.json();
                      alert(`Tapu PDF yükleme hatası: ${error.message}`);
                    }
                  } catch (error) {
                    console.error('Tapu PDF upload error:', error);
                    alert('Tapu PDF yüklenirken bir hata oluştu.');
                  }
                }
              }}
            />
          </Box>
        );
      }
      
      // Sağlık raporu için PDF yükleme alanı
      if (offerFormData.insuranceTypeId === '2' && key === 'healthReport') {
        return (
          <Box key={key} sx={{ width: '100%' }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              {getFieldLabel(key)} (PDF Dosyası)
            </Typography>
            <Box
              sx={{
                border: '2px dashed #ccc',
                borderRadius: 2,
                p: 3,
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.3s',
                '&:hover': {
                  borderColor: theme.palette.primary.main,
                },
                backgroundColor: dynamicFormData[key] ? '#e8f5e8' : '#fafafa'
              }}
              onClick={() => document.getElementById('health-report-upload-input')?.click()}
            >
              {dynamicFormData[key] ? (
                <Box>
                  <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                    ✅ Sağlık Raporu Yüklendi
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {dynamicFormData[key]}
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      color="error"
                      onClick={(e) => {
                        e.stopPropagation();
                        setDynamicFormData(prev => ({ ...prev, [key]: '' }));
                      }}
                    >
                      Dosyayı Kaldır
                    </Button>
                  </Box>
                </Box>
              ) : (
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    Sağlık raporunuzu PDF formatında yükleyin
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Maksimum 10MB, sadece PDF formatı
                  </Typography>
                </Box>
              )}
            </Box>
            <input
              id="health-report-upload-input"
              type="file"
              accept=".pdf"
              style={{ display: 'none' }}
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (file) {
                  if (file.type !== 'application/pdf') {
                    alert('Lütfen sadece PDF dosyası seçin.');
                    return;
                  }
                  if (file.size > 10 * 1024 * 1024) {
                    alert('Dosya boyutu 10MB\'dan küçük olmalıdır.');
                    return;
                  }
                  
                  try {
                    // PDF dosyasını backend'e yükle
                    const formData = new FormData();
                    formData.append('file', file);
                    
                    const response = await fetch('http://localhost:5000/api/Document/upload-health-report', {
                      method: 'POST',
                      headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                      },
                      body: formData
                    });
                    
                    if (response.ok) {
                      const result = await response.json();
                      console.log('✅ Health Report PDF uploaded:', result);
                      setDynamicFormData(prev => ({ 
                        ...prev, 
                        [key]: `${file.name} (${result.fileUrl})` 
                      }));
                    } else {
                      const error = await response.json();
                      alert(`Sağlık raporu PDF yükleme hatası: ${error.message}`);
                    }
                  } catch (error) {
                    console.error('Health Report PDF upload error:', error);
                    alert('Sağlık raporu PDF yüklenirken bir hata oluştu.');
                  }
                }
              }}
            />
          </Box>
        );
      }
      
      // Yıllık ciro raporu için PDF yükleme alanı
      if (offerFormData.insuranceTypeId === '3' && key === 'annualRevenueReport') {
        return (
          <Box key={key} sx={{ width: '100%' }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              {getFieldLabel(key)} (PDF Dosyası)
            </Typography>
            <Box
              sx={{
                border: '2px dashed #ccc',
                borderRadius: 2,
                p: 3,
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.3s',
                '&:hover': {
                  borderColor: theme.palette.primary.main,
                },
                backgroundColor: dynamicFormData[key] ? '#e8f5e8' : '#fafafa'
              }}
              onClick={() => document.getElementById('annual-revenue-upload-input')?.click()}
            >
              {dynamicFormData[key] ? (
                <Box>
                  <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                    ✅ Yıllık Ciro Raporu Yüklendi
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {dynamicFormData[key]}
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      color="error"
                      onClick={(e) => {
                        e.stopPropagation();
                        setDynamicFormData(prev => ({ ...prev, [key]: '' }));
                      }}
                    >
                      Dosyayı Kaldır
                    </Button>
                  </Box>
                </Box>
              ) : (
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    Yıllık ciro raporunuzu PDF formatında yükleyin
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Maksimum 10MB, sadece PDF formatı
                  </Typography>
                </Box>
              )}
            </Box>
            <input
              id="annual-revenue-upload-input"
              type="file"
              accept=".pdf"
              style={{ display: 'none' }}
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (file) {
                  if (file.type !== 'application/pdf') {
                    alert('Lütfen sadece PDF dosyası seçin.');
                    return;
                  }
                  if (file.size > 10 * 1024 * 1024) {
                    alert('Dosya boyutu 10MB\'dan küçük olmalıdır.');
                    return;
                  }
                  
                  try {
                    // PDF dosyasını backend'e yükle
                    const formData = new FormData();
                    formData.append('file', file);
                    
                    const response = await fetch('http://localhost:5000/api/Document/upload-annual-revenue', {
                      method: 'POST',
                      headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                      },
                      body: formData
                    });
                    
                    if (response.ok) {
                      const result = await response.json();
                      console.log('✅ Annual Revenue PDF uploaded:', result);
                      setDynamicFormData(prev => ({ 
                        ...prev, 
                        [key]: `${file.name} (${result.fileUrl})` 
                      }));
                    } else {
                      const error = await response.json();
                      alert(`Yıllık ciro raporu PDF yükleme hatası: ${error.message}`);
                    }
                  } catch (error) {
                    console.error('Annual Revenue PDF upload error:', error);
                    alert('Yıllık ciro raporu PDF yüklenirken bir hata oluştu.');
                  }
                }
              }}
            />
          </Box>
        );
      }
      
      // Risk raporu için PDF yükleme alanı
      if (offerFormData.insuranceTypeId === '3' && key === 'riskReport') {
        return (
          <Box key={key} sx={{ width: '100%' }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              {getFieldLabel(key)} (PDF Dosyası)
            </Typography>
            <Box
              sx={{
                border: '2px dashed #ccc',
                borderRadius: 2,
                p: 3,
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.3s',
                '&:hover': {
                  borderColor: theme.palette.primary.main,
                },
                backgroundColor: dynamicFormData[key] ? '#e8f5e8' : '#fafafa'
              }}
              onClick={() => document.getElementById('risk-report-upload-input')?.click()}
            >
              {dynamicFormData[key] ? (
                <Box>
                  <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                    ✅ Risk Raporu Yüklendi
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {dynamicFormData[key]}
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      color="error"
                      onClick={(e) => {
                        e.stopPropagation();
                        setDynamicFormData(prev => ({ ...prev, [key]: '' }));
                      }}
                    >
                      Dosyayı Kaldır
                    </Button>
                  </Box>
                </Box>
              ) : (
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    Risk raporunuzu PDF formatında yükleyin
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Maksimum 10MB, sadece PDF formatı
                  </Typography>
                </Box>
              )}
            </Box>
            <input
              id="risk-report-upload-input"
              type="file"
              accept=".pdf"
              style={{ display: 'none' }}
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (file) {
                  if (file.type !== 'application/pdf') {
                    alert('Lütfen sadece PDF dosyası seçin.');
                    return;
                  }
                  if (file.size > 10 * 1024 * 1024) {
                    alert('Dosya boyutu 10MB\'dan küçük olmalıdır.');
                    return;
                  }
                  
                  try {
                    // PDF dosyasını backend'e yükle
                    const formData = new FormData();
                    formData.append('file', file);
                    
                    const response = await fetch('http://localhost:5001/api/Document/upload-risk-report', {
                      method: 'POST',
                      headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                      },
                      body: formData
                    });
                    
                    if (response.ok) {
                      const result = await response.json();
                      console.log('✅ Risk Report PDF uploaded:', result);
                      setDynamicFormData(prev => ({ 
                        ...prev, 
                        [key]: `${file.name} (${result.fileUrl})` 
                      }));
                    } else {
                      const error = await response.json();
                      alert(`Risk raporu PDF yükleme hatası: ${error.message}`);
                    }
                  } catch (error) {
                    console.error('Risk Report PDF upload error:', error);
                    alert('Risk raporu PDF yüklenirken bir hata oluştu.');
                  }
                }
              }}
            />
          </Box>
        );
      }
      
      // Tıbbi geçmiş raporu için PDF yükleme alanı
      if (offerFormData.insuranceTypeId === '5' && key === 'medicalHistoryReport') {
        return (
          <Box key={key} sx={{ width: '100%' }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              {getFieldLabel(key)} (PDF Dosyası)
            </Typography>
            <Box
              sx={{
                border: '2px dashed #ccc',
                borderRadius: 2,
                p: 3,
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.3s',
                '&:hover': {
                  borderColor: theme.palette.primary.main,
                },
                backgroundColor: dynamicFormData[key] ? '#e8f5e8' : '#fafafa'
              }}
              onClick={() => document.getElementById('medical-history-upload-input')?.click()}
            >
              {dynamicFormData[key] ? (
                <Box>
                  <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                    ✅ Tıbbi Geçmiş Raporu Yüklendi
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {dynamicFormData[key]}
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      color="error"
                      onClick={(e) => {
                        e.stopPropagation();
                        setDynamicFormData(prev => ({ ...prev, [key]: '' }));
                      }}
                    >
                      Dosyayı Kaldır
                    </Button>
                  </Box>
                </Box>
              ) : (
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    Tıbbi geçmiş raporunuzu PDF formatında yükleyin
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Maksimum 10MB, sadece PDF formatı
                  </Typography>
                </Box>
              )}
            </Box>
            <input
              id="medical-history-upload-input"
              type="file"
              accept=".pdf"
              style={{ display: 'none' }}
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (file) {
                  if (file.type !== 'application/pdf') {
                    alert('Lütfen sadece PDF dosyası seçin.');
                    return;
                  }
                  if (file.size > 10 * 1024 * 1024) {
                    alert('Dosya boyutu 10MB\'dan küçük olmalıdır.');
                    return;
                  }
                  
                  try {
                    // PDF dosyasını backend'e yükle
                    const formData = new FormData();
                    formData.append('file', file);
                    
                    const response = await fetch('http://localhost:5001/api/Document/upload-medical-history', {
                      method: 'POST',
                      headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                      },
                      body: formData
                    });
                    
                    if (response.ok) {
                      const result = await response.json();
                      console.log('✅ Medical History PDF uploaded:', result);
                      setDynamicFormData(prev => ({ 
                        ...prev, 
                        [key]: `${file.name} (${result.fileUrl})` 
                      }));
                    } else {
                      const error = await response.json();
                      alert(`Tıbbi geçmiş raporu PDF yükleme hatası: ${error.message}`);
                    }
                  } catch (error) {
                    console.error('Medical History PDF upload error:', error);
                    alert('Tıbbi geçmiş raporu PDF yüklenirken bir hata oluştu.');
                  }
                }
              }}
            />
          </Box>
        );
      }
      
      // Aile geçmişi raporu için PDF yükleme alanı
      if (offerFormData.insuranceTypeId === '5' && key === 'familyHistoryReport') {
        return (
          <Box key={key} sx={{ width: '100%' }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              {getFieldLabel(key)} (PDF Dosyası)
            </Typography>
            <Box
              sx={{
                border: '2px dashed #ccc',
                borderRadius: 2,
                p: 3,
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.3s',
                '&:hover': {
                  borderColor: theme.palette.primary.main,
                },
                backgroundColor: dynamicFormData[key] ? '#e8f5e8' : '#fafafa'
              }}
              onClick={() => document.getElementById('family-history-upload-input')?.click()}
            >
              {dynamicFormData[key] ? (
                <Box>
                  <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                    ✅ Aile Geçmişi Raporu Yüklendi
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {dynamicFormData[key]}
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      color="error"
                      onClick={(e) => {
                        e.stopPropagation();
                        setDynamicFormData(prev => ({ ...prev, [key]: '' }));
                      }}
                    >
                      Dosyayı Kaldır
                    </Button>
                  </Box>
                </Box>
              ) : (
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    Aile geçmişi raporunuzu PDF formatında yükleyin
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Maksimum 10MB, sadece PDF formatı
                  </Typography>
                </Box>
              )}
            </Box>
            <input
              id="family-history-upload-input"
              type="file"
              accept=".pdf"
              style={{ display: 'none' }}
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (file) {
                  if (file.type !== 'application/pdf') {
                    alert('Lütfen sadece PDF dosyası seçin.');
                    return;
                  }
                  if (file.size > 10 * 1024 * 1024) {
                    alert('Dosya boyutu 10MB\'dan küçük olmalıdır.');
                    return;
                  }
                  
                  try {
                    // PDF dosyasını backend'e yükle
                    const formData = new FormData();
                    formData.append('file', file);
                    
                    const response = await fetch('http://localhost:5001/api/Document/upload-family-history', {
                      method: 'POST',
                      headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                      },
                      body: formData
                    });
                    
                    if (response.ok) {
                      const result = await response.json();
                      console.log('✅ Family History PDF uploaded:', result);
                      setDynamicFormData(prev => ({ 
                        ...prev, 
                        [key]: `${file.name} (${result.fileUrl})` 
                      }));
                    } else {
                      const error = await response.json();
                      alert(`Aile geçmişi raporu PDF yükleme hatası: ${error.message}`);
                    }
                  } catch (error) {
                    console.error('Family History PDF upload error:', error);
                    alert('Aile geçmişi raporu PDF yüklenirken bir hata oluştu.');
                  }
                }
              }}
            />
          </Box>
        );
      }
      
      // Kimlik ön yüz fotoğrafı için fotoğraf yükleme alanı
      if (offerFormData.insuranceTypeId === '6' && key === 'idFrontPhoto') {
        return (
          <Box key={key} sx={{ width: '100%' }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              {getFieldLabel(key)}
            </Typography>
            <Box
              sx={{
                border: '2px dashed #ccc',
                borderRadius: 2,
                p: 3,
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.3s',
                '&:hover': {
                  borderColor: theme.palette.primary.main,
                },
                backgroundColor: dynamicFormData[key] ? '#e8f5e8' : '#fafafa'
              }}
              onClick={() => document.getElementById('id-front-photo-input')?.click()}
            >
              {dynamicFormData[key] ? (
                <Box>
                  <img 
                    src={dynamicFormData[key]} 
                    alt="Kimlik Ön Yüz" 
                    style={{ 
                      maxWidth: '100%', 
                      maxHeight: '200px', 
                      borderRadius: '8px',
                      marginBottom: '10px'
                    }} 
                  />
                  <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                    ✅ Kimlik Ön Yüz Fotoğrafı Yüklendi
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      color="error"
                      onClick={(e) => {
                        e.stopPropagation();
                        setDynamicFormData(prev => ({ ...prev, [key]: '' }));
                      }}
                    >
                      Fotoğrafı Kaldır
                    </Button>
                  </Box>
                </Box>
              ) : (
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    📸 Kimlik ön yüz fotoğrafını yükleyin
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Kamera ile çekin veya cihazınızdan seçin
                  </Typography>
                  <Box sx={{ mt: 2, display: 'flex', gap: 1, justifyContent: 'center' }}>
                    <Button
                      size="small"
                      variant="outlined"
                      onClick={(e) => {
                        e.stopPropagation();
                        document.getElementById('id-front-photo-input')?.click();
                      }}
                    >
                      📷 Fotoğraf Seç
                    </Button>
                    <Button
                      size="small"
                      variant="outlined"
                      onClick={(e) => {
                        e.stopPropagation();
                        openCamera('idFrontPhoto');
                      }}
                    >
                      📱 Kamera ile Çek
                    </Button>
                  </Box>
                </Box>
              )}
            </Box>
            <input
              id="id-front-photo-input"
              type="file"
              accept="image/*"
              style={{ display: 'none' }}
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (file) {
                  if (!file.type.startsWith('image/')) {
                    alert('Lütfen sadece resim dosyası seçin.');
                    return;
                  }
                  if (file.size > 10 * 1024 * 1024) {
                    alert('Dosya boyutu 10MB\'dan küçük olmalıdır.');
                    return;
                  }
                  
                  try {
                    // Resmi backend'e yükle
                    const formData = new FormData();
                    formData.append('file', file);
                    
                    const response = await fetch('http://localhost:5001/api/Document/upload-id-front', {
                      method: 'POST',
                      headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                      },
                      body: formData
                    });
                    
                    if (response.ok) {
                      const result = await response.json();
                      console.log('✅ ID Front Photo uploaded:', result);
                      
                      // Resmi önizleme için base64'e çevir
                      const reader = new FileReader();
                      reader.onload = (e) => {
                        setDynamicFormData(prev => ({ 
                          ...prev, 
                          [key]: e.target?.result as string 
                        }));
                      };
                      reader.readAsDataURL(file);
                    } else {
                      const error = await response.json();
                      alert(`Kimlik ön yüz fotoğrafı yükleme hatası: ${error.message}`);
                    }
                  } catch (error) {
                    console.error('ID Front Photo upload error:', error);
                    alert('Kimlik ön yüz fotoğrafı yüklenirken bir hata oluştu.');
                  }
                }
              }}
            />
          </Box>
        );
      }
      
      // Kimlik arka yüz fotoğrafı için fotoğraf yükleme alanı
      if (offerFormData.insuranceTypeId === '6' && key === 'idBackPhoto') {
        return (
          <Box key={key} sx={{ width: '100%' }}>
            <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
              {getFieldLabel(key)}
            </Typography>
            <Box
              sx={{
                border: '2px dashed #ccc',
                borderRadius: 2,
                p: 3,
                textAlign: 'center',
                cursor: 'pointer',
                transition: 'border-color 0.3s',
                '&:hover': {
                  borderColor: theme.palette.primary.main,
                },
                backgroundColor: dynamicFormData[key] ? '#e8f5e8' : '#fafafa'
              }}
              onClick={() => document.getElementById('id-back-photo-input')?.click()}
            >
              {dynamicFormData[key] ? (
                <Box>
                  <img 
                    src={dynamicFormData[key]} 
                    alt="Kimlik Arka Yüz" 
                    style={{ 
                      maxWidth: '100%', 
                      maxHeight: '200px', 
                      borderRadius: '8px',
                      marginBottom: '10px'
                    }} 
                  />
                  <Typography variant="body2" color="success.main" sx={{ mb: 1 }}>
                    ✅ Kimlik Arka Yüz Fotoğrafı Yüklendi
                  </Typography>
                  <Box sx={{ mt: 1 }}>
                    <Button
                      size="small"
                      color="error"
                      onClick={(e) => {
                        e.stopPropagation();
                        setDynamicFormData(prev => ({ ...prev, [key]: '' }));
                      }}
                    >
                      Fotoğrafı Kaldır
                    </Button>
                  </Box>
                </Box>
              ) : (
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    📸 Kimlik arka yüz fotoğrafını yükleyin
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Kamera ile çekin veya cihazınızdan seçin
                  </Typography>
                  <Box sx={{ mt: 2, display: 'flex', gap: 1, justifyContent: 'center' }}>
                    <Button
                      size="small"
                      variant="outlined"
                      onClick={(e) => {
                        e.stopPropagation();
                        document.getElementById('id-back-photo-input')?.click();
                      }}
                    >
                      📷 Fotoğraf Seç
                    </Button>
                    <Button
                      size="small"
                      variant="outlined"
                      onClick={(e) => {
                        e.stopPropagation();
                        openCamera('idBackPhoto');
                      }}
                    >
                      📱 Kamera ile Çek
                    </Button>
                  </Box>
                </Box>
              )}
            </Box>
            <input
              id="id-back-photo-input"
              type="file"
              accept="image/*"
              style={{ display: 'none' }}
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (file) {
                  if (!file.type.startsWith('image/')) {
                    alert('Lütfen sadece resim dosyası seçin.');
                    return;
                  }
                  if (file.size > 10 * 1024 * 1024) {
                    alert('Dosya boyutu 10MB\'dan küçük olmalıdır.');
                    return;
                  }
                  
                  try {
                    // Resmi backend'e yükle
                    const formData = new FormData();
                    formData.append('file', file);
                    
                    const response = await fetch('http://localhost:5001/api/Document/upload-id-back', {
                      method: 'POST',
                      headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`
                      },
                      body: formData
                    });
                    
                    if (response.ok) {
                      const result = await response.json();
                      console.log('✅ ID Back Photo uploaded:', result);
                      
                      // Resmi önizleme için base64'e çevir
                      const reader = new FileReader();
                      reader.onload = (e) => {
                        setDynamicFormData(prev => ({ 
                          ...prev, 
                          [key]: e.target?.result as string 
                        }));
                      };
                      reader.readAsDataURL(file);
                    } else {
                      const error = await response.json();
                      alert(`Kimlik arka yüz fotoğrafı yükleme hatası: ${error.message}`);
                    }
                  } catch (error) {
                    console.error('ID Back Photo upload error:', error);
                    alert('Kimlik arka yüz fotoğrafı yüklenirken bir hata oluştu.');
                  }
                }
              }}
            />
          </Box>
        );
      }
      
      // Diğer alanlar için normal TextField
      return (
        <TextField
          key={key}
          fullWidth
          label={getFieldLabel(key)}
          value={dynamicFormData[key] || ''}
          onChange={(e) => setDynamicFormData(prev => ({
            ...prev,
            [key]: e.target.value
          }))}
          required
        />
      );
    });
  };

  // Teklif düzenleme
  const handleOfferEdit = (offer: any) => {
    console.log('Teklif düzenleme:', offer);
    // TODO: Teklif düzenleme modal'ı aç
    setError('Teklif düzenleme özelliği yakında eklenecek');
  };

  // Teklif iptal etme
  const handleOfferCancel = async (offerId: number) => {
    try {
      console.log('Teklif iptal ediliyor:', offerId);
      // TODO: Backend'e iptal isteği gönder
      setError('Teklif iptal etme özelliği yakında eklenecek');
    } catch (error) {
      console.error('Teklif iptal hatası:', error);
      setError('Teklif iptal edilirken hata oluştu');
    }
  };

  // Modal'ları kapat
  const handleCloseServicesModal = () => {
    setServicesModalOpen(false);
  };

  const handleCloseOfferModal = () => {
    setOfferModalOpen(false);
    setOfferStep(0);
    setOfferFormData({
      insuranceTypeId: '',
      finalPrice: '',
      status: 'Pending',
      department: '',
      coverageAmount: '',
      requestedStartDate: ''
    });
    setDynamicFormData({});
  };

  // Camera functions
  const openCamera = async (fieldName: string) => {
    try {
      setCameraField(fieldName);
      const stream = await navigator.mediaDevices.getUserMedia({ 
        video: { 
          facingMode: 'environment', // Arka kamera
          width: { ideal: 1280 },
          height: { ideal: 720 }
        } 
      });
      setCameraStream(stream);
      setIsCameraModalOpen(true);
    } catch (error) {
      console.error('Kamera erişim hatası:', error);
      alert('Kamera erişimi sağlanamadı. Lütfen kamera izinlerini kontrol edin.');
    }
  };

  const closeCamera = () => {
    if (cameraStream) {
      cameraStream.getTracks().forEach(track => track.stop());
      setCameraStream(null);
    }
    setIsCameraModalOpen(false);
    setCameraField('');
  };

  const capturePhoto = async () => {
    if (!cameraStream) return;
    
    try {
      const video = document.getElementById('camera-video') as HTMLVideoElement;
      const canvas = document.createElement('canvas');
      const context = canvas.getContext('2d');
      
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;
      
      if (context) {
        context.drawImage(video, 0, 0);
        
        canvas.toBlob(async (blob) => {
          if (blob) {
            const file = new File([blob], `photo_${Date.now()}.jpg`, { type: 'image/jpeg' });
            
            // Backend'e yükle
            const formData = new FormData();
            formData.append('file', file);
            
            const endpoint = cameraField === 'idFrontPhoto' 
              ? 'http://localhost:5001/api/Document/upload-id-front'
              : 'http://localhost:5001/api/Document/upload-id-back';
            
            const response = await fetch(endpoint, {
              method: 'POST',
              headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`
              },
              body: formData
            });
            
            if (response.ok) {
              const result = await response.json();
              console.log(`✅ ${cameraField} photo captured and uploaded:`, result);
              
              // Base64 preview için
              const reader = new FileReader();
              reader.onload = (e) => {
                setDynamicFormData(prev => ({ 
                  ...prev, 
                  [cameraField]: e.target?.result as string 
                }));
              };
              reader.readAsDataURL(file);
              
              closeCamera();
            } else {
              const error = await response.json();
              alert(`Fotoğraf yükleme hatası: ${error.message}`);
            }
          }
        }, 'image/jpeg', 0.8);
      }
    } catch (error) {
      console.error('Fotoğraf çekme hatası:', error);
      alert('Fotoğraf çekilirken bir hata oluştu.');
    }
  };

  // Fetch customer data on component mount
  useEffect(() => {
    const fetchCustomerData = async () => {
      console.log('🔍 CustomerDashboard: Starting fetchCustomerData');
      console.log('🔑 Token:', token);
      console.log('👤 User:', user);
      
      if (!token || !user) {
        console.log('❌ CustomerDashboard: No token or user data');
        setError('Kullanıcı bilgileri bulunamadı');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        // Fetch customer data
        console.log('📞 CustomerDashboard: Fetching customers...');
        const customers = await apiService.getCustomers();
        console.log('✅ CustomerDashboard: Customers fetched:', customers);
        
        const customer = customers.find(c => c.user?.email === user.email);
        console.log('🔍 CustomerDashboard: Looking for customer with email:', user.email);
        console.log('👤 CustomerDashboard: Found customer:', customer);
        
        if (customer) {
          setCustomerData(customer);
          console.log('💾 CustomerDashboard: Customer data set:', customer);
          console.log('🔍 CustomerDashboard: Customer ID:', customer.customerId);
          console.log('🔍 CustomerDashboard: Customer User ID:', customer.userId);
          console.log('🔍 CustomerDashboard: Full customer object:', JSON.stringify(customer, null, 2));
          
          // Fetch policies for this customer
          try {
            console.log('📋 CustomerDashboard: Fetching my policies...');
            const policiesData = await apiService.getMyPolicies();
            setPolicies(policiesData);
            console.log('✅ CustomerDashboard: Policies set:', policiesData);
          } catch (policyError) {
            console.log('❌ CustomerDashboard: Policies error:', policyError);
            setPolicies([]);
          }

          // Fetch claims for this customer
          try {
            console.log('📝 CustomerDashboard: Fetching claims...');
            const claimsData = await apiService.getMyClaims();
            setClaims(claimsData);
          } catch (claimsError) {
            console.log('❌ CustomerDashboard: Claims error:', claimsError);
            setClaims([]);
          }

          // Fetch documents for this customer
          try {
            console.log('📄 CustomerDashboard: Fetching documents...');
            const documentsData = await apiService.getMyDocuments();
            setDocuments(documentsData);
            console.log('✅ CustomerDashboard: Documents set:', documentsData);
          } catch (documentsError) {
            console.log('❌ CustomerDashboard: Documents error:', documentsError);
            setDocuments([]);
          }

          // Fetch offers for this customer
          try {
            console.log('📋 CustomerDashboard: Fetching offers for customer:', customer.customerId, 'userId:', customer.userId);
            // Try both customerId and userId
            let offersData;
            try {
              offersData = await apiService.getOffersByCustomer(customer.customerId);
              console.log('✅ CustomerDashboard: Offers fetched with customerId:', offersData);
            } catch (error) {
              console.log('⚠️ CustomerDashboard: Failed with customerId, trying userId:', error);
              if (customer.userId) {
                offersData = await apiService.getOffersByCustomer(customer.userId);
                console.log('✅ CustomerDashboard: Offers fetched with userId:', offersData);
              } else {
                throw error;
              }
            }
            
            setOffers(offersData || []);
            console.log('✅ CustomerDashboard: Offers set:', offersData);
          } catch (offerError) {
            console.log('❌ CustomerDashboard: Offers error:', offerError);
            setOffers([]);
          }
        } else {
          console.log('❌ CustomerDashboard: No customer found for email:', user.email);
          setError('Müşteri bilgileri bulunamadı');
        }
      } catch (error) {
        console.error('❌ CustomerDashboard: Error fetching data:', error);
        setError('Veriler alınırken hata oluştu');
      } finally {
        setLoading(false);
      }
    };

    fetchCustomerData();
  }, [token, user]);

  // Müşteri bilgilerini yükle
  useEffect(() => {
    const fetchCustomerInfo = async () => {
      if (!token || !user) return;
      
      try {
        console.log('🔍 Fetching customer info for user:', user.email);
        const customers = await apiService.getCustomers();
        const customer = customers.find(c => c.user?.email === user.email);
        
        if (customer) {
          setCustomerInfo(customer);
          console.log('✅ Customer info loaded:', customer);
        }
      } catch (error) {
        console.error('❌ Error fetching customer info:', error);
      }
    };

    fetchCustomerInfo();
  }, [token, user]);

  return (
    <Box sx={{ minHeight: '100vh', backgroundColor: theme.palette.background.default }}>
      <Header />
      
      {/* Main Content */}
      <Container maxWidth="xl" sx={{ mt: 3, mb: 3 }}>
        {/* Loading State */}
        {loading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', py: 8 }}>
            <CircularProgress size={60} />
            <Typography variant="h6" sx={{ ml: 2 }}>
              Veriler yükleniyor...
            </Typography>
          </Box>
        )}

        {/* Error State */}
        {error && !loading && (
          <Paper sx={{ p: 3, mb: 3, backgroundColor: '#ffebee', border: '1px solid #f44336' }}>
            <Typography variant="h6" color="error" gutterBottom>
              Hata Oluştu
            </Typography>
            <Typography variant="body1" color="error">
              {error}
            </Typography>
            <Button 
              variant="contained" 
              onClick={() => window.location.reload()} 
              sx={{ mt: 2 }}
            >
              Tekrar Dene
            </Button>
          </Paper>
        )}

        {/* Content - Only show when not loading and no error */}
        {!loading && !error && (
          <>
            {/* Header Section */}
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Box>
            <Typography variant="h4" component="h1" fontWeight={700} gutterBottom>
              Hoş Geldiniz, {customerData?.user?.name || 'Müşteri'}
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Sigorta poliçelerinizi ve taleplerinizi yönetin
            </Typography>
          </Box>
          
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <IconButton color="inherit">
              <Badge badgeContent={1} color="error">
                <Notifications />
              </Badge>
            </IconButton>
            <Avatar sx={{ bgcolor: theme.palette.primary.main }}>
              <Person />
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

        {/* Customer Info Card */}
        <Paper sx={{ p: 3, mb: 3, background: 'linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%)' }}>
          <Grid container spacing={3} alignItems="center">
            <Grid item xs={12} md={8}>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Avatar sx={{ width: 64, height: 64, mr: 3, bgcolor: theme.palette.primary.main }}>
                  <Person sx={{ fontSize: 32 }} />
                </Avatar>
                <Box>
                  <Typography variant="h5" fontWeight={600} gutterBottom>
                    {customerData?.user?.name || 'Müşteri'}
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Email sx={{ fontSize: 16, color: theme.palette.text.secondary }} />
                      <Typography variant="body2">{customerData?.user?.email || 'E-posta yok'}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Phone sx={{ fontSize: 16, color: theme.palette.text.secondary }} />
                      <Typography variant="body2">{customerData?.phone || 'Telefon yok'}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <LocationOn sx={{ fontSize: 16, color: theme.palette.text.secondary }} />
                      <Typography variant="body2">{customerData?.address || 'Adres yok'}</Typography>
                    </Box>
                  </Box>
                </Box>
              </Box>
            </Grid>
            <Grid item xs={12} md={4}>
              <Box sx={{ textAlign: 'right' }}>
                <Chip
                  label={customerData?.type || 'Bilinmiyor'}
                  color="primary"
                  sx={{ mb: 1 }}
                />
                <Typography variant="body2" color="text.secondary">
                  Üye olma: {customerData?.user?.createdAt ? new Date(customerData.user.createdAt).getFullYear().toString() : 'Bilinmiyor'}
                </Typography>
                <Button
                  startIcon={<Edit />}
                  variant="outlined"
                  onClick={handleProfileEdit}
                  sx={{ mt: 1 }}
                >
                  Profili Düzenle
                </Button>
              </Box>
            </Grid>
          </Grid>
        </Paper>

        {/* Statistics Cards and Quick Actions */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          {/* Statistics Cards */}
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
                    ) : stat.trend === 'down' ? (
                      <TrendingDown sx={{ color: '#f44336', mr: 1, fontSize: 20 }} />
                    ) : (
                      <Box sx={{ width: 20, mr: 1 }} />
                    )}
                    <Typography
                      variant="body2"
                      sx={{
                        color: stat.trend === 'up' ? '#4caf50' : 
                               stat.trend === 'down' ? '#f44336' : 
                               stat.trend === 'warning' ? '#ff9800' : theme.palette.text.secondary,
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
          
          {/* Quick Actions */}
          <Grid item xs={12} sm={12} md={4}>
            <Card
              sx={{
                height: '100%',
                background: 'linear-gradient(135deg, #f5f5f5 0%, #e0e0e0 100%)',
                border: '1px solid #e0e0e0',
              }}
            >
              <CardContent>
                <Typography variant="h6" color="text.secondary" gutterBottom>
                  Hızlı İşlemler
                </Typography>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
                  <Button
                    fullWidth
                    variant="contained"
                    startIcon={<Add />}
                    onClick={handleNewClaim}
                    sx={{ 
                      justifyContent: 'flex-start', 
                      py: 1.5,
                      background: 'linear-gradient(135deg, #2196f3 0%, #1976d2 100%)',
                      '&:hover': {
                        background: 'linear-gradient(135deg, #1976d2 0%, #1565c0 100%)',
                      },
                    }}
                  >
                    Yeni Talep Oluştur
                  </Button>
                  
                  <Button
                    fullWidth
                    variant="outlined"
                    startIcon={<Support />}
                    onClick={handleContactSupport}
                    sx={{ justifyContent: 'flex-start', py: 1.5 }}
                  >
                    Destek Al
                  </Button>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        {/* Main Content Grid */}
        <Grid container spacing={3}>
          {/* Main Content with Tabs */}
          <Grid item xs={12}>
            <Paper sx={{ p: 3 }}>
              <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
                <Tabs value={activeTab} onChange={handleTabChange}>
                  <Tab label="Aktif Poliçelerim" />
                  <Tab label="Eski Poliçelerim" />
                  <Tab label="Teklif Taleplerim" />
                  <Tab label="Olay Bildirimlerim" />
                  <Tab label="Dokümanlarım" />
                </Tabs>
              </Box>
              
              {/* Active Policies Tab */}
              {activeTab === 0 && (
                <>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h6" fontWeight={600}>
                  Aktif Poliçelerim
                </Typography>
                <Button
                  startIcon={<Add />}
                  variant="contained"
                  size="small"
                  onClick={handleNewClaim}
                  sx={{ borderRadius: 2 }}
                >
                  Yeni Talep
                </Button>
              </Box>
              
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Poliçe</TableCell>
                      <TableCell>Şirket</TableCell>
                      <TableCell>Prim</TableCell>
                      <TableCell>Geçerlilik</TableCell>
                      <TableCell>Durum</TableCell>
                      <TableCell>İşlemler</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {policies.filter(policy => {
                      const endDate = new Date(policy.endDate);
                      const today = new Date();
                      return endDate >= today;
                    }).length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                          <Box sx={{ textAlign: 'center' }}>
                            <Shield sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
                            <Typography variant="h6" color="text.secondary" gutterBottom>
                              Henüz aktif poliçeniz bulunmuyor
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                              Ödeme yaptığınız teklifleriniz burada görünecektir
                            </Typography>
                          </Box>
                        </TableCell>
                      </TableRow>
                    ) : (
                      policies
                        .filter(policy => {
                          // Aktif poliçeler (geçerlilik süresi dolmamış)
                          const endDate = new Date(policy.endDate);
                          const today = new Date();
                          return endDate >= today;
                        })
                        .map((policy) => (
                        <TableRow key={policy.policyId} hover>
                        <TableCell>
                          <Box sx={{ display: 'flex', alignItems: 'center' }}>
                            <Shield sx={{ fontSize: 24, color: theme.palette.primary.main, mr: 2 }} />
                            <Box>
                              <Typography variant="body2" fontWeight={500}>
                                  {policy.offer?.insuranceTypeName || 'Sigorta Poliçesi'}
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                  #{policy.policyNumber}
                              </Typography>
                            </Box>
                          </Box>
                        </TableCell>
                          <TableCell>
                            <Typography variant="body2">
                              Insurance Company
                            </Typography>
                          </TableCell>
                        <TableCell>
                          <Typography variant="body2" fontWeight={600}>
                              ₺{policy.totalPremium}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Box>
                            <Typography variant="body2" fontSize="0.75rem">
                                {new Date(policy.startDate).toLocaleDateString('tr-TR')} - {new Date(policy.endDate).toLocaleDateString('tr-TR')}
                            </Typography>
                          </Box>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={policy.status}
                            size="small"
                              color={policy.status === 'Active' ? 'success' : 'default'}
                          />
                        </TableCell>
                        <TableCell>
                          <IconButton 
                            size="small" 
                            color="primary"
                              title="Detaylar"
                              onClick={() => handleViewPolicyPdf(policy)}
                          >
                            <Visibility />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                      ))
                    )}
                  </TableBody>
                </Table>
              </TableContainer>
                </>
              )}

              {/* Expired Policies Tab */}
              {activeTab === 1 && (
                <>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                    <Typography variant="h6" fontWeight={600}>
                      Eski Poliçelerim
                    </Typography>
                  </Box>

                  <TableContainer component={Paper} sx={{ boxShadow: 'none', border: `1px solid ${theme.palette.divider}` }}>
                    <Table>
                      <TableHead>
                        <TableRow sx={{ backgroundColor: theme.palette.grey[50] }}>
                          <TableCell sx={{ fontWeight: 600 }}>Poliçe No</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Sigorta Türü</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Başlangıç</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Bitiş</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Prim</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Durum</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>İşlemler</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {policies
                          .filter(policy => {
                            // Geçerlilik süresi dolmuş poliçeler
                            const endDate = new Date(policy.endDate);
                            const today = new Date();
                            return endDate < today;
                          })
                          .map((policy: any) => (
                            <TableRow key={policy.policyId} hover>
                              <TableCell>{policy.policyNumber}</TableCell>
                              <TableCell>{policy.insuranceType?.name || 'Bilinmiyor'}</TableCell>
                              <TableCell>
                                {policy.startDate ? new Date(policy.startDate).toLocaleDateString('tr-TR') : 'Bilinmiyor'}
                              </TableCell>
                              <TableCell>
                                {policy.endDate ? new Date(policy.endDate).toLocaleDateString('tr-TR') : 'Bilinmiyor'}
                              </TableCell>
                              <TableCell>
                                ₺{policy.totalPremium ? policy.totalPremium.toLocaleString('tr-TR', { minimumFractionDigits: 2 }) : '0,00'}
                              </TableCell>
                              <TableCell>
                                <Chip 
                                  label="Süresi Dolmuş" 
                                  color="error" 
                                  size="small" 
                                  sx={{ fontWeight: 500 }}
                                />
                              </TableCell>
                              <TableCell>
                                <IconButton 
                                  size="small" 
                                  color="primary"
                                  title="Detaylar"
                                  onClick={() => handleViewPolicyPdf(policy)}
                                >
                                  <Visibility />
                                </IconButton>
                              </TableCell>
                            </TableRow>
                          ))
                        }
                        {policies.filter(policy => {
                          const endDate = new Date(policy.endDate);
                          const today = new Date();
                          return endDate < today;
                        }).length === 0 && (
                          <TableRow>
                            <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                              <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                                <Typography variant="body1" color="text.secondary">
                                  Henüz süresi dolmuş poliçeniz bulunmuyor.
                                </Typography>
                              </Box>
                            </TableCell>
                          </TableRow>
                        )}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </>
              )}

              {/* Offer Requests Tab */}
              {activeTab === 2 && (
                <>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                    <Typography variant="h6" fontWeight={600}>
                      Teklif Taleplerim
                    </Typography>
                  </Box>

                  {/* Alt Tab'lar */}
                  <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
                    <Tabs value={offerTabValue} onChange={handleOfferTabChange} aria-label="offer tabs">
                      <Tab label="Aktif Taleplerim" />
                      <Tab label="Onaylanmış Taleplerim" />
                    </Tabs>
                  </Box>
                  
                  <TableContainer>
                    <Table>
                      <TableHead>
                        <TableRow>
                          <TableCell>Teklif ID</TableCell>
                          <TableCell>Açıklama</TableCell>
                          <TableCell>Sigorta Türü</TableCell>
                          <TableCell>Temel Fiyat</TableCell>
                          <TableCell>İndirim Oranı</TableCell>
                          <TableCell>Final Fiyat</TableCell>
                          <TableCell>Kapsam Seviyesi</TableCell>
                          <TableCell>Başlangıç Tarihi</TableCell>
                          <TableCell>Geçerlilik</TableCell>
                          <TableCell>Durum</TableCell>
                          <TableCell>Müşteri Onayı</TableCell>
                          <TableCell>Oluşturulma</TableCell>
                          <TableCell>İşlemler</TableCell>
                          <TableCell>Ödeme</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {(() => {
                          const filteredOffers = getFilteredOffers();
                          return filteredOffers.length === 0 ? (
                            <TableRow>
                              <TableCell colSpan={14} sx={{ textAlign: 'center', py: 4 }}>
                                <Typography variant="body2" color="text.secondary">
                                  {offerTabValue === 0 ? 'Henüz aktif teklif talebi bulunmuyor.' : 'Henüz onaylanmış teklif talebi bulunmuyor.'}
                                </Typography>
                              </TableCell>
                            </TableRow>
                          ) : (
                            filteredOffers.map((offer) => (
                          <TableRow key={offer.offerId} hover>
                            <TableCell>
                              <Typography variant="body2" fontWeight={600}>
                                #{offer.offerId}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2" sx={{ maxWidth: 150, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                                {offer.description || 'N/A'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2">
                                {offer.insuranceTypeName || 'N/A'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2" fontWeight={600}>
                                ₺{offer.basePrice || '0'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2">
                                %{offer.discountRate || '0'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2" fontWeight={600} color="primary">
                                ₺{offer.finalPrice || offer.basePrice || '0'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="body2">
                                {offer.coverageAmount === 0 ? 'Temel Kapsam (+0%)' :
                                 offer.coverageAmount === 25 ? 'Orta Kapsam (+25%)' :
                                 offer.coverageAmount === 40 ? 'Premium Kapsam (+40%)' :
                                 'Bilinmeyen'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="caption">
                                {offer.requestedStartDate ? new Date(offer.requestedStartDate).toLocaleDateString('tr-TR') : 'N/A'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Typography variant="caption">
                                {offer.validUntil ? (() => {
                                  console.log('🔍 ValidUntil raw value:', offer.validUntil);
                                  console.log('🔍 ValidUntil type:', typeof offer.validUntil);
                                  
                                  // Basit tarih parsing
                                  let parsedDate;
                                  try {
                                    // Önce string olarak kontrol et
                                    const dateString = String(offer.validUntil);
                                    console.log('🔍 Date string:', dateString);
                                    
                                    // ISO formatına dönüştür (space'i T ile değiştir)
                                    const isoString = dateString.replace(' ', 'T');
                                    console.log('🔍 ISO string:', isoString);
                                    
                                    parsedDate = new Date(isoString);
                                    console.log('🔍 Parsed date:', parsedDate);
                                    console.log('🔍 Is valid:', !isNaN(parsedDate.getTime()));
                                    console.log('🔍 Year:', parsedDate.getFullYear());
                                    
                                    // Sadece gerçekten geçersiz tarihleri kontrol et
                                    if (isNaN(parsedDate.getTime()) || 
                                        parsedDate.getFullYear() < 1900 || 
                                        parsedDate.getFullYear() > 2100) {
                                      console.log('⚠️ Invalid date detected');
                                      return 'Geçersiz Tarih';
                                    }
                                    
                                    // Güvenli tarih formatlaması
                                    const day = parsedDate.getDate().toString().padStart(2, '0');
                                    const month = (parsedDate.getMonth() + 1).toString().padStart(2, '0');
                                    const year = parsedDate.getFullYear();
                                    const result = `${day}.${month}.${year}`;
                                    console.log('✅ Final result:', result);
                                    return result;
                                  } catch (error) {
                                    console.error('❌ Date parsing error:', error);
                                    return 'Tarih Hatası';
                                  }
                                })() : 'N/A'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Chip
                                label={offer.status || 'pending'}
                                size="small"
                                color={
                                  offer.status === 'approved' ? 'success' :
                                  offer.status === 'rejected' ? 'error' :
                                  offer.status === 'pending' ? 'warning' : 'default'
                                }
                              />
                            </TableCell>
                            <TableCell>
                              {offer.status === 'approved' ? (
                                <FormControl size="small" sx={{ minWidth: 120 }} data-offer-id={offer.offerId}>
                                  <Select
                                    value={dropdownSelections[offer.offerId] || (offer.isCustomerApproved ? 'approved' : 'pending')}
                                    onChange={(e) => handleOfferApproval(offer.offerId, e.target.value)}
                                    displayEmpty
                                  >
                                    <MenuItem key="pending" value="pending">Beklemede</MenuItem>
                                    <MenuItem key="approved" value="approved">Onaylandı</MenuItem>
                                  </Select>
                                </FormControl>
                              ) : (
                                <Typography variant="caption">
                                  {offer.isCustomerApproved ? 'Onaylandı' : 'Beklemede'}
                                </Typography>
                              )}
                            </TableCell>
                            <TableCell>
                              <Typography variant="caption">
                                {offer.createdAt ? new Date(offer.createdAt).toLocaleDateString('tr-TR') : 'N/A'}
                              </Typography>
                            </TableCell>
                            <TableCell>
                              <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                                <IconButton 
                                  size="medium" 
                                  color="primary"
                                  onClick={() => handleViewOffer(offer)}
                                  title="Detaylar"
                                  sx={{ 
                                    transform: 'scale(1.2)',
                                    '&:hover': {
                                      transform: 'scale(1.3)',
                                    }
                                  }}
                                >
                                  <Visibility sx={{ fontSize: '1.2rem' }} />
                                </IconButton>
                                {/* Sadece ödeme yapılmamış (isCustomerApproved = false) taleplerde sil butonu göster */}
                                {!offer.isCustomerApproved && (
                                  <IconButton 
                                    size="medium" 
                                    color="error"
                                    onClick={() => handleDeleteOffer(offer.offerId)}
                                    title="Sil"
                                    sx={{ 
                                      transform: 'scale(1.2)',
                                      '&:hover': {
                                        transform: 'scale(1.3)',
                                      }
                                    }}
                                  >
                                    <Delete sx={{ fontSize: '1.2rem' }} />
                                  </IconButton>
                                )}
                              </Box>
                            </TableCell>
                            <TableCell>
                              {/* Sadece aktif taleplerde ödeme butonu göster */}
                              {offerTabValue === 0 ? (
                                <IconButton 
                                  size="medium"
                                  color="primary"
                                  onClick={() => handlePaymentOffer(offer)}
                                  title="Ödeme Yap"
                                  disabled={dropdownSelections[offer.offerId] !== 'approved'}
                                  sx={{ 
                                    transform: 'scale(1.5)',
                                    backgroundColor: '#10b981',
                                    color: 'white',
                                    opacity: dropdownSelections[offer.offerId] === 'approved' ? 1 : 0.5,
                                    '&:hover': {
                                      transform: 'scale(1.6)',
                                      backgroundColor: '#059669',
                                    },
                                    '&:disabled': {
                                      transform: 'scale(1.5)',
                                      opacity: 0.5,
                                      cursor: 'not-allowed',
                                      backgroundColor: '#6b7280',
                                    }
                                  }}
                                >
                                  <Payment sx={{ fontSize: '1.2rem' }} />
                                </IconButton>
                              ) : (
                                <Chip
                                  label="Ödeme Yapıldı"
                                  size="small"
                                  color="success"
                                  variant="outlined"
                                />
                              )}
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

              {/* Claims Tab - Olay Bildirimlerim */}
              {activeTab === 3 && (
                <>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                    <Typography variant="h6" fontWeight={600}>
                      Olay Bildirimlerim
                    </Typography>
                  </Box>

                  {claims.length === 0 ? (
                    <Box sx={{ textAlign: 'center', py: 8 }}>
                      <Typography variant="body1" color="text.secondary">
                        Henüz olay bildirimi bulunmamaktadır.
                      </Typography>
                    </Box>
                  ) : (
                    <TableContainer>
                      <Table>
                        <TableHead>
                          <TableRow>
                            <TableCell>Poliçe No</TableCell>
                            <TableCell>Olay Türü</TableCell>
                            <TableCell>Açıklama</TableCell>
                            <TableCell>Olay Tarihi</TableCell>
                            <TableCell>Durum</TableCell>
                            <TableCell>Yetkili Notu</TableCell>
                            <TableCell>İşleyen Yetkili</TableCell>
                            <TableCell>Yetkili E-posta</TableCell>
                            <TableCell>Yetkili Telefon</TableCell>
                            <TableCell>Oluşturma Tarihi</TableCell>
                            <TableCell align="center">İşlemler</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {claims.map((claim) => (
                            <TableRow key={claim.claimId}>
                              <TableCell>{claim.policyNumber || '-'}</TableCell>
                              <TableCell>
                                <Chip 
                                  label={claim.type} 
                                  size="small" 
                                  color="primary"
                                  variant="outlined"
                                />
                              </TableCell>
                              <TableCell>
                                <Typography variant="body2" sx={{ maxWidth: 300, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                                  {claim.description}
                                </Typography>
                              </TableCell>
                              <TableCell>
                                {claim.incidentDate ? new Date(claim.incidentDate).toLocaleDateString('tr-TR') : '-'}
                              </TableCell>
                              <TableCell>
                                <Chip 
                                  label={
                                    claim.status === 'Pending' ? 'Beklemede' :
                                    claim.status === 'Approved' ? 'Onaylandı' :
                                    claim.status === 'Rejected' ? 'Reddedildi' : claim.status
                                  }
                                  size="small"
                                  color={
                                    claim.status === 'Pending' ? 'warning' :
                                    claim.status === 'Approved' ? 'success' :
                                    claim.status === 'Rejected' ? 'error' :
                                    'default'
                                  }
                                />
                              </TableCell>
                              <TableCell>
                                <Typography variant="body2" sx={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                                  {claim.notes || '-'}
                                </Typography>
                              </TableCell>
                              <TableCell>
                                <Typography variant="body2">
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
                                {claim.createdAt ? new Date(claim.createdAt).toLocaleDateString('tr-TR') : '-'}
                              </TableCell>
                              <TableCell align="center">
                                <Box sx={{ display: 'flex', gap: 1, justifyContent: 'center' }}>
                                  {claim.status === 'Pending' && (
                                    <>
                                      <Tooltip title="Düzenle">
                                        <IconButton 
                                          size="small" 
                                          color="primary"
                                          onClick={async () => {
                                            setSelectedClaim(claim);
                                            setEditClaimData({
                                              description: claim.description || ''
                                            });
                                            
                                            // Fetch documents for this claim
                                            try {
                                              const allDocs = await apiService.getMyDocuments();
                                              const claimDocs = allDocs.filter((doc: any) => doc.claimId === claim.claimId);
                                              setClaimDocuments(claimDocs);
                                            } catch (error) {
                                              console.error('Error fetching claim documents:', error);
                                              setClaimDocuments([]);
                                            }
                                            
                                            setNewClaimFiles([]);
                                            setEditClaimModalOpen(true);
                                          }}
                                        >
                                          <Edit />
                                        </IconButton>
                                      </Tooltip>
                                      <Tooltip title="Sil">
                                        <IconButton 
                                          size="small" 
                                          color="error"
                                          onClick={async () => {
                                            if (window.confirm('Bu olay bildirimini silmek istediğinizden emin misiniz?')) {
                                              try {
                                                await apiService.deleteMyClaim(claim.claimId);
                                                // Refresh claims list
                                                const updatedClaims = await apiService.getMyClaims();
                                                setClaims(updatedClaims);
                                                alert('Olay bildirimi başarıyla silindi');
                                              } catch (error: any) {
                                                alert(error.message || 'Olay bildirimi silinemedi');
                                              }
                                            }
                                          }}
                                        >
                                          <Delete />
                                        </IconButton>
                                      </Tooltip>
                                    </>
                                  )}
                                  {claim.status !== 'Pending' && (
                                    <Typography variant="caption" color="text.secondary">
                                      {claim.status === 'Approved' && 'Onaylandı'}
                                      {claim.status === 'Rejected' && 'Reddedildi'}
                                    </Typography>
                                  )}
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

              {/* Documents Tab */}
              {activeTab === 4 && (
                <>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                    <Typography variant="h6" fontWeight={600}>
                      Dokümanlarım
                    </Typography>
                  </Box>
                  
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                    {documentsData.map((doc) => (
                      <Box
                        key={doc.id}
                        sx={{
                          p: 2,
                          border: `1px solid ${theme.palette.divider}`,
                          borderRadius: 2,
                          backgroundColor: theme.palette.background.paper,
                        }}
                      >
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                          <Typography variant="body2" fontWeight={600}>
                            {doc.name}
                          </Typography>
                          <IconButton 
                            size="small" 
                            color="primary"
                            title="Detaylar"
                            onClick={() => handleViewDocument(doc)}
                          >
                            <Visibility />
                          </IconButton>
                        </Box>
                        <Typography variant="caption" color="text.secondary" display="block">
                          {doc.type} - {doc.size} - {doc.date}
                        </Typography>
                      </Box>
                    ))}
                    {documentsData.length === 0 && (
                      <Box sx={{ textAlign: 'center', py: 4 }}>
                        <Typography variant="body1" color="text.secondary">
                          Henüz dokümanınız bulunmuyor.
                        </Typography>
                      </Box>
                    )}
                  </Box>
                </>
              )}
            </Paper>
          </Grid>

        </Grid>
        </>
        )}

        {/* Hizmet Seçimi Modal'ı */}
        <Dialog 
          open={servicesModalOpen} 
          onClose={handleCloseServicesModal}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle sx={{ textAlign: 'center', pb: 1 }}>
              Hizmet Seçin
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              İhtiyacınıza uygun sigorta hizmetini seçin
            </Typography>
          </DialogTitle>
          
          <DialogContent>
            <Grid container spacing={3}>
              {insuranceServices.map((service) => (
                <Grid item xs={12} sm={6} md={4} key={service.id}>
                  <Card 
                    sx={{ 
                      cursor: 'pointer',
                      transition: 'all 0.3s ease',
                      '&:hover': {
                        transform: 'translateY(-4px)',
                        boxShadow: theme.shadows[8],
                      }
                    }}
                    onClick={() => handleServiceSelect(service)}
                  >
                    <CardContent sx={{ textAlign: 'center', p: 3 }}>
                      <Box sx={{ mb: 2 }}>
                        {service.icon}
                      </Box>
                      <Typography variant="h6" fontWeight={600} gutterBottom>
                        {service.name}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                        {service.description}
                      </Typography>
                      <Box sx={{ mb: 2 }}>
                        {service.features.map((feature, index) => (
                          <Chip
                            key={index}
                            label={feature}
                            size="small"
                            variant="outlined"
                            sx={{ mr: 0.5, mb: 0.5 }}
                          />
                        ))}
                      </Box>
                      <Typography variant="h6" color="primary" fontWeight={600}>
                        ₺{service.basePrice.toLocaleString()}/yıl
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
          </DialogContent>
          
          <DialogActions sx={{ p: 3 }}>
            <Button onClick={handleCloseServicesModal} variant="outlined">
              İptal
            </Button>
          </DialogActions>
        </Dialog>

        {/* Teklif Alma Modal'ı */}
        <Dialog 
          open={offerModalOpen} 
          onClose={handleCloseOfferModal}
          maxWidth="sm"
          fullWidth
        >
          <DialogTitle sx={{ textAlign: 'center', pb: 1 }}>
              {selectedService?.name} - Teklif Al
          </DialogTitle>
          
          <DialogContent>
            <Stepper activeStep={offerStep} sx={{ mb: 4 }}>
              <Step>
                <StepLabel>Bilgiler</StepLabel>
              </Step>
              <Step>
                <StepLabel>Kapsam</StepLabel>
              </Step>
              <Step>
                <StepLabel>Tamamlandı</StepLabel>
              </Step>
            </Stepper>

            {offerStep === 0 && (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                <TextField
                  fullWidth
                  label="Müşteri ID"
                  value={customerData?.customerId || ''}
                  InputProps={{ readOnly: true }}
                  sx={{ 
                    '& .MuiInputBase-input': { 
                      backgroundColor: theme.palette.grey[100],
                      color: theme.palette.text.secondary 
                    } 
                  }}
                  helperText="Bu alan değiştirilemez"
                />
                <TextField
                  fullWidth
                  label="Sigorta Türü ID"
                  value={offerFormData.insuranceTypeId}
                  onChange={(e) => handleOfferFormChange('insuranceTypeId', e.target.value)}
                  required
                  disabled
                />
                <TextField
                  fullWidth
                  label="Departman"
                  value={offerFormData.department}
                  onChange={(e) => handleOfferFormChange('department', e.target.value)}
                  required
                  disabled
                />
                
                {/* Dinamik form alanları */}
                {renderDynamicFormFields()}
              </Box>
            )}

            {offerStep === 1 && (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                <FormControl fullWidth>
                  <FormLabel>Kapsam Seviyesi</FormLabel>
                  <RadioGroup
                    value={offerFormData.coverageAmount}
                    onChange={(e) => handleOfferFormChange('coverageAmount', e.target.value)}
                  >
                    <FormControlLabel value="0" control={<Radio />} label="Temel Kapsam (+0% fiyat artışı)" />
                    <FormControlLabel value="25" control={<Radio />} label="Orta Kapsam (+25% fiyat artışı)" />
                    <FormControlLabel value="40" control={<Radio />} label="Premium Kapsam (+40% fiyat artışı)" />
                  </RadioGroup>
                  <FormHelperText>
                    Seçilen kapsam seviyesi, acente/admin tarafından belirlenen temel fiyata eklenir.
                  </FormHelperText>
                </FormControl>
                
                <TextField
                  fullWidth
                  label="Başlangıç Tarihi"
                  type="date"
                  value={offerFormData.requestedStartDate}
                  onChange={(e) => handleOfferFormChange('requestedStartDate', e.target.value)}
                  InputLabelProps={{ shrink: true }}
                  inputProps={{ 
                    min: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().split('T')[0] // En az 1 gün sonra
                  }}
                  helperText="En az 1 gün sonraki bir tarih seçin"
                  required
                />
              </Box>
            )}

            {offerStep === 2 && (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                {offerSuccess ? (
                  <Alert severity="success" sx={{ mb: 2 }}>
                    Teklif talebiniz başarıyla alındı! En kısa sürede size ulaşacağız.
                  </Alert>
                ) : (
                  <CircularProgress />
                )}
              </Box>
            )}
          </DialogContent>
          
          <DialogActions sx={{ p: 3 }}>
            {offerStep > 0 && (
              <Button 
                onClick={() => setOfferStep(prev => prev - 1)} 
                variant="outlined"
                disabled={offerLoading}
              >
                Geri
              </Button>
            )}
            
            {offerStep === 0 && (
              <Button 
                onClick={() => setOfferStep(prev => prev + 1)} 
                variant="contained"
                disabled={offerLoading}
              >
                Devam Et
              </Button>
            )}
            
            {offerStep === 1 && (
              <Button 
                onClick={handleOfferSubmit}
                variant="contained"
                disabled={offerLoading || !offerFormData.coverageAmount || !offerFormData.requestedStartDate}
                startIcon={offerLoading ? <CircularProgress size={20} /> : null}
              >
                {offerLoading ? 'Gönderiliyor...' : 'Teklif Gönder'}
              </Button>
            )}
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
            <span>Teklif Detayları</span>
          </DialogTitle>
          <DialogContent sx={{ p: 3 }}>
            {selectedViewOffer && (
              <Box>
                {/* Header Info */}
                <Box sx={{ mb: 3, p: 2, backgroundColor: 'grey.50', borderRadius: 1 }}>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    Teklif #{selectedViewOffer.offerId}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {selectedViewOffer.insuranceTypeName} - {selectedViewOffer.department}
                  </Typography>
                </Box>

                {/* Basic Details */}
                <Grid container spacing={2} sx={{ mb: 3 }}>
                  <Grid item xs={12} md={6}>
                    <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                      📊 Temel Bilgiler
                    </Typography>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                      <Typography variant="body2">
                        <strong>Temel Fiyat:</strong> ₺{selectedViewOffer.basePrice}
                      </Typography>
                      <Typography variant="body2">
                        <strong>İndirim Oranı:</strong> %{selectedViewOffer.discountRate}
                      </Typography>
                      <Typography variant="body2">
                        <strong>Final Fiyat:</strong> ₺{selectedViewOffer.finalPrice}
                      </Typography>
                      <Typography variant="body2">
                        <strong>Teminat Tutarı:</strong> ₺{selectedViewOffer.coverageAmount}
                      </Typography>
                    </Box>
                  </Grid>
                  <Grid item xs={12} md={6}>
                    <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                      📅 Tarihler
                    </Typography>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                      <Typography variant="body2">
                        <strong>Başlangıç:</strong> {selectedViewOffer.requestedStartDate ? new Date(selectedViewOffer.requestedStartDate).toLocaleDateString('tr-TR') : 'N/A'}
                      </Typography>
                      <Typography variant="body2">
                        <strong>Geçerlilik:</strong> {selectedViewOffer.validUntil ? new Date(selectedViewOffer.validUntil).toLocaleDateString('tr-TR') : 'N/A'}
                      </Typography>
                      <Typography variant="body2">
                        <strong>Oluşturulma:</strong> {selectedViewOffer.createdAt ? new Date(selectedViewOffer.createdAt).toLocaleDateString('tr-TR') : 'N/A'}
                      </Typography>
                    </Box>
                  </Grid>
                </Grid>

                {/* Status */}
                <Box sx={{ mb: 3, p: 2, backgroundColor: 'grey.50', borderRadius: 1 }}>
                  <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                    📋 Durum Bilgileri
                  </Typography>
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                    <Typography variant="body2">
                      <strong>Durum:</strong> {selectedViewOffer.status}
                    </Typography>
                    <Typography variant="body2">
                      <strong>Müşteri Onayı:</strong> {selectedViewOffer.isCustomerApproved ? 'Onaylandı' : 'Beklemede'}
                    </Typography>
                    {selectedViewOffer.customerApprovedAt && (
                      <Typography variant="body2">
                        <strong>Onay Tarihi:</strong> {new Date(selectedViewOffer.customerApprovedAt).toLocaleDateString('tr-TR')}
                      </Typography>
                    )}
                  </Box>
                </Box>

                {/* Policy PDF Section */}
                {selectedViewOffer.policyPdfUrl && (
                  <Box sx={{ mb: 3, p: 2, backgroundColor: 'warning.50', borderRadius: 1 }}>
                    <Typography variant="subtitle2" fontWeight={600} gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
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
                {(selectedViewOffer.customerAdditionalInfo || documents.length > 0) && (
                  <Box sx={{ mb: 3, p: 2, backgroundColor: 'grey.50', borderRadius: 1 }}>
                    <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                      📝 Form Bilgileri ve Yüklenen Dosyalar
                    </Typography>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                      {/* Show uploaded documents first */}
                      {documents
                        .filter(doc => {
                          // Show documents that belong to this customer
                          // Current user is test@customer.com (User_Id = 9) with Customer_Id = 2
                          // Exclude auto-generated policy PDFs
                          return doc.customerId === 2 && !doc.fileName?.includes('Poliçe_Offer_');
                        })
                        .map((doc) => {
                          const isPdfFile = doc.fileType === 'application/pdf' || doc.fileName?.includes('.pdf');
                          const fileUrl = doc.fileUrl?.startsWith('/uploads/') 
                            ? `http://localhost:5000/api/Document/serve${doc.fileUrl.replace('/uploads/', '/')}`
                            : doc.fileUrl;
                          
                          return (
                            <Box key={`doc-${doc.documentId}`} sx={{ display: 'flex', alignItems: 'center', gap: 1, p: 1, backgroundColor: 'white', borderRadius: 1, border: '1px solid #e0e0e0', mb: 1 }}>
                              <Typography variant="body2" sx={{ flex: 1 }}>
                                <strong>📄 {doc.fileName || 'Doküman'}:</strong>
                              </Typography>
                              <Button
                                variant="outlined"
                                size="small"
                                color="primary"
                                startIcon={<Description />}
                                onClick={() => {
                                  setPdfUrl(fileUrl);
                                  setPdfTitle(doc.fileName || 'Doküman');
                                  setIsPdfModalOpen(true);
                                }}
                              >
                                PDF Aç
                              </Button>
                            </Box>
                          );
                        })}
                      
                      {/* Show form data (excluding files since they're now in documents) */}
                      {selectedViewOffer.customerAdditionalInfo && (() => {
                        try {
                          const additionalInfo = typeof selectedViewOffer.customerAdditionalInfo === 'string' 
                            ? JSON.parse(selectedViewOffer.customerAdditionalInfo) 
                            : selectedViewOffer.customerAdditionalInfo;
                          
                          return Object.entries(additionalInfo)
                            .filter(([key, value]) => {
                              // Exclude file entries since they're now handled by documents
                              const isPdfFile = String(value).includes('.pdf') && String(value).includes('/uploads/');
                              const isImageFile = String(value).includes('/uploads/') && (String(value).includes('.jpg') || String(value).includes('.jpeg') || String(value).includes('.png'));
                              return !isPdfFile && !isImageFile;
                            })
                            .map(([key, value]) => {
                              const label = key === 'destination' ? '📍 Hedef' :
                                           key === 'address' ? '🏠 Adres' :
                                           key === 'propertyType' ? '🏢 Bina Tipi' :
                                           key === 'buildingAge' ? '📅 Bina Yaşı' :
                                           key === 'securityFeatures' ? '🔒 Güvenlik Önlemleri' :
                                           key === 'insuranceType' ? '📋 Sigorta Türü' :
                                           key === 'travelDuration' ? '⏰ Seyahat Süresi' :
                                           key === 'travelPurpose' ? '🎯 Seyahat Amacı' :
                                           key.charAt(0).toUpperCase() + key.slice(1);
                              
                              return (
                                <Typography key={key} variant="body2">
                                  <strong>{label}:</strong> {String(value)}
                                </Typography>
                              );
                            });
                        } catch (error) {
                          return (
                            <Typography variant="body2">
                              <strong>Ek Bilgiler:</strong> {selectedViewOffer.customerAdditionalInfo}
                            </Typography>
                          );
                        }
                      })()}
                    </Box>
                  </Box>
                )}
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

        {/* Payment Modal */}
        <Dialog 
          open={paymentModalOpen} 
          onClose={() => setPaymentModalOpen(false)} 
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
            <Payment />
            <span>Ödeme İşlemi</span>
          </DialogTitle>
          <DialogContent sx={{ p: 3 }}>
            {selectedPaymentOffer && (
              <Box>
                {!paymentSuccess ? (
                  <>
                    {/* Payment Info */}
                    <Box sx={{ mb: 3, p: 2, backgroundColor: 'grey.50', borderRadius: 1 }}>
                      <Typography variant="h6" fontWeight={600} gutterBottom>
                        Ödeme Bilgileri
                      </Typography>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Box>
                          <Typography variant="body2">
                            <strong>Teklif No:</strong> #{selectedPaymentOffer.offerId}
                          </Typography>
                          <Typography variant="body2">
                            <strong>Sigorta Türü:</strong> {selectedPaymentOffer.insuranceTypeName}
                          </Typography>
                          <Typography variant="body2">
                            <strong>Teminat Tutarı:</strong> ₺{selectedPaymentOffer.coverageAmount}
                          </Typography>
                        </Box>
                        <Box sx={{ textAlign: 'right' }}>
                          <Typography variant="h5" color="primary" fontWeight={600}>
                            ₺{selectedPaymentOffer.finalPrice}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            Final Fiyat
                          </Typography>
                        </Box>
                      </Box>
                    </Box>

                    {/* Payment Form */}
                    <Grid container spacing={2}>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="Kart Numarası"
                          value={paymentData.cardNumber}
                          onChange={handleCardNumberChange}
                          placeholder="1234 5678 9012 3456"
                          inputProps={{ maxLength: 19 }}
                        />
                      </Grid>
                      <Grid item xs={3}>
                        <FormControl fullWidth>
                          <InputLabel>Ay</InputLabel>
                          <Select
                            value={paymentData.expiryMonth}
                            onChange={(e) => setPaymentData(prev => ({ ...prev, expiryMonth: e.target.value }))}
                            label="Ay"
                          >
                            <MenuItem key="01" value="01">01</MenuItem>
                            <MenuItem key="02" value="02">02</MenuItem>
                            <MenuItem key="03" value="03">03</MenuItem>
                            <MenuItem key="04" value="04">04</MenuItem>
                            <MenuItem key="05" value="05">05</MenuItem>
                            <MenuItem key="06" value="06">06</MenuItem>
                            <MenuItem key="07" value="07">07</MenuItem>
                            <MenuItem key="08" value="08">08</MenuItem>
                            <MenuItem key="09" value="09">09</MenuItem>
                            <MenuItem key="10" value="10">10</MenuItem>
                            <MenuItem key="11" value="11">11</MenuItem>
                            <MenuItem key="12" value="12">12</MenuItem>
                          </Select>
                        </FormControl>
                      </Grid>
                      <Grid item xs={3}>
                        <FormControl fullWidth>
                          <InputLabel>Yıl</InputLabel>
                          <Select
                            value={paymentData.expiryYear}
                            onChange={(e) => setPaymentData(prev => ({ ...prev, expiryYear: e.target.value }))}
                            label="Yıl"
                          >
                            {Array.from({ length: 15 }, (_, i) => {
                              const year = new Date().getFullYear() + i;
                              return (
                                <MenuItem key={year} value={year.toString().slice(-2)}>
                                  {year}
                                </MenuItem>
                              );
                            })}
                          </Select>
                        </FormControl>
                      </Grid>
                      <Grid item xs={6}>
                        <TextField
                          fullWidth
                          label="CVV"
                          value={paymentData.cvv}
                          onChange={(e) => setPaymentData(prev => ({ ...prev, cvv: e.target.value }))}
                          placeholder="123"
                          inputProps={{ maxLength: 3 }}
                        />
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="Kart Sahibinin Adı"
                          value={paymentData.cardName}
                          onChange={(e) => setPaymentData(prev => ({ ...prev, cardName: e.target.value }))}
                          placeholder="Ad Soyad"
                        />
                      </Grid>
                      <Grid item xs={12}>
                        <TextField
                          fullWidth
                          label="E-posta Adresi"
                          type="email"
                          value={paymentData.email}
                          onChange={(e) => setPaymentData(prev => ({ ...prev, email: e.target.value }))}
                          placeholder="ornek@email.com"
                        />
                      </Grid>
                    </Grid>
                  </>
                ) : (
                  <>
                    {/* Success Message */}
                    <Box sx={{ textAlign: 'center', py: 2 }}>
                      <Typography variant="h5" fontWeight={600} color="success.main" gutterBottom>
                        Ödeme Başarılı!
                      </Typography>
                      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
                        Sigorta poliçeniz aktif edilmiştir.
                      </Typography>
                    </Box>



                    {/* Print Button */}
                    <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
                      <Button
                        variant="outlined"
                        startIcon={<Print />}
                        onClick={handlePrintReceipt}
                        sx={{ minWidth: 150 }}
                      >
                        Yazdır
                      </Button>
                    </Box>
                  </>
                )}
              </Box>
            )}
          </DialogContent>
          <DialogActions sx={{ p: 2, backgroundColor: 'grey.50' }}>
            {!paymentSuccess ? (
              <>
                <Button 
                  onClick={() => setPaymentModalOpen(false)} 
                  variant="outlined"
                >
                  İptal
                </Button>
                <Button 
                  onClick={handlePaymentSubmit}
                  variant="contained"
                  color="primary"
                  disabled={paymentLoading || !paymentData.cardNumber || !paymentData.expiryMonth || !paymentData.expiryYear || !paymentData.cvv || !paymentData.cardName || !paymentData.email}
                  startIcon={paymentLoading ? <CircularProgress size={20} /> : <Payment />}
                >
                  {paymentLoading ? 'İşleniyor...' : `₺${selectedPaymentOffer?.finalPrice} Öde`}
                </Button>
              </>
            ) : (
              <>
                <Button
                  variant="outlined"
                  startIcon={<Print />}
                  onClick={handlePrintReceipt}
                >
                  Yazdır
                </Button>
                <Button 
                  onClick={() => setPaymentModalOpen(false)} 
                  variant="contained"
                  color="primary"
                >
                  Tamam
                </Button>
              </>
            )}
          </DialogActions>
        </Dialog>

        {/* Policy PDF Viewer Modal */}
        <Dialog 
          open={policyPdfModalOpen} 
          onClose={() => setPolicyPdfModalOpen(false)} 
          maxWidth="lg"
          fullWidth
          PaperProps={{
            sx: {
              borderRadius: 2,
              boxShadow: '0 8px 32px rgba(0,0,0,0.12)',
              height: '90vh'
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
            <Shield />
            <span>Poliçe Dokümanı - {selectedPolicy?.policyNumber}</span>
          </DialogTitle>
          <DialogContent sx={{ p: 0, height: '100%' }}>
            {policyPdfUrl && (
              <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                {/* PDF Viewer */}
                <Box sx={{ flex: 1, minHeight: 0 }}>
                  <iframe
                    src={`http://localhost:5001${policyPdfUrl}`}
                    width="100%"
                    height="100%"
                    style={{ border: 'none' }}
                    title={`Poliçe ${selectedPolicy?.policyNumber}`}
                  />
                </Box>
                
                {/* PDF Info */}
                {selectedPolicy && (
                  <Box sx={{ p: 2, backgroundColor: 'grey.50', borderTop: 1, borderColor: 'divider' }}>
                    <Typography variant="body2" color="text.secondary">
                      <strong>Poliçe Numarası:</strong> {selectedPolicy.policyNumber} | 
                      <strong> Sigorta Türü:</strong> {selectedPolicy.offer?.insuranceTypeName || 'Bilinmeyen'} | 
                      <strong> Prim:</strong> ₺{selectedPolicy.totalPremium} | 
                      <strong> Durum:</strong> {selectedPolicy.status}
                    </Typography>
                  </Box>
                )}
              </Box>
            )}
          </DialogContent>
          <DialogActions sx={{ p: 2, backgroundColor: 'grey.50' }}>
            <Button
              onClick={() => setPolicyPdfModalOpen(false)}
              variant="outlined"
            >
              Kapat
            </Button>
            <Button
              onClick={handleDownloadPolicyPdf}
              variant="contained"
              color="primary"
              startIcon={<span>📥</span>}
            >
              İndir
            </Button>
          </DialogActions>
        </Dialog>

        {/* Camera Modal */}
        <Dialog 
          open={isCameraModalOpen} 
          onClose={closeCamera}
          maxWidth="sm"
          fullWidth
          PaperProps={{
            sx: {
              borderRadius: 2,
              boxShadow: '0 8px 32px rgba(0,0,0,0.12)',
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
            📱 Kamera ile Fotoğraf Çek
          </DialogTitle>
          <DialogContent sx={{ p: 2, textAlign: 'center' }}>
            <Box sx={{ 
              position: 'relative',
              width: '100%',
              maxWidth: '400px',
              margin: '0 auto'
            }}>
              <video
                id="camera-video"
                autoPlay
                playsInline
                style={{
                  width: '100%',
                  height: 'auto',
                  borderRadius: '8px',
                  backgroundColor: '#000'
                }}
                ref={(video) => {
                  if (video && cameraStream) {
                    video.srcObject = cameraStream;
                  }
                }}
              />
              <Box sx={{ 
                position: 'absolute',
                top: '50%',
                left: '50%',
                transform: 'translate(-50%, -50%)',
                width: '80px',
                height: '80px',
                border: '3px solid white',
                borderRadius: '50%',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                backgroundColor: 'rgba(255,255,255,0.2)',
                backdropFilter: 'blur(5px)'
              }}>
                <Box sx={{
                  width: '60px',
                  height: '60px',
                  border: '2px solid white',
                  borderRadius: '50%',
                  backgroundColor: 'rgba(255,255,255,0.3)'
                }} />
              </Box>
            </Box>
            <Typography variant="body2" sx={{ mt: 2, color: 'text.secondary' }}>
              {cameraField === 'idFrontPhoto' ? 'Kimlik ön yüzünü' : 'Kimlik arka yüzünü'} kameraya hizalayın ve fotoğraf çekin
            </Typography>
          </DialogContent>
          <DialogActions sx={{ p: 2, gap: 1 }}>
            <Button 
              onClick={closeCamera} 
              variant="outlined"
              color="error"
            >
              İptal
            </Button>
            <Button 
              onClick={capturePhoto}
              variant="contained"
              color="primary"
              startIcon={<span>📸</span>}
              disabled={!cameraStream}
            >
              Fotoğraf Çek
            </Button>
          </DialogActions>
        </Dialog>

        {/* Policy PDF Modal */}
        <Dialog
          open={policyPdfModalOpen}
          onClose={() => setPolicyPdfModalOpen(false)}
          maxWidth="lg"
          fullWidth
          fullScreen={isMobile}
          sx={{
            '& .MuiDialog-paper': {
              margin: isMobile ? 0 : 2,
              maxHeight: isMobile ? '100vh' : '90vh',
              borderRadius: isMobile ? 0 : 2,
            }
          }}
        >
          <DialogTitle sx={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center',
            backgroundColor: theme.palette.primary.main,
            color: 'white',
            padding: 2
          }}>
            <Typography variant="h6" component="div">
              📄 Poliçe Detayları - #{selectedPolicy?.policyNumber}
            </Typography>
            <IconButton
              onClick={() => setPolicyPdfModalOpen(false)}
              sx={{ color: 'white' }}
            >
              ✕
            </IconButton>
          </DialogTitle>
          <DialogContent sx={{ 
            padding: 0, 
            backgroundColor: '#f5f5f5',
            height: isMobile ? 'calc(100vh - 120px)' : '70vh'
          }}>
            {policyPdfUrl && (
              <iframe
                src={policyPdfUrl}
                width="100%"
                height="100%"
                style={{
                  border: 'none',
                  borderRadius: isMobile ? 0 : '0 0 8px 8px'
                }}
                title={`Poliçe - ${selectedPolicy?.policyNumber}`}
                onError={(e) => {
                  console.error('Policy PDF yükleme hatası:', e);
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
              onClick={() => window.open(policyPdfUrl, '_blank')}
              variant="outlined"
              startIcon={<span>🔗</span>}
            >
              Yeni Sekmede Aç
            </Button>
            <Button
              onClick={() => setPolicyPdfModalOpen(false)}
              variant="contained"
              color="primary"
            >
              Kapat
            </Button>
          </DialogActions>
        </Dialog>

        {/* PDF Modal */}
        <Dialog
          open={isPdfModalOpen}
          onClose={() => setIsPdfModalOpen(false)}
          maxWidth="lg"
          fullWidth
          fullScreen={isMobile}
          sx={{
            '& .MuiDialog-paper': {
              margin: isMobile ? 0 : 2,
              maxHeight: isMobile ? '100vh' : '90vh',
              borderRadius: isMobile ? 0 : 2,
            }
          }}
        >
          <DialogTitle sx={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center',
            backgroundColor: theme.palette.primary.main,
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
            height: isMobile ? 'calc(100vh - 120px)' : '70vh'
          }}>
            {pdfUrl && (
              <iframe
                src={pdfUrl}
                width="100%"
                height="100%"
                style={{
                  border: 'none',
                  borderRadius: isMobile ? 0 : '0 0 8px 8px'
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

        {/* Document Details Modal */}
        <Dialog
          open={documentDetailsModalOpen}
          onClose={() => setDocumentDetailsModalOpen(false)}
          maxWidth="md"
          fullWidth
          fullScreen={isMobile}
          sx={{
            '& .MuiDialog-paper': {
              margin: isMobile ? 0 : 2,
              maxHeight: isMobile ? '100vh' : '80vh',
              borderRadius: isMobile ? 0 : 2,
            }
          }}
        >
          <DialogTitle sx={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center',
            backgroundColor: theme.palette.primary.main,
            color: 'white',
            padding: 2
          }}>
            <Typography variant="h6" component="div">
              📄 Doküman Detayları - {selectedDocument?.name}
            </Typography>
            <IconButton
              onClick={() => setDocumentDetailsModalOpen(false)}
              sx={{ color: 'white' }}
            >
              ✕
            </IconButton>
          </DialogTitle>
          <DialogContent sx={{ 
            padding: 3, 
            backgroundColor: '#f5f5f5'
          }}>
            {selectedDocument && (
              <Box>
                {/* Document Info */}
                <Box sx={{ mb: 3 }}>
                  <Typography variant="h6" gutterBottom sx={{ color: theme.palette.primary.main }}>
                    Doküman Bilgileri
                  </Typography>
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" fontWeight={600}>Dosya Adı:</Typography>
                      <Typography variant="body2">{selectedDocument.name}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" fontWeight={600}>Dosya Türü:</Typography>
                      <Typography variant="body2">{selectedDocument.type}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" fontWeight={600}>Dosya Boyutu:</Typography>
                      <Typography variant="body2">{selectedDocument.size}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="body2" fontWeight={600}>Yükleme Tarihi:</Typography>
                      <Typography variant="body2">{selectedDocument.date}</Typography>
                    </Box>
                  </Box>
                </Box>

                {/* Document Preview */}
                <Box sx={{ mb: 3 }}>
                  <Typography variant="h6" gutterBottom sx={{ color: theme.palette.primary.main }}>
                    Doküman Önizleme
                  </Typography>
                  <Box sx={{ 
                    border: `1px solid ${theme.palette.divider}`,
                    borderRadius: 2,
                    backgroundColor: 'white',
                    minHeight: '400px',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                  }}>
                    {selectedDocument.url ? (
                      <iframe
                        src={selectedDocument.url.startsWith('http') ? selectedDocument.url : `http://localhost:5000/api/Document/serve/${selectedDocument.url.replace('/uploads/', 'uploads/')}`}
                        width="100%"
                        height="400px"
                        style={{
                          border: 'none',
                          borderRadius: '8px'
                        }}
                        title={selectedDocument.name}
                        onError={(e) => {
                          console.error('Document preview yükleme hatası:', e);
                        }}
                      />
                    ) : (
                      <Typography variant="body2" color="text.secondary">
                        Doküman önizlemesi mevcut değil
                      </Typography>
                    )}
                  </Box>
                </Box>
              </Box>
            )}
          </DialogContent>
          <DialogActions sx={{ 
            backgroundColor: '#f5f5f5',
            padding: 2,
            gap: 1
          }}>
            {selectedDocument?.url && (
              <Button
                onClick={() => {
                  const url = selectedDocument.url.startsWith('http') 
                    ? selectedDocument.url 
                    : `http://localhost:5000/api/Document/serve/${selectedDocument.url.replace('/uploads/', 'uploads/')}`;
                  window.open(url, '_blank');
                }}
                variant="outlined"
                startIcon={<span>🔗</span>}
              >
                Yeni Sekmede Aç
              </Button>
            )}
            <Button
              onClick={() => selectedDocument && handleDocumentDownload(selectedDocument.id)}
              variant="outlined"
              startIcon={<span>📥</span>}
            >
              İndir
            </Button>
            <Button
              onClick={() => setDocumentDetailsModalOpen(false)}
              variant="contained"
              color="primary"
            >
              Kapat
            </Button>
          </DialogActions>
        </Dialog>

        {/* Profile Settings Modal */}
        <Dialog
          open={profileModalOpen}
          onClose={() => setProfileModalOpen(false)}
          maxWidth="md"
          fullWidth
          PaperProps={{
            sx: {
              borderRadius: 2,
              boxShadow: '0 10px 30px rgba(0,0,0,0.1)',
            }
          }}
        >
          <DialogTitle sx={{
            backgroundColor: theme.palette.primary.main,
            color: 'white',
            display: 'flex',
            alignItems: 'center',
            gap: 1,
            fontWeight: 600
          }}>
            <Edit />
            Profil Ayarları
          </DialogTitle>
          
          <DialogContent sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
              {/* Kişisel Bilgiler */}
              <Box>
                <Typography variant="h6" gutterBottom sx={{ color: theme.palette.primary.main, fontWeight: 600 }}>
                  Kişisel Bilgiler
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={12} sm={6}>
                    <TextField
                      fullWidth
                      label="Ad Soyad"
                      value={profileData.name}
                      onChange={(e) => setProfileData({ ...profileData, name: e.target.value })}
                      variant="outlined"
                      size="small"
                    />
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <TextField
                      fullWidth
                      label="E-posta"
                      type="email"
                      value={profileData.email}
                      onChange={(e) => setProfileData({ ...profileData, email: e.target.value })}
                      variant="outlined"
                      size="small"
                    />
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <TextField
                      fullWidth
                      label="Telefon"
                      value={profileData.phone}
                      onChange={(e) => setProfileData({ ...profileData, phone: e.target.value })}
                      variant="outlined"
                      size="small"
                    />
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <TextField
                      fullWidth
                      label="Adres"
                      value={profileData.address}
                      onChange={(e) => setProfileData({ ...profileData, address: e.target.value })}
                      variant="outlined"
                      size="small"
                    />
                  </Grid>
                </Grid>
              </Box>

              {/* Şifre Değiştirme */}
              <Box>
                <Typography variant="h6" gutterBottom sx={{ color: theme.palette.primary.main, fontWeight: 600 }}>
                  Şifre Değiştir (İsteğe Bağlı)
                </Typography>
                <Grid container spacing={2}>
                  <Grid item xs={12} sm={6}>
                    <TextField
                      fullWidth
                      label="Yeni Şifre"
                      type="password"
                      value={profileData.password}
                      onChange={(e) => setProfileData({ ...profileData, password: e.target.value })}
                      variant="outlined"
                      size="small"
                      placeholder="Şifre değiştirmek istemiyorsanız boş bırakın"
                    />
                  </Grid>
                  <Grid item xs={12} sm={6}>
                    <TextField
                      fullWidth
                      label="Şifre Tekrar"
                      type="password"
                      value={profileData.confirmPassword}
                      onChange={(e) => setProfileData({ ...profileData, confirmPassword: e.target.value })}
                      variant="outlined"
                      size="small"
                      placeholder="Şifreyi tekrar girin"
                    />
                  </Grid>
                </Grid>
              </Box>
            </Box>
          </DialogContent>
          
          <DialogActions sx={{ p: 3, backgroundColor: '#f5f5f5', gap: 1 }}>
            <Button
              onClick={() => setProfileModalOpen(false)}
              variant="outlined"
              disabled={loading}
            >
              İptal
            </Button>
            <Button
              onClick={handleProfileUpdate}
              variant="contained"
              color="primary"
              disabled={loading}
              startIcon={loading ? <CircularProgress size={20} /> : <Edit />}
            >
              {loading ? 'Güncelleniyor...' : 'Profili Güncelle'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Poliçe Seçim Modal */}
        <Dialog
          open={policySelectionModalOpen}
          onClose={() => setPolicySelectionModalOpen(false)}
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
            textAlign: 'center', 
            pb: 2,
            backgroundColor: '#f8f9fa',
            borderBottom: '1px solid #e9ecef'
          }}>
            <Typography variant="h5" component="div" sx={{ fontWeight: 600, color: '#2c3e50' }}>
              Olay Bildirimi - Poliçe Seçimi
            </Typography>
            <Typography variant="body2" sx={{ color: '#6c757d', mt: 1 }}>
              Olay bildirimi için aktif poliçenizi seçin
            </Typography>
          </DialogTitle>
          
          <DialogContent sx={{ p: 3 }}>
            <Grid container spacing={2}>
              {policies.filter(policy => {
                const endDate = new Date(policy.endDate);
                const today = new Date();
                return endDate >= today;
              }).map((policy) => (
                <Grid item xs={12} sm={6} key={policy.policyId}>
                  <Card 
                    sx={{ 
                      cursor: 'pointer',
                      border: selectedPolicyForIncident?.policyId === policy.policyId ? '2px solid #1976d2' : '1px solid #e0e0e0',
                      '&:hover': {
                        borderColor: '#1976d2',
                        boxShadow: '0 4px 12px rgba(25, 118, 210, 0.15)'
                      },
                      transition: 'all 0.2s ease-in-out'
                    }}
                    onClick={() => setSelectedPolicyForIncident(policy)}
                  >
                    <CardContent>
                      <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                        <Chip 
                          label={policy.offer?.insuranceTypeName || 'Sigorta'} 
                          color="primary" 
                          size="small"
                          sx={{ mr: 1 }}
                        />
                        <Chip 
                          label="Aktif" 
                          color="success" 
                          size="small"
                        />
                      </Box>
                      
                      <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
                        {policy.policyNumber}
                      </Typography>
                      
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                        <strong>Başlangıç:</strong> {new Date(policy.startDate).toLocaleDateString('tr-TR')}
                      </Typography>
                      
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                        <strong>Bitiş:</strong> {new Date(policy.endDate).toLocaleDateString('tr-TR')}
                      </Typography>
                      
                      <Typography variant="body2" color="text.secondary">
                        <strong>Prim:</strong> ₺{policy.totalPremium?.toLocaleString('tr-TR') || '0'}
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
            
            {policies.filter(policy => {
              const endDate = new Date(policy.endDate);
              const today = new Date();
              return endDate >= today;
            }).length === 0 && (
              <Box sx={{ textAlign: 'center', py: 4 }}>
                <Typography variant="h6" color="text.secondary">
                  Aktif poliçeniz bulunmamaktadır
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                  Olay bildirimi için önce bir poliçe satın almanız gerekmektedir.
                </Typography>
              </Box>
            )}
          </DialogContent>
          
          <DialogActions sx={{ p: 3, backgroundColor: '#f8f9fa', gap: 1 }}>
            <Button 
              onClick={() => setPolicySelectionModalOpen(false)}
              variant="outlined"
              size="large"
            >
              İptal
            </Button>
            <Button 
              onClick={() => {
                if (selectedPolicyForIncident) {
                  setPolicySelectionModalOpen(false);
                  setIncidentModalOpen(true);
                  setIncidentFormData({
                    ...incidentFormData,
                    policyId: selectedPolicyForIncident.policyId.toString(),
                    contactName: customerInfo?.name || '',
                    contactEmail: customerInfo?.email || '',
                    contactPhone: customerInfo?.phone || '',
                    contactAddress: customerInfo?.address || ''
                  });
                }
              }}
              variant="contained"
              size="large"
              disabled={!selectedPolicyForIncident}
              sx={{ minWidth: 120 }}
            >
              Bu Poliçe ile Devam Et
            </Button>
          </DialogActions>
        </Dialog>

        {/* Olay Formu Modal */}
        <Dialog
          open={incidentModalOpen}
          onClose={() => setIncidentModalOpen(false)}
          maxWidth="lg"
          fullWidth
          disableEnforceFocus
          disableAutoFocus={false}
          PaperProps={{
            sx: {
              borderRadius: 2,
              boxShadow: '0 8px 32px rgba(0,0,0,0.12)',
              maxHeight: '90vh'
            }
          }}
        >
          <DialogTitle sx={{ 
            textAlign: 'center', 
            pb: 2,
            backgroundColor: '#f8f9fa',
            borderBottom: '1px solid #e9ecef'
          }}>
            <Typography variant="h5" component="div" sx={{ fontWeight: 600, color: '#2c3e50' }}>
              Olay Bildirimi
            </Typography>
            <Typography variant="body2" sx={{ color: '#6c757d', mt: 1 }}>
              {selectedPolicyForIncident?.offer?.insuranceTypeName || 'Sigorta'} - {selectedPolicyForIncident?.policyNumber}
            </Typography>
          </DialogTitle>
          
          <DialogContent sx={{ p: 3 }}>
            <Stepper activeStep={incidentStep} sx={{ mb: 4 }}>
              <Step>
                <StepLabel>Olay Bildirimi</StepLabel>
              </Step>
              <Step>
                <StepLabel>Belgeler</StepLabel>
              </Step>
              <Step>
                <StepLabel>Özet</StepLabel>
              </Step>
            </Stepper>

            {incidentStep === 0 && (
              <Box>
                <Typography variant="h6" sx={{ mb: 3, color: '#2c3e50' }}>
                  Olay Bildirimi
                </Typography>
                
                {/* Müşteri Bilgileri - Otomatik Doldurulmuş */}
                <Box sx={{ mb: 4, p: 3, backgroundColor: '#f8f9fa', borderRadius: 2, border: '1px solid #e9ecef' }}>
                  <Typography variant="subtitle1" sx={{ mb: 2, fontWeight: 'bold', color: '#495057' }}>
                    📋 Müşteri Bilgileri
                  </Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={12} md={6}>
                      <Typography variant="body2" color="text.secondary">Ad Soyad:</Typography>
                      <Typography variant="body1" sx={{ fontWeight: 'medium' }}>
                        {user?.name || 'Yükleniyor...'}
                      </Typography>
                    </Grid>
                    <Grid item xs={12} md={6}>
                      <Typography variant="body2" color="text.secondary">E-posta:</Typography>
                      <Typography variant="body1" sx={{ fontWeight: 'medium' }}>
                        {user?.email || 'Yükleniyor...'}
                      </Typography>
                    </Grid>
                    <Grid item xs={12} md={6}>
                      <Typography variant="body2" color="text.secondary">Telefon:</Typography>
                      <Typography variant="body1" sx={{ fontWeight: 'medium' }}>
                        {customerInfo?.phone || 'Belirtilmemiş'}
                      </Typography>
                    </Grid>
                    <Grid item xs={12} md={6}>
                      <Typography variant="body2" color="text.secondary">Adres:</Typography>
                      <Typography variant="body1" sx={{ fontWeight: 'medium' }}>
                        {customerInfo?.address || 'Belirtilmemiş'}
                      </Typography>
                    </Grid>
                  </Grid>
                </Box>
                
                {/* Olay Detayları */}
                <Grid container spacing={3}>
                  <Grid item xs={12} md={6}>
                    <TextField
                      select
                      fullWidth
                      required
                      label="Olay Türü *"
                      value={incidentFormData.incidentType}
                      onChange={handleIncidentTypeChange}
                      SelectProps={{
                        native: false,
                        MenuProps: {
                          disablePortal: false,
                          PaperProps: {
                            style: {
                              maxHeight: 300,
                              zIndex: 99999
                            }
                          }
                        }
                      }}
                    >
                      {getOfferInsuranceTypeName(selectedPolicyForIncident?.offer) === 'Trafik Sigortası' && [
                        <MenuItem key="araba-kazasi" value="Araba Kazası">Araba Kazası</MenuItem>,
                        <MenuItem key="hirsizlik" value="Hırsızlık">Hırsızlık</MenuItem>,
                        <MenuItem key="vandalizm" value="Vandalizm">Vandalizm</MenuItem>,
                        <MenuItem key="cam-kirilmasi" value="Cam Kırılması">Cam Kırılması</MenuItem>,
                        <MenuItem key="dogal-afet" value="Doğal Afet">Doğal Afet</MenuItem>
                      ]}
                      {getOfferInsuranceTypeName(selectedPolicyForIncident?.offer) === 'Konut Sigortası' && [
                        <MenuItem key="yangin" value="Yangın">Yangın</MenuItem>,
                        <MenuItem key="su-baskini" value="Su Baskını">Su Baskını</MenuItem>,
                        <MenuItem key="hirsizlik" value="Hırsızlık">Hırsızlık</MenuItem>,
                        <MenuItem key="dogal-afet" value="Doğal Afet">Doğal Afet</MenuItem>,
                        <MenuItem key="elektrik-arizasi" value="Elektrik Arızası">Elektrik Arızası</MenuItem>
                      ]}
                      {getOfferInsuranceTypeName(selectedPolicyForIncident?.offer) === 'Sağlık Sigortası' && [
                        <MenuItem key="hastane" value="Hastane Masrafları">Hastane Masrafları</MenuItem>,
                        <MenuItem key="ameliyat" value="Ameliyat">Ameliyat</MenuItem>,
                        <MenuItem key="ilac" value="İlaç Masrafları">İlaç Masrafları</MenuItem>,
                        <MenuItem key="acil" value="Acil Müdahale">Acil Müdahale</MenuItem>,
                        <MenuItem key="rehabilitasyon" value="Rehabilitasyon">Rehabilitasyon</MenuItem>
                      ]}
                      {getOfferInsuranceTypeName(selectedPolicyForIncident?.offer) === 'Seyahat Sigortası' && [
                        <MenuItem key="ucus-iptal" value="Uçuş İptali">Uçuş İptali</MenuItem>,
                        <MenuItem key="bagaj" value="Bagaj Kaybı">Bagaj Kaybı</MenuItem>,
                        <MenuItem key="saglik-acil" value="Sağlık Acil Durumu">Sağlık Acil Durumu</MenuItem>,
                        <MenuItem key="seyahat-iptal" value="Seyahat İptali">Seyahat İptali</MenuItem>,
                        <MenuItem key="pasaport" value="Pasaport Kaybı">Pasaport Kaybı</MenuItem>
                      ]}
                      {getOfferInsuranceTypeName(selectedPolicyForIncident?.offer) === 'Hayat Sigortası' && [
                        <MenuItem key="vefat" value="Vefat">Vefat</MenuItem>,
                        <MenuItem key="maluliyet" value="Maluliyet">Maluliyet</MenuItem>,
                        <MenuItem key="kritik-hastalik" value="Kritik Hastalık">Kritik Hastalık</MenuItem>,
                        <MenuItem key="kaza-yaralanma" value="Kaza Sonucu Yaralanma">Kaza Sonucu Yaralanma</MenuItem>,
                        <MenuItem key="is-goremezlik" value="İş Göremezlik">İş Göremezlik</MenuItem>
                      ]}
                      {getOfferInsuranceTypeName(selectedPolicyForIncident?.offer) === 'İş Yeri Sigortası' && [
                        <MenuItem key="is-kazasi" value="İş Kazası">İş Kazası</MenuItem>,
                        <MenuItem key="meslek-hastaligi" value="Meslek Hastalığı">Meslek Hastalığı</MenuItem>,
                        <MenuItem key="is-yeri-yangini" value="İş Yeri Yangını">İş Yeri Yangını</MenuItem>,
                        <MenuItem key="makine-arizasi" value="Makine Arızası">Makine Arızası</MenuItem>,
                        <MenuItem key="elektrik-kazasi" value="Elektrik Kazası">Elektrik Kazası</MenuItem>,
                        <MenuItem key="kimyasal-kaza" value="Kimyasal Kaza">Kimyasal Kaza</MenuItem>,
                        <MenuItem key="yapisal-hasar" value="Yapısal Hasar">Yapısal Hasar</MenuItem>,
                        <MenuItem key="hirsizlik" value="Hırsızlık">Hırsızlık</MenuItem>,
                        <MenuItem key="vandalizm" value="Vandalizm">Vandalizm</MenuItem>,
                        <MenuItem key="dogal-afet" value="Doğal Afet">Doğal Afet</MenuItem>
                      ]}
                      {getOfferInsuranceTypeName(selectedPolicyForIncident?.offer) === 'Bilinmeyen Sigorta Türü' && [
                        <MenuItem key="genel" value="Genel Olay">Genel Olay</MenuItem>,
                        <MenuItem key="hirsizlik-genel" value="Hırsızlık">Hırsızlık</MenuItem>,
                        <MenuItem key="vandalizm-genel" value="Vandalizm">Vandalizm</MenuItem>,
                        <MenuItem key="dogal-afet-genel" value="Doğal Afet">Doğal Afet</MenuItem>,
                        <MenuItem key="diger" value="Diğer">Diğer</MenuItem>
                      ]}
                    </TextField>
                      
                    {/* Poliçe türüne göre açıklayıcı metin */}
                    {getOfferInsuranceTypeName(selectedPolicyForIncident?.offer) !== 'Bilinmeyen Sigorta Türü' && (
                      <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
                        💡 {getOfferInsuranceTypeName(selectedPolicyForIncident?.offer)} için uygun olay türlerini seçin
                      </Typography>
                    )}
                  </Grid>
                  
                  <Grid item xs={12} md={6}>
                    <TextField
                      fullWidth
                      label="Olay Tarihi *"
                      type="date"
                      value={incidentFormData.incidentDate}
                      onChange={(e) => setIncidentFormData({...incidentFormData, incidentDate: e.target.value})}
                      InputLabelProps={{ shrink: true }}
                    />
                  </Grid>
                  
                  <Grid item xs={12}>
                    <TextField
                      fullWidth
                      label="Olay Açıklaması *"
                      multiline
                      rows={4}
                      value={incidentFormData.description}
                      onChange={(e) => setIncidentFormData({...incidentFormData, description: e.target.value})}
                      placeholder="Olayı detaylı bir şekilde açıklayın..."
                    />
                  </Grid>
                </Grid>
              </Box>
            )}

            {incidentStep === 1 && (
              <Box>
                <Typography variant="h6" sx={{ mb: 3, color: '#2c3e50' }}>
                  Belgeler (Maksimum 16 adet)
                </Typography>
                
                <Box
                  sx={{
                    border: '2px dashed #ccc',
                    borderRadius: 2,
                    p: 4,
                    textAlign: 'center',
                    mb: 3,
                    backgroundColor: '#f9f9f9',
                    '&:hover': {
                      backgroundColor: '#f0f0f0',
                      borderColor: '#1976d2'
                    }
                  }}
                  onClick={() => {
                    const input = document.createElement('input');
                    input.type = 'file';
                    input.multiple = true;
                    input.accept = '.pdf,.jpg,.jpeg,.png,.doc,.docx,.xls,.xlsx';
                    input.onchange = (e) => {
                      const files = Array.from((e.target as HTMLInputElement).files || []);
                      if (files.length > 0) {
                        const newFiles = [...uploadedFiles, ...files].slice(0, 16); // Maksimum 16 dosya
                        setUploadedFiles(newFiles);
                      }
                    };
                    input.click();
                  }}
                  onDragOver={(e) => {
                    e.preventDefault();
                    e.currentTarget.style.backgroundColor = '#e3f2fd';
                    e.currentTarget.style.borderColor = '#1976d2';
                  }}
                  onDragLeave={(e) => {
                    e.preventDefault();
                    e.currentTarget.style.backgroundColor = '#f9f9f9';
                    e.currentTarget.style.borderColor = '#ccc';
                  }}
                  onDrop={(e) => {
                    e.preventDefault();
                    e.currentTarget.style.backgroundColor = '#f9f9f9';
                    e.currentTarget.style.borderColor = '#ccc';
                    
                    const files = Array.from(e.dataTransfer.files);
                    if (files.length > 0) {
                      const newFiles = [...uploadedFiles, ...files].slice(0, 16); // Maksimum 16 dosya
                      setUploadedFiles(newFiles);
                    }
                  }}
                >
                  <Typography variant="h6" sx={{ mb: 2 }}>
                    Belgeleri buraya sürükleyin veya tıklayarak seçin
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    Desteklenen formatlar: JPG, PNG, PDF, DOC, DOCX, XLS, XLSX
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Maksimum dosya boyutu: 10MB | Toplam: {uploadedFiles.length}/16
                  </Typography>
                </Box>
                
                {uploadedFiles.length > 0 && (
                  <Box>
                    <Typography variant="subtitle1" sx={{ mb: 2 }}>
                      Yüklenen Belgeler:
                    </Typography>
                    <Grid container spacing={2}>
                      {uploadedFiles.map((file, index) => (
                        <Grid item xs={12} sm={6} md={4} key={index}>
                          <Card sx={{ p: 2 }}>
                            <Typography variant="body2" sx={{ fontWeight: 600 }}>
                              {file.name}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {(file.size / 1024 / 1024).toFixed(2)} MB
                            </Typography>
                            <IconButton
                              size="small"
                              onClick={() => {
                                const newFiles = uploadedFiles.filter((_, i) => i !== index);
                                setUploadedFiles(newFiles);
                              }}
                              sx={{ ml: 1 }}
                            >
                              <Delete />
                            </IconButton>
                          </Card>
                        </Grid>
                      ))}
                    </Grid>
                  </Box>
                )}
              </Box>
            )}

            {incidentStep === 2 && (
              <Box>
                <Typography variant="h6" sx={{ mb: 3, color: '#2c3e50' }}>
                  Özet ve Onay
                </Typography>
                
                <Card sx={{ p: 3, mb: 3 }}>
                  <Typography variant="h6" sx={{ mb: 2 }}>
                    Poliçe Bilgileri
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 1 }}>
                    <strong>Poliçe No:</strong> {selectedPolicyForIncident?.policyNumber}
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 1 }}>
                    <strong>Sigorta Türü:</strong> {selectedPolicyForIncident?.offer?.insuranceTypeName}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Geçerlilik:</strong> {new Date(selectedPolicyForIncident?.startDate).toLocaleDateString('tr-TR')} - {new Date(selectedPolicyForIncident?.endDate).toLocaleDateString('tr-TR')}
                  </Typography>
                </Card>
                
                <Card sx={{ p: 3, mb: 3 }}>
                  <Typography variant="h6" sx={{ mb: 2 }}>
                    Olay Bilgileri
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 1 }}>
                    <strong>Olay Türü:</strong> {incidentFormData.incidentType}
                  </Typography>
                  <Typography variant="body2" sx={{ mb: 1 }}>
                    <strong>Olay Tarihi:</strong> {new Date(incidentFormData.incidentDate).toLocaleDateString('tr-TR')}
                  </Typography>
                  <Typography variant="body2">
                    <strong>Açıklama:</strong> {incidentFormData.description}
                  </Typography>
                </Card>
                
                <Card sx={{ p: 3 }}>
                  <Typography variant="h6" sx={{ mb: 2 }}>
                    Belgeler ({uploadedFiles.length} adet)
                  </Typography>
                  {uploadedFiles.length > 0 ? (
                    uploadedFiles.map((file, index) => (
                      <Typography key={index} variant="body2" sx={{ mb: 1 }}>
                        • {file.name} ({(file.size / 1024 / 1024).toFixed(2)} MB)
                      </Typography>
                    ))
                  ) : (
                    <Typography variant="body2" color="text.secondary">
                      Belge yüklenmedi
                    </Typography>
                  )}
                </Card>
              </Box>
            )}
          </DialogContent>
          
          <DialogActions sx={{ p: 3, backgroundColor: '#f8f9fa', gap: 1 }}>
            <Button 
              onClick={() => setIncidentModalOpen(false)}
              variant="outlined"
              size="large"
            >
              İptal
            </Button>
            
            {incidentStep > 0 && (
              <Button 
                onClick={() => setIncidentStep(incidentStep - 1)}
                variant="outlined"
                size="large"
              >
                Geri
              </Button>
            )}
            
            {incidentStep < 2 ? (
              <Button 
                onClick={() => setIncidentStep(incidentStep + 1)}
                variant="contained"
                size="large"
                disabled={
                  (incidentStep === 0 && (!incidentFormData.incidentType || !incidentFormData.incidentDate || !incidentFormData.description)) ||
                  (incidentStep === 1 && uploadedFiles.length === 0)
                }
              >
                İleri
              </Button>
            ) : (
              <Button 
                onClick={async () => {
                  try {
                    setIncidentLoading(true);
                    
                    // Olay formu verilerini hazırla
                    const claimData = {
                      policyId: parseInt(incidentFormData.policyId),
                      description: incidentFormData.description,
                      type: incidentFormData.incidentType,
                      incidentDate: incidentFormData.incidentDate ? new Date(incidentFormData.incidentDate) : new Date(),
                      // Araç sigortası için ek alanlar
                      vehiclePlate: incidentFormData.vehiclePlate,
                      vehicleBrand: incidentFormData.vehicleBrand,
                      vehicleModel: incidentFormData.vehicleModel,
                      vehicleYear: incidentFormData.vehicleYear,
                      accidentLocation: incidentFormData.accidentLocation,
                      accidentTime: incidentFormData.accidentTime,
                      // Konut sigortası için ek alanlar
                      propertyAddress: incidentFormData.propertyAddress,
                      propertyFloor: incidentFormData.propertyFloor,
                      propertyType: incidentFormData.propertyType,
                      // Sağlık sigortası için ek alanlar
                      patientName: incidentFormData.patientName,
                      patientAge: incidentFormData.patientAge,
                      patientGender: incidentFormData.patientGender,
                      patientTc: incidentFormData.patientTc,
                      hospitalName: incidentFormData.hospitalName,
                      doctorName: incidentFormData.doctorName,
                      // Seyahat sigortası için ek alanlar
                      travelStartDate: incidentFormData.travelStartDate,
                      travelEndDate: incidentFormData.travelEndDate,
                      travelCountry: incidentFormData.travelCountry,
                      travelPurpose: incidentFormData.travelPurpose,
                      // Hayat sigortası için ek alanlar
                      insuredName: incidentFormData.insuredName,
                      insuredAge: incidentFormData.insuredAge,
                      insuredGender: incidentFormData.insuredGender,
                      insuredTc: incidentFormData.insuredTc,
                      // İş yeri sigortası için ek alanlar
                      workplaceName: incidentFormData.workplaceName,
                      workplaceAddress: incidentFormData.workplaceAddress,
                      employeeCount: incidentFormData.employeeCount,
                      businessSector: incidentFormData.businessSector,
                      sgkNumber: incidentFormData.sgkNumber,
                      safetyOfficer: incidentFormData.safetyOfficer,
                      emergencyContact: incidentFormData.emergencyContact
                    };

                    console.log('📝 Creating claim with data:', claimData);
                    
                    // Olay oluştur
                    const createdClaim = await apiService.createClaim(claimData);
                    console.log('✅ Claim created successfully:', createdClaim);
                    
                    // Belgeleri yükle
                    if (uploadedFiles.length > 0) {
                      console.log('📄 Uploading documents:', uploadedFiles.length);
                      await apiService.uploadIncidentDocuments(uploadedFiles, createdClaim.claimId);
                      console.log('✅ Documents uploaded successfully');
                    }
                    
                    setIncidentLoading(false);
                    setIncidentSuccess(true);
                    setIncidentModalOpen(false);
                    
                    // Form sıfırla
                    setIncidentFormData({
                      policyId: '',
                      incidentType: '',
                      description: '',
                      incidentDate: '',
                      contactName: '',
                      contactPhone: '',
                      contactEmail: '',
                      contactAddress: '',
                      vehiclePlate: '',
                      vehicleBrand: '',
                      vehicleModel: '',
                      vehicleYear: '',
                      accidentLocation: '',
                      accidentTime: '',
                      propertyAddress: '',
                      propertyFloor: '',
                      propertyType: '',
                      patientName: '',
                      patientAge: '',
                      patientGender: '',
                      patientTc: '',
                      hospitalName: '',
                      doctorName: '',
                      travelStartDate: '',
                      travelEndDate: '',
                      travelCountry: '',
                      travelPurpose: '',
                      insuredName: '',
                      insuredAge: '',
                      insuredGender: '',
                      insuredTc: '',
                      workplaceName: '',
                      workplaceAddress: '',
                      employeeCount: '',
                      businessSector: '',
                      sgkNumber: '',
                      safetyOfficer: '',
                      emergencyContact: ''
                    });
                    setUploadedFiles([]);
                    setIncidentStep(0);
                    
                    // Başarı mesajı göster
                    alert('Olay bildirimi başarıyla gönderildi!');
                    
                  } catch (error: any) {
                    console.error('❌ Error submitting incident form:', error);
                    setIncidentLoading(false);
                    alert(`Hata: ${error.message}`);
                  }
                }}
                variant="contained"
                size="large"
                disabled={incidentLoading}
                startIcon={incidentLoading ? <CircularProgress size={20} /> : <Send />}
              >
                {incidentLoading ? 'Gönderiliyor...' : 'Olay Bildirimini Gönder'}
              </Button>
            )}
          </DialogActions>
        </Dialog>

        {/* Edit Claim Modal */}
        <Dialog 
          open={editClaimModalOpen} 
          onClose={() => setEditClaimModalOpen(false)}
          maxWidth="md"
          fullWidth
        >
          <DialogTitle>Olay Bildirimini Düzenle</DialogTitle>
          <DialogContent>
            <Box sx={{ pt: 2, display: 'flex', flexDirection: 'column', gap: 3 }}>
              {/* Olay Türü - Sadece Görüntüleme */}
              <TextField
                label="Olay Türü"
                value={selectedClaim?.type || ''}
                fullWidth
                disabled
                helperText="Olay türü değiştirilemez"
              />
              
              {/* Açıklama */}
              <TextField
                label="Açıklama"
                value={editClaimData.description}
                onChange={(e) => setEditClaimData({ ...editClaimData, description: e.target.value })}
                fullWidth
                multiline
                rows={4}
                required
              />

              {/* Mevcut Belgeler */}
              <Box>
                <Typography variant="subtitle1" sx={{ mb: 2, fontWeight: 600 }}>
                  Yüklü Belgeler
                </Typography>
                {claimDocuments.length === 0 ? (
                  <Typography variant="body2" color="text.secondary">
                    Henüz belge yüklenmemiş
                  </Typography>
                ) : (
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                    {claimDocuments.map((doc) => (
                      <Box 
                        key={doc.id}
                        sx={{ 
                          display: 'flex', 
                          alignItems: 'center', 
                          justifyContent: 'space-between',
                          p: 1.5,
                          border: '1px solid',
                          borderColor: 'divider',
                          borderRadius: 1
                        }}
                      >
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Description color="primary" />
                          <Typography variant="body2">{doc.fileName}</Typography>
                        </Box>
                        <IconButton 
                          size="small" 
                          color="error"
                          onClick={async () => {
                            if (window.confirm('Bu belgeyi silmek istediğinizden emin misiniz?')) {
                              try {
                                await apiService.deleteDocument(doc.id);
                                // Refresh documents
                                const allDocs = await apiService.getMyDocuments();
                                const claimDocs = allDocs.filter((d: any) => d.claimId === selectedClaim?.claimId);
                                setClaimDocuments(claimDocs);
                                alert('Belge başarıyla silindi');
                              } catch (error: any) {
                                alert(error.message || 'Belge silinemedi');
                              }
                            }
                          }}
                        >
                          <Delete />
                        </IconButton>
                      </Box>
                    ))}
                  </Box>
                )}
              </Box>

              {/* Yeni Belge Yükleme */}
              <Box>
                <Typography variant="subtitle1" sx={{ mb: 2, fontWeight: 600 }}>
                  Yeni Belge Ekle
                </Typography>
                <Button
                  variant="outlined"
                  component="label"
                  startIcon={<Add />}
                  fullWidth
                >
                  Dosya Seç
                  <input
                    type="file"
                    hidden
                    multiple
                    accept="image/*,.pdf,.doc,.docx"
                    onChange={(e) => {
                      if (e.target.files) {
                        setNewClaimFiles([...newClaimFiles, ...Array.from(e.target.files)]);
                      }
                    }}
                  />
                </Button>
                
                {/* Seçilen Yeni Dosyalar */}
                {newClaimFiles.length > 0 && (
                  <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 1 }}>
                    {newClaimFiles.map((file, index) => (
                      <Box 
                        key={index}
                        sx={{ 
                          display: 'flex', 
                          alignItems: 'center', 
                          justifyContent: 'space-between',
                          p: 1.5,
                          border: '1px solid',
                          borderColor: 'success.main',
                          borderRadius: 1,
                          bgcolor: 'success.lighter'
                        }}
                      >
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Image color="success" />
                          <Typography variant="body2">{file.name}</Typography>
                        </Box>
                        <IconButton 
                          size="small" 
                          onClick={() => {
                            setNewClaimFiles(newClaimFiles.filter((_, i) => i !== index));
                          }}
                        >
                          <Close />
                        </IconButton>
                      </Box>
                    ))}
                  </Box>
                )}
              </Box>
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setEditClaimModalOpen(false)}>
              İptal
            </Button>
            <Button 
              onClick={async () => {
                try {
                  if (!selectedClaim) return;
                  
                  // Update description
                  await apiService.updateMyClaim(selectedClaim.claimId, {
                    description: editClaimData.description
                  });
                  
                  // Upload new files if any
                  if (newClaimFiles.length > 0) {
                    await apiService.uploadIncidentDocuments(newClaimFiles, selectedClaim.claimId);
                  }
                  
                  // Refresh claims list
                  const updatedClaims = await apiService.getMyClaims();
                  setClaims(updatedClaims);
                  
                  setEditClaimModalOpen(false);
                  alert('Olay bildirimi başarıyla güncellendi');
                } catch (error: any) {
                  alert(error.message || 'Olay bildirimi güncellenemedi');
                }
              }}
              variant="contained"
              color="primary"
            >
              Kaydet
            </Button>
          </DialogActions>
        </Dialog>
      </Container>
    </Box>
  );
};

export default CustomerDashboard;
