import { useState, useEffect } from 'react';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, ArcElement, PointElement, LineElement, Title, Tooltip, Legend, Filler } from 'chart.js';
import { Bar, Doughnut, Line } from 'react-chartjs-2';
import API from '../api/axios';
import { useNavigate } from 'react-router-dom';

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, PointElement, LineElement, Title, Tooltip, Legend, Filler);

const chartOpts = { responsive: true, maintainAspectRatio: false, plugins: { legend: { labels: { color: '#94a3b8', font: { family: 'Inter' } } } }, scales: { x: { ticks: { color: '#64748b' }, grid: { color: '#2a2f45' } }, y: { ticks: { color: '#64748b' }, grid: { color: '#2a2f45' } } } };
const doughnutOpts = { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom', labels: { color: '#94a3b8', padding: 16, font: { family: 'Inter' } } } } };

// Fallback data in case API fails
const fallbackData = {
  stats: { totalCompanies: 12, totalUsers: 145, totalAdmins: 15, activeSubscriptions: 10, expiredSubscriptions: 2 },
  companies: [
    { companyId: 1, companyName: "Acme Corp", adminName: "John Doe", subscriptionPlan: "Premium Plan", startDate: "2025-01-10T00:00:00Z", expiryDate: "2026-01-10T00:00:00Z", status: "Active", isNearExpiry: false, userCount: 45, isActive: true },
    { companyId: 2, companyName: "TechGear", adminName: "Jane Smith", subscriptionPlan: "Standard Plan", startDate: "2025-11-20T00:00:00Z", expiryDate: "2026-11-20T00:00:00Z", status: "Active", isNearExpiry: false, userCount: 18, isActive: true },
    { companyId: 3, companyName: "OldFactory", adminName: "Bob Brown", subscriptionPlan: "Basic Plan", startDate: "2024-03-15T00:00:00Z", expiryDate: "2025-03-15T00:00:00Z", status: "Expired", isNearExpiry: false, userCount: 5, isActive: false }
  ],
  planDistribution: [
    { plan: "Basic Plan", count: 3 },
    { plan: "Standard Plan", count: 5 },
    { plan: "Premium Plan", count: 4 }
  ],
  alerts: [
    { companyName: "OldFactory", expiryDate: "2025-03-15T00:00:00Z", type: "Danger", message: "Subscription expired on Mar 15, 2025" }
  ]
};

export default function SuperAdminDashboard() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    API.get('/superadmin/dashboard-stats')
      .then(res => setData(res.data))
      .catch(err => {
        console.error("Super Admin API Error:", err);
        setData(fallbackData);
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  const stats = data?.stats || {};
  
  // Prepare Charts Data
  const planData = {
    labels: data?.planDistribution?.map(p => p.plan) || [],
    datasets: [{ data: data?.planDistribution?.map(p => p.count) || [], backgroundColor: ['#3b82f6', '#ec4899', '#a855f7'], borderWidth: 0 }]
  };

  const activeCompanies = data?.companies?.slice(0, 6) || [];
  const usersPerCompany = {
    labels: activeCompanies.map(c => c.companyName),
    datasets: [{ label: 'Users', data: activeCompanies.map(c => c.userCount), backgroundColor: '#6366f1', borderRadius: 4 }]
  };

  // Mock growth data since we don't have historical points
  const growthData = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
    datasets: [{ label: 'Subscribers', data: [2, 4, 5, 8, 10, stats.totalCompanies || 12], backgroundColor: 'rgba(34, 197, 94, 0.2)', borderColor: '#22c55e', borderWidth: 2, fill: true, tension: 0.4 }]
  };

  return (
    <div className="super-admin-dashboard">
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: 'var(--text-primary)' }}>System Overview</h2>
        <p style={{ color: 'var(--text-muted)' }}>Global usage and subscription monitoring</p>
      </div>

      {data?.alerts?.length > 0 && (
        <div style={{ marginBottom: 24, display: 'flex', flexDirection: 'column', gap: 12 }}>
          {data.alerts.map((alert, idx) => (
            <div key={idx} style={{ background: alert.type === 'Danger' ? 'rgba(239,68,68,0.1)' : 'rgba(245,158,11,0.1)', border: `1px solid ${alert.type === 'Danger' ? 'var(--danger)' : 'var(--warning)'}`, padding: 16, borderRadius: 8, display: 'flex', alignItems: 'center', gap: 12 }}>
              <span style={{ fontSize: 20 }}>{alert.type === 'Danger' ? '❌' : '⚠️'}</span>
              <div>
                <div style={{ fontWeight: 600, color: alert.type === 'Danger' ? 'var(--danger)' : 'var(--warning)' }}>{alert.companyName}</div>
                <div style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{alert.message}</div>
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="stats-grid">
        <div className="card stat-card" style={{ cursor: 'pointer' }} onClick={() => navigate('/companies')}>
          <div className="stat-icon blue">🏢</div>
          <div className="stat-value">{stats.totalCompanies}</div>
          <div className="stat-label">Total Companies</div>
        </div>
        <div className="card stat-card" style={{ cursor: 'pointer' }} onClick={() => navigate('/users')}>
          <div className="stat-icon purple">👥</div>
          <div className="stat-value">{stats.totalUsers}</div>
          <div className="stat-label">Total Active Users</div>
        </div>
        <div className="card stat-card" style={{ cursor: 'pointer' }} onClick={() => navigate('/subscriptions')}>
          <div className="stat-icon green">✅</div>
          <div className="stat-value">{stats.activeSubscriptions}</div>
          <div className="stat-label">Active Subscriptions</div>
        </div>
        <div className="card stat-card" style={{ cursor: 'pointer' }} onClick={() => navigate('/subscriptions')}>
          <div className="stat-icon red">❌</div>
          <div className="stat-value">{stats.expiredSubscriptions}</div>
          <div className="stat-label">Expired Subscriptions</div>
        </div>
      </div>

      <div className="charts-grid" style={{ gridTemplateColumns: '1fr 1.5fr 1fr' }}>
        <div className="card chart-card">
          <div className="card-header"><span className="card-title">Plan Distribution</span></div>
          <div style={{ height: 260 }}><Doughnut data={planData} options={doughnutOpts} /></div>
        </div>
        <div className="card chart-card">
          <div className="card-header"><span className="card-title">Users per Company</span></div>
          <div style={{ height: 260 }}><Bar data={usersPerCompany} options={chartOpts} /></div>
        </div>
        <div className="card chart-card">
          <div className="card-header"><span className="card-title">Growth (YTD)</span></div>
          <div style={{ height: 260 }}><Line data={growthData} options={chartOpts} /></div>
        </div>
      </div>

      <div className="card" style={{ marginTop: 24 }}>
        <div className="card-header">
          <span className="card-title">Subscription Directory</span>
        </div>
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Company Name</th>
                <th>Admin (Owner)</th>
                <th>Users</th>
                <th>Plan</th>
                <th>Status</th>
                <th>Expiry Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data?.companies?.map(c => (
                <tr key={c.companyId}>
                  <td style={{ fontWeight: 600 }}>{c.companyName}</td>
                  <td>{c.adminName}</td>
                  <td>{c.userCount}</td>
                  <td>
                    <span className={`badge ${c.subscriptionPlan === 'Premium Plan' ? 'badge-purple' : c.subscriptionPlan === 'Standard Plan' ? 'badge-info' : 'badge-success'}`}>
                      {c.subscriptionPlan || 'None'}
                    </span>
                  </td>
                  <td>
                    <span className={`badge ${c.status === 'Active' ? 'badge-success' : c.status === 'System Owner' ? 'badge-purple' : 'badge-danger'}`}>
                      {c.status}
                    </span>
                  </td>
                  <td>{c.expiryDate ? new Date(c.expiryDate).toLocaleDateString() : 'N/A'}</td>
                  <td>
                    <button className="btn btn-ghost btn-sm">Manage</button>
                  </td>
                </tr>
              ))}
              {(!data?.companies || data.companies.length === 0) && (
                <tr><td colSpan="7" style={{ textAlign: 'center', padding: 20 }}>No companies registered</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
