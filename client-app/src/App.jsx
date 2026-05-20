import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import Layout from './components/Layout';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import ProductionPage from './pages/ProductionPage';
import WorkOrdersPage from './pages/WorkOrdersPage';
import InventoryPage from './pages/InventoryPage';
import MrpPage from './pages/MrpPage';
import QualityPage from './pages/QualityPage';
import CostingPage from './pages/CostingPage';
import UsersPage from './pages/UsersPage';
import ReportsPage from './pages/ReportsPage';
import SystemLogsPage from './pages/SystemLogsPage';
import CompaniesPage from './pages/CompaniesPage';
import SubscriptionsPage from './pages/SubscriptionsPage';
import AdminAccountsPage from './pages/AdminAccountsPage';
import BackupsPage from './pages/BackupsPage';
import PricingPage from './pages/PricingPage';
import PaymentSuccessPage from './pages/PaymentSuccessPage';

function ProtectedRoute({ children }) {
  const { user, loading } = useAuth();
  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;
  return user ? children : <Navigate to="/login" />;
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<ProtectedRoute><Layout /></ProtectedRoute>}>
            <Route index element={<Navigate to="/dashboard" />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="production" element={<ProductionPage />} />
            <Route path="work-orders" element={<WorkOrdersPage />} />
            <Route path="mrp" element={<MrpPage />} />
            <Route path="inventory" element={<InventoryPage />} />
            <Route path="quality" element={<QualityPage />} />
            <Route path="costing" element={<CostingPage />} />
            <Route path="users" element={<UsersPage />} />
            <Route path="reports" element={<ReportsPage />} />
            <Route path="system-logs" element={<SystemLogsPage />} />
            <Route path="companies" element={<CompaniesPage />} />
            <Route path="subscriptions" element={<SubscriptionsPage />} />
            <Route path="admins" element={<AdminAccountsPage />} />
            <Route path="backups" element={<BackupsPage />} />
            <Route path="pricing" element={<PricingPage />} />
            <Route path="payment/success" element={<PaymentSuccessPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/dashboard" />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
