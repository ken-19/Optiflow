import { useState, useEffect } from 'react';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, ArcElement, PointElement, LineElement, Title, Tooltip, Legend, Filler } from 'chart.js';
import { Bar, Doughnut, Line } from 'react-chartjs-2';
import API from '../api/axios';
import { useNavigate } from 'react-router-dom';

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, PointElement, LineElement, Title, Tooltip, Legend, Filler);

const chartOpts = { responsive: true, maintainAspectRatio: false, plugins: { legend: { labels: { color: '#94a3b8', font: { family: 'Inter' } } } }, scales: { x: { ticks: { color: '#64748b' }, grid: { color: '#2a2f45' } }, y: { ticks: { color: '#64748b' }, grid: { color: '#2a2f45' } } } };
const doughnutOpts = { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom', labels: { color: '#94a3b8', padding: 16, font: { family: 'Inter' } } } } };

// Fallback data when API is unavailable
const fallbackData = { totalWorkOrders: 24, completedWorkOrders: 18, totalMaterials: 56, activeSchedules: 8, defectRate: 2.3, totalCost: 125400, lowStockItems: 3, openDefects: 4, workOrderStatusDistribution: { Pending: 3, InProgress: 3, Completed: 18 }, monthlyCosts: { '2026-01': 12000, '2026-02': 15000, '2026-03': 11000, '2026-04': 18000 }, defectSeverity: { Low: 8, Medium: 4, High: 2, Critical: 1 } };

import { useAuth } from '../contexts/AuthContext';
import SuperAdminDashboard from './SuperAdminDashboard';

export default function DashboardPage() {
  const { user } = useAuth();

  const isSuperAdmin = user?.roles?.includes('Super Admin');
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(!isSuperAdmin);
  const navigate = useNavigate();

  useEffect(() => {
    if (!isSuperAdmin) {
      API.get('/dashboard').then(r => setData(r.data)).catch(() => setData(fallbackData)).finally(() => setLoading(false));
    }
  }, [isSuperAdmin]);

  if (isSuperAdmin) return <SuperAdminDashboard />;
  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  const woChart = {
    labels: Object.keys(data.workOrderStatusDistribution || {}),
    datasets: [{ data: Object.values(data.workOrderStatusDistribution || {}), backgroundColor: ['#f59e0b', '#3b82f6', '#22c55e', '#ef4444', '#8b5cf6'], borderWidth: 0 }]
  };
  const costChart = {
    labels: Object.keys(data.monthlyCosts || {}),
    datasets: [{ label: 'Cost (₱)', data: Object.values(data.monthlyCosts || {}), backgroundColor: 'rgba(99,102,241,0.3)', borderColor: '#6366f1', borderWidth: 2, fill: true, tension: 0.4 }]
  };
  const defectChart = {
    labels: Object.keys(data.defectSeverity || {}),
    datasets: [{ data: Object.values(data.defectSeverity || {}), backgroundColor: ['#22c55e', '#f59e0b', '#f97316', '#ef4444'], borderWidth: 0 }]
  };
  const prodChart = {
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
    datasets: [{ label: 'Output', data: [42, 55, 48, 62, 58, 35], backgroundColor: '#6366f1', borderRadius: 6 }, { label: 'Target', data: [50, 50, 50, 50, 50, 50], backgroundColor: '#2a2f45', borderRadius: 6 }]
  };

  const canViewProduction = user?.roles?.some(r => ['Super Admin', 'Admin', 'Production Planner', 'Shop Floor Supervisor', 'Plant Manager'].includes(r));
  const canViewInventory = user?.roles?.some(r => ['Super Admin', 'Admin', 'Inventory Manager', 'Plant Manager'].includes(r));
  const canViewQuality = user?.roles?.some(r => ['Super Admin', 'Admin', 'Quality Control Inspector', 'Plant Manager'].includes(r));
  const canViewCosts = user?.roles?.some(r => ['Super Admin', 'Admin', 'Cost Accountant', 'Plant Manager'].includes(r));

  return (
    <>
      <div className="stats-grid">
        {canViewProduction && (
          <>
            <div className="card stat-card" style={{cursor: 'pointer'}} onClick={() => navigate('/work-orders')}><div className="stat-icon purple">📋</div><div className="stat-value">{data.totalWorkOrders}</div><div className="stat-label">Total Work Orders</div><div className="stat-change up">↑ 12% this month</div></div>
            <div className="card stat-card" style={{cursor: 'pointer'}} onClick={() => navigate('/work-orders')}><div className="stat-icon green">✅</div><div className="stat-value">{data.completedWorkOrders}</div><div className="stat-label">Completed</div><div className="stat-change up">↑ 8% this month</div></div>
            <div className="card stat-card" style={{cursor: 'pointer'}} onClick={() => navigate('/production')}><div className="stat-icon amber">🏭</div><div className="stat-value">{data.activeSchedules}</div><div className="stat-label">Active Schedules</div></div>
          </>
        )}
        {canViewInventory && (
          <div className="card stat-card" style={{cursor: 'pointer'}} onClick={() => navigate('/inventory')}><div className="stat-icon blue">📦</div><div className="stat-value">{data.totalMaterials}</div><div className="stat-label">Materials</div>{data.lowStockItems > 0 && <div className="stat-change down">⚠ {data.lowStockItems} low stock</div>}</div>
        )}
        {canViewQuality && (
          <div className="card stat-card" style={{cursor: 'pointer'}} onClick={() => navigate('/quality')}><div className="stat-icon red">🐛</div><div className="stat-value">{data.defectRate}%</div><div className="stat-label">Defect Rate</div></div>
        )}
      </div>
      <div className="charts-grid">
        {canViewProduction && (
          <>
            <div className="card chart-card"><div className="card-header"><span className="card-title">Work Order Status</span></div><div style={{ height: 260 }}><Doughnut data={woChart} options={doughnutOpts} /></div></div>
            <div className="card chart-card"><div className="card-header"><span className="card-title">Production Output</span></div><div style={{ height: 260 }}><Bar data={prodChart} options={chartOpts} /></div></div>
          </>
        )}
        {canViewCosts && (
          <div className="card chart-card"><div className="card-header"><span className="card-title">Monthly Costs</span></div><div style={{ height: 260 }}><Line data={costChart} options={chartOpts} /></div></div>
        )}
        {canViewQuality && (
          <div className="card chart-card"><div className="card-header"><span className="card-title">Defect Severity</span></div><div style={{ height: 260 }}><Doughnut data={defectChart} options={doughnutOpts} /></div></div>
        )}
      </div>
    </>
  );
}
