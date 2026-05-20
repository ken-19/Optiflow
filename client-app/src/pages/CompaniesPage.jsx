import { useState, useEffect } from 'react';
import API from '../api/axios';

const fallbackCompanies = [
  { companyId: 1, companyName: "Acme Corp", adminName: "John Doe", email: "admin@acme.com", subscriptionPlan: "Premium Plan", userCount: 45, isActive: true, createdAt: "2025-01-10T00:00:00Z" },
  { companyId: 2, companyName: "TechGear", adminName: "Jane Smith", email: "admin@techgear.com", subscriptionPlan: "Standard Plan", userCount: 18, isActive: true, createdAt: "2025-06-20T00:00:00Z" },
  { companyId: 3, companyName: "OldFactory", adminName: "Bob Brown", email: "admin@oldfactory.com", subscriptionPlan: "Basic Plan", userCount: 5, isActive: false, createdAt: "2024-03-15T00:00:00Z" },
  { companyId: 4, companyName: "SteelWorks Inc", adminName: "Maria Garcia", email: "admin@steelworks.com", subscriptionPlan: "Premium Plan", userCount: 32, isActive: true, createdAt: "2025-04-01T00:00:00Z" },
  { companyId: 5, companyName: "QuickParts", adminName: "Li Wei", email: "admin@quickparts.com", subscriptionPlan: "Standard Plan", userCount: 12, isActive: true, createdAt: "2025-08-14T00:00:00Z" },
];

export default function CompaniesPage() {
  const [companies, setCompanies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [selectedCompany, setSelectedCompany] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [modalLoading, setModalLoading] = useState(false);

  useEffect(() => {
    API.get('/superadmin/companies')
      .then(res => setCompanies(res.data))
      .catch(() => setCompanies(fallbackCompanies))
      .finally(() => setLoading(false));
  }, []);

  const handleViewDetails = (id) => {
    setModalLoading(true);
    setShowModal(true);
    API.get(`/superadmin/companies/${id}`)
      .then(res => setSelectedCompany(res.data))
      .catch(err => console.error("Failed to fetch details", err))
      .finally(() => setModalLoading(false));
  };

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  const filtered = companies.filter(c => {
    const matchesSearch = c.companyName?.toLowerCase().includes(search.toLowerCase()) ||
                          c.adminName?.toLowerCase().includes(search.toLowerCase());
    const matchesStatus = statusFilter === 'all' ||
                          (statusFilter === 'active' && c.isActive) ||
                          (statusFilter === 'inactive' && !c.isActive);
    return matchesSearch && matchesStatus;
  });

  const totalCompanies = companies.length;
  const activeCount = companies.filter(c => c.isActive).length;
  const inactiveCount = companies.filter(c => !c.isActive).length;

  return (
    <div className="super-admin-dashboard">
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: 24, fontWeight: 700, color: 'var(--text-primary)' }}>Companies</h2>
        <p style={{ color: 'var(--text-muted)' }}>Manage all registered companies and their tenants</p>
      </div>

      <div className="stats-grid" style={{ marginBottom: 24 }}>
        <div className="card stat-card">
          <div className="stat-icon blue">🏢</div>
          <div className="stat-value">{totalCompanies}</div>
          <div className="stat-label">Total Companies</div>
        </div>
        <div className="card stat-card">
          <div className="stat-icon green">✅</div>
          <div className="stat-value">{activeCount}</div>
          <div className="stat-label">Active</div>
        </div>
        <div className="card stat-card">
          <div className="stat-icon red">🚫</div>
          <div className="stat-value">{inactiveCount}</div>
          <div className="stat-label">Inactive</div>
        </div>
      </div>

      <div className="card">
        <div className="card-header" style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: 12, alignItems: 'center' }}>
          <span className="card-title">Company Directory</span>
          <div style={{ display: 'flex', gap: 8 }}>
            <input
              type="text"
              className="form-control"
              placeholder="Search companies..."
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
                <th>Company Name</th>
                <th>Admin</th>
                <th>Email</th>
                <th>Plan</th>
                <th>Users</th>
                <th>Status</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(c => (
                <tr key={c.companyId}>
                  <td style={{ fontWeight: 600 }}>{c.companyName}</td>
                  <td>{c.adminName}</td>
                  <td style={{ fontSize: 13, color: 'var(--text-muted)' }}>{c.email}</td>
                  <td>
                    <span className={`badge ${c.subscriptionPlan === 'Premium Plan' ? 'badge-purple' : c.subscriptionPlan === 'Standard Plan' ? 'badge-info' : 'badge-success'}`}>
                      {c.subscriptionPlan || 'None'}
                    </span>
                  </td>
                  <td>{c.userCount}</td>
                  <td>
                    <span className={`badge ${c.isActive ? 'badge-success' : 'badge-danger'}`}>
                      {c.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td>{c.createdAt ? new Date(c.createdAt).toLocaleDateString() : 'N/A'}</td>
                  <td>
                    <button 
                      className="btn btn-ghost btn-sm"
                      onClick={() => handleViewDetails(c.companyId)}
                    >
                      View
                    </button>
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && (
                <tr><td colSpan="8" style={{ textAlign: 'center', padding: 20, color: 'var(--text-muted)' }}>No companies found</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Details Modal */}
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()} style={{ maxWidth: 600 }}>
            <div className="modal-header">
              <h3 className="modal-title">Company Details</h3>
              <button className="btn-close" onClick={() => setShowModal(false)}>×</button>
            </div>
            <div className="modal-body">
              {modalLoading ? (
                <div style={{ textAlign: 'center', padding: 40 }}><div className="spinner"></div></div>
              ) : selectedCompany ? (
                <div className="company-details">
                  <div style={{ display: 'flex', alignItems: 'center', gap: 16, marginBottom: 24 }}>
                    <div style={{ width: 64, height: 64, background: 'var(--primary-light)', borderRadius: 12, display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 32 }}>
                      🏢
                    </div>
                    <div>
                      <h4 style={{ fontSize: 20, fontWeight: 700, margin: 0 }}>{selectedCompany.companyName}</h4>
                      <span className={`badge ${selectedCompany.isActive ? 'badge-success' : 'badge-danger'}`} style={{ marginTop: 4 }}>
                        {selectedCompany.isActive ? 'Active Tenant' : 'Inactive Tenant'}
                      </span>
                    </div>
                  </div>

                  <div className="details-grid" style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 20 }}>
                    <div className="detail-item">
                      <label style={{ display: 'block', fontSize: 12, color: 'var(--text-muted)', marginBottom: 4 }}>Industry</label>
                      <div style={{ fontWeight: 500 }}>{selectedCompany.industry || 'General'}</div>
                    </div>
                    <div className="detail-item">
                      <label style={{ display: 'block', fontSize: 12, color: 'var(--text-muted)', marginBottom: 4 }}>Subscription</label>
                      <div style={{ fontWeight: 600, color: 'var(--primary-color)' }}>{selectedCompany.subscriptionPlan}</div>
                    </div>
                    <div className="detail-item">
                      <label style={{ display: 'block', fontSize: 12, color: 'var(--text-muted)', marginBottom: 4 }}>Contact Email</label>
                      <div style={{ fontWeight: 500 }}>{selectedCompany.contactEmail}</div>
                    </div>
                    <div className="detail-item">
                      <label style={{ display: 'block', fontSize: 12, color: 'var(--text-muted)', marginBottom: 4 }}>Contact Phone</label>
                      <div style={{ fontWeight: 500 }}>{selectedCompany.contactPhone || 'N/A'}</div>
                    </div>
                    <div className="detail-item" style={{ gridColumn: 'span 2' }}>
                      <label style={{ display: 'block', fontSize: 12, color: 'var(--text-muted)', marginBottom: 4 }}>Address</label>
                      <div style={{ fontWeight: 500 }}>{selectedCompany.address || 'No address provided'}</div>
                    </div>
                  </div>

                  <div style={{ marginTop: 24, padding: 16, background: 'var(--bg-lighter)', borderRadius: 8 }}>
                    <h5 style={{ margin: '0 0 12px 0', fontSize: 14 }}>Company Administrators</h5>
                    {selectedCompany.admins?.length > 0 ? (
                      selectedCompany.admins.map(admin => (
                        <div key={admin.userId} style={{ display: 'flex', justifyContent: 'space-between', fontSize: 13, marginBottom: 8 }}>
                          <span>{admin.firstName} {admin.lastName}</span>
                          <span style={{ color: 'var(--text-muted)' }}>{admin.email}</span>
                        </div>
                      ))
                    ) : (
                      <div style={{ fontSize: 13, color: 'var(--text-muted)' }}>No administrators assigned</div>
                    )}
                  </div>
                </div>
              ) : (
                <div style={{ textAlign: 'center', padding: 20 }}>Failed to load company details</div>
              )}
            </div>
            <div className="modal-footer">
              <button className="btn btn-primary" onClick={() => setShowModal(false)}>Close</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
