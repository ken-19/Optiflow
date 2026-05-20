import { useState, useEffect } from 'react';
import API from '../api/axios';

const fallbackAdmins = [
  { userId: 1, username: "john_doe", firstName: "John", lastName: "Doe", email: "john@acme.com", companyName: "Acme Corp", role: "Admin", isActive: true, lastLogin: "2026-05-10T08:30:00Z" },
  { userId: 2, username: "jane_smith", firstName: "Jane", lastName: "Smith", email: "jane@techgear.com", companyName: "TechGear", role: "Admin", isActive: true, lastLogin: "2026-05-09T14:22:00Z" },
  { userId: 3, username: "bob_brown", firstName: "Bob", lastName: "Brown", email: "bob@oldfactory.com", companyName: "OldFactory", role: "Admin", isActive: false, lastLogin: "2025-12-01T10:00:00Z" },
  { userId: 4, username: "maria_garcia", firstName: "Maria", lastName: "Garcia", email: "maria@steelworks.com", companyName: "SteelWorks Inc", role: "Admin", isActive: true, lastLogin: "2026-05-11T09:15:00Z" },
  { userId: 5, username: "li_wei", firstName: "Li", lastName: "Wei", email: "li@quickparts.com", companyName: "QuickParts", role: "Admin", isActive: true, lastLogin: "2026-05-08T16:45:00Z" },
];

export default function AdminAccountsPage() {
  const [admins, setAdmins] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');

  useEffect(() => {
    API.get('/superadmin/admins')
      .then(res => setAdmins(res.data))
      .catch(() => setAdmins(fallbackAdmins))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  const filtered = admins.filter(a => {
    const matchesSearch = a.username?.toLowerCase().includes(search.toLowerCase()) ||
                          a.firstName?.toLowerCase().includes(search.toLowerCase()) ||
                          a.lastName?.toLowerCase().includes(search.toLowerCase()) ||
                          a.companyName?.toLowerCase().includes(search.toLowerCase());
    const matchesStatus = statusFilter === 'all' ||
                          (statusFilter === 'active' && a.isActive) ||
                          (statusFilter === 'inactive' && !a.isActive);
    return matchesSearch && matchesStatus;
  });

  const totalAdmins = admins.length;
  const activeAdmins = admins.filter(a => a.isActive).length;
  const inactiveAdmins = admins.filter(a => !a.isActive).length;

  return (
    <div className="super-admin-dashboard">
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: 'var(--text-primary)' }}>Manage Admin Accounts</h2>
        <p style={{ color: 'var(--text-muted)' }}>View and manage company administrator accounts</p>
      </div>

      <div className="stats-grid" style={{ marginBottom: 24 }}>
        <div className="card stat-card">
          <div className="stat-icon purple">👤</div>
          <div className="stat-value">{totalAdmins}</div>
          <div className="stat-label">Total Admins</div>
        </div>
        <div className="card stat-card">
          <div className="stat-icon green">✅</div>
          <div className="stat-value">{activeAdmins}</div>
          <div className="stat-label">Active Admins</div>
        </div>
        <div className="card stat-card">
          <div className="stat-icon red">🚫</div>
          <div className="stat-value">{inactiveAdmins}</div>
          <div className="stat-label">Inactive Admins</div>
        </div>
      </div>

      <div className="card">
        <div className="card-header" style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: 12, alignItems: 'center' }}>
          <span className="card-title">Admin Accounts</span>
          <div style={{ display: 'flex', gap: 8 }}>
            <input
              type="text"
              className="form-control"
              placeholder="Search admins..."
              style={{ width: 220, padding: '6px 12px', fontSize: 13 }}
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
            <select
              className="form-control"
              style={{ padding: '6px 12px', fontSize: 13 }}
              value={statusFilter}
              onChange={e => setStatusFilter(e.target.value)}
            >
              <option value="all">All Status</option>
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
            </select>
          </div>
        </div>
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Username</th>
                <th>Name</th>
                <th>Email</th>
                <th>Company</th>
                <th>Role</th>
                <th>Status</th>
                <th>Last Login</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(a => (
                <tr key={a.userId}>
                  <td style={{ fontWeight: 600 }}>{a.username}</td>
                  <td>{a.firstName} {a.lastName}</td>
                  <td style={{ fontSize: 13, color: 'var(--text-muted)' }}>{a.email}</td>
                  <td>{a.companyName}</td>
                  <td><span className="badge badge-info">{a.role}</span></td>
                  <td>
                    <span className={`badge ${a.isActive ? 'badge-success' : 'badge-danger'}`}>
                      {a.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>{a.lastLogin ? new Date(a.lastLogin).toLocaleString() : 'Never'}</td>
                  <td>
                    <button className="btn btn-ghost btn-sm">Manage</button>
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && (
                <tr><td colSpan="8" style={{ textAlign: 'center', padding: 20, color: 'var(--text-muted)' }}>No admin accounts found</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
