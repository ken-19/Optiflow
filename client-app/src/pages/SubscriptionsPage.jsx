import { useState, useEffect } from 'react';
import API from '../api/axios';

const planColors = {
  'Premium Plan': 'badge-purple',
  'Standard Plan': 'badge-info',
  'Basic Plan': 'badge-success',
};

const statusColors = {
  'Active': 'badge-success',
  'Expired': 'badge-danger',
  'Expiring Soon': 'badge-warning',
};

export default function SubscriptionsPage() {
  const [subscriptions, setSubscriptions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [planFilter, setPlanFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');
  
  // Edit state
  const [editing, setEditing] = useState(null);
  const [editForm, setEditForm] = useState({ plan: '', expiryDate: '', isActive: true });
  const [saving, setSaving] = useState(false);

  const loadSubscriptions = () => {
    setLoading(true);
    API.get('/superadmin/subscriptions')
      .then(res => setSubscriptions(res.data))
      .catch(() => setSubscriptions([]))
      .finally(() => setLoading(false));
  };

  useEffect(loadSubscriptions, []);

  const handleManage = (sub) => {
    setEditing(sub);
    setEditForm({
      plan: sub.plan,
      expiryDate: sub.expiryDate ? sub.expiryDate.split('T')[0] : '',
      isActive: sub.autoRenew
    });
  };

  const handleSave = async (e) => {
    e.preventDefault();
    if (!editing) return;
    setSaving(true);
    try {
      await API.put(`/superadmin/subscriptions/${editing.subscriptionId}`, {
        plan: editForm.plan,
        expiryDate: editForm.expiryDate,
        isActive: editForm.isActive
      });
      setEditing(null);
      loadSubscriptions();
    } catch (err) {
      alert('Failed to update subscription');
    } finally {
      setSaving(false);
    }
  };

  if (loading && subscriptions.length === 0) return <div className="page-loading"><div className="spinner"></div></div>;

  const filtered = subscriptions.filter(s => {
    const matchesPlan = planFilter === 'all' || s.plan === planFilter;
    const matchesStatus = statusFilter === 'all' || s.status === statusFilter;
    return matchesPlan && matchesStatus;
  });

  const totalRevenue = subscriptions.reduce((sum, s) => sum + (s.amount || 0), 0);
  const activeCount = subscriptions.filter(s => s.status === 'Active').length;
  const expiredCount = subscriptions.filter(s => s.status === 'Expired').length;
  const expiringSoonCount = subscriptions.filter(s => s.status === 'Expiring Soon').length;

  return (
    <div className="super-admin-dashboard">
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: 'var(--text-primary)' }}>Manage Subscriptions</h2>
        <p style={{ color: 'var(--text-muted)' }}>Monitor and manage company subscription plans</p>
      </div>

      <div className="stats-grid" style={{ marginBottom: 24 }}>
        <div className="card stat-card">
          <div className="stat-icon green">✅</div>
          <div className="stat-value">{activeCount}</div>
          <div className="stat-label">Active Subscriptions</div>
        </div>
        <div className="card stat-card">
          <div className="stat-icon amber">⚠️</div>
          <div className="stat-value">{expiringSoonCount}</div>
          <div className="stat-label">Expiring Soon</div>
        </div>
        <div className="card stat-card">
          <div className="stat-icon red">❌</div>
          <div className="stat-value">{expiredCount}</div>
          <div className="stat-label">Expired</div>
        </div>
        <div className="card stat-card">
          <div className="stat-icon purple">💰</div>
          <div className="stat-value">₱{totalRevenue.toLocaleString()}</div>
          <div className="stat-label">Total Revenue</div>
        </div>
      </div>

      <div className="card">
        <div className="card-header" style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: 12, alignItems: 'center' }}>
          <span className="card-title">All Subscriptions</span>
          <div style={{ display: 'flex', gap: 8 }}>
            <select className="form-control" style={{ padding: '6px 12px', fontSize: 13 }} value={planFilter} onChange={e => setPlanFilter(e.target.value)}>
              <option value="all">All Plans</option>
              <option value="Basic Plan">Basic</option>
              <option value="Standard Plan">Standard</option>
              <option value="Premium Plan">Premium</option>
            </select>
            <select className="form-control" style={{ padding: '6px 12px', fontSize: 13 }} value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
              <option value="all">All Status</option>
              <option value="Active">Active</option>
              <option value="Expiring Soon">Expiring Soon</option>
              <option value="Expired">Expired</option>
            </select>
          </div>
        </div>
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Company</th>
                <th>Plan</th>
                <th>Start Date</th>
                <th>Expiry Date</th>
                <th>Amount</th>
                <th>Auto-Renew</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(s => (
                <tr key={s.subscriptionId}>
                  <td style={{ fontWeight: 600 }}>{s.companyName}</td>
                  <td><span className={`badge ${planColors[s.plan] || 'badge-info'}`}>{s.plan}</span></td>
                  <td>{s.startDate ? new Date(s.startDate).toLocaleDateString() : 'N/A'}</td>
                  <td>{s.expiryDate ? new Date(s.expiryDate).toLocaleDateString() : 'N/A'}</td>
                  <td>₱{(s.amount || 0).toLocaleString()}</td>
                  <td><span className={`badge ${s.autoRenew ? 'badge-success' : 'badge-danger'}`}>{s.autoRenew ? 'Yes' : 'No'}</span></td>
                  <td><span className={`badge ${statusColors[s.status] || 'badge-info'}`}>{s.status}</span></td>
                  <td>
                    <button className="btn btn-ghost btn-sm" onClick={() => handleManage(s)}>Manage</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Simple Modal Overlay */}
      {editing && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          backgroundColor: 'rgba(0,0,0,0.7)', display: 'flex', alignItems: 'center',
          justifyContent: 'center', zIndex: 1000, padding: 20
        }}>
          <div className="card" style={{ width: '100%', maxWidth: 450, padding: 30 }}>
            <h3 style={{ marginBottom: 20 }}>Manage Subscription: {editing.companyName}</h3>
            <form onSubmit={handleSave}>
              <div className="form-group">
                <label>Plan</label>
                <select 
                  className="form-control" 
                  value={editForm.plan} 
                  onChange={e => setEditForm({...editForm, plan: e.target.value})}
                  required
                >
                  <option value="Basic Plan">Basic Plan</option>
                  <option value="Standard Plan">Standard Plan</option>
                  <option value="Premium Plan">Premium Plan</option>
                </select>
              </div>
              <div className="form-group">
                <label>Expiry Date</label>
                <input 
                  type="date" 
                  className="form-control" 
                  value={editForm.expiryDate} 
                  onChange={e => setEditForm({...editForm, expiryDate: e.target.value})}
                  required
                />
              </div>
              <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <input 
                  type="checkbox" 
                  id="isActive"
                  checked={editForm.isActive} 
                  onChange={e => setEditForm({...editForm, isActive: e.target.checked})}
                />
                <label htmlFor="isActive" style={{ marginBottom: 0 }}>Subscription Active (Auto-Renew)</label>
              </div>
              <div style={{ display: 'flex', gap: 10, marginTop: 30 }}>
                <button type="button" className="btn btn-ghost" style={{ flex: 1 }} onClick={() => setEditing(null)}>Cancel</button>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }} disabled={saving}>
                  {saving ? 'Saving...' : 'Save Changes'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
