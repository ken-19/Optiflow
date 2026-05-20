import { useState, useEffect } from 'react';
import API from '../api/axios';

const fallback = [
  { scheduleId: 1, productName: 'Industrial Pump Assembly', plannedQuantity: 100, startDate: '2026-04-01', endDate: '2026-04-30', status: 'InProgress', priority: 'High', workOrderCount: 2 },
  { scheduleId: 2, productName: 'Conveyor Belt Module', plannedQuantity: 50, startDate: '2026-05-01', endDate: '2026-05-31', status: 'Planned', priority: 'Medium', workOrderCount: 0 },
];

export default function ProductionPage() {
  const [schedules, setSchedules] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [search, setSearch] = useState('');
  const [form, setForm] = useState({ productName: '', plannedQuantity: '', startDate: '', endDate: '', priority: 'Medium', notes: '' });

  const load = () => { API.get('/production-schedules').then(r => setSchedules(r.data)).catch(() => setSchedules(fallback)).finally(() => setLoading(false)); };
  useEffect(load, []);

  const resetForm = () => setForm({ productName: '', plannedQuantity: '', startDate: '', endDate: '', priority: 'Medium', notes: '' });

  const openCreate = () => { resetForm(); setEditItem(null); setShowModal(true); };
  const openEdit = (s) => { setForm({ productName: s.productName, plannedQuantity: s.plannedQuantity, startDate: s.startDate?.split('T')[0], endDate: s.endDate?.split('T')[0], priority: s.priority || 'Medium', notes: s.notes || '' }); setEditItem(s); setShowModal(true); };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (editItem) { await API.put(`/production-schedules/${editItem.scheduleId}`, form); }
      else { await API.post('/production-schedules', form); }
      setShowModal(false); load();
    } catch { alert('Error saving schedule'); }
  };

  const handleDelete = async (id) => { if (confirm('Delete this schedule?')) { try { await API.delete(`/production-schedules/${id}`); load(); } catch { alert('Error deleting'); } } };

  const handleStatusChange = async (id, status) => { try { await API.patch(`/production-schedules/${id}/status`, JSON.stringify(status), { headers: { 'Content-Type': 'application/json' } }); load(); } catch { alert('Error updating status'); } };

  const statusBadge = (s) => {
    const m = { Planned: 'badge-info', InProgress: 'badge-warning', Completed: 'badge-success', Cancelled: 'badge-danger' };
    return <span className={`badge ${m[s] || 'badge-info'}`}>{s}</span>;
  };
  const priBadge = (p) => { const m = { High: 'badge-danger', Medium: 'badge-warning', Low: 'badge-success' }; return <span className={`badge ${m[p] || 'badge-info'}`}>{p}</span>; };

  const filtered = schedules.filter(s => s.productName?.toLowerCase().includes(search.toLowerCase()));

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  return (
    <>
      <div className="stats-grid">
        <div className="card stat-card"><div className="stat-icon purple">🏭</div><div className="stat-value">{schedules.length}</div><div className="stat-label">Total Schedules</div></div>
        <div className="card stat-card"><div className="stat-icon amber">⏳</div><div className="stat-value">{schedules.filter(s=>s.status==='InProgress').length}</div><div className="stat-label">In Progress</div></div>
        <div className="card stat-card"><div className="stat-icon green">✅</div><div className="stat-value">{schedules.filter(s=>s.status==='Completed').length}</div><div className="stat-label">Completed</div></div>
        <div className="card stat-card"><div className="stat-icon blue">📅</div><div className="stat-value">{schedules.filter(s=>s.status==='Planned').length}</div><div className="stat-label">Planned</div></div>
      </div>
      <div className="toolbar">
        <div className="search-box"><span className="search-icon">🔍</span><input className="form-input" placeholder="Search schedules..." value={search} onChange={e => setSearch(e.target.value)} /></div>
        <button className="btn btn-primary" onClick={openCreate}>+ New Schedule</button>
      </div>
      <div className="table-container">
        <table className="data-table">
          <thead><tr><th>ID</th><th>Product</th><th>Qty</th><th>Start</th><th>End</th><th>Priority</th><th>Status</th><th>WOs</th><th>Actions</th></tr></thead>
          <tbody>
            {filtered.map(s => (
              <tr key={s.scheduleId}>
                <td>#{s.scheduleId}</td><td style={{fontWeight:600}}>{s.productName}</td><td>{s.plannedQuantity}</td>
                <td>{s.startDate?.split('T')[0]}</td><td>{s.endDate?.split('T')[0]}</td>
                <td>{priBadge(s.priority)}</td><td>{statusBadge(s.status)}</td><td>{s.workOrderCount || 0}</td>
                <td style={{display:'flex',gap:4}}>
                  <button className="btn btn-ghost btn-sm" onClick={()=>openEdit(s)}>✏️</button>
                  <select className="form-select" style={{width:110,padding:'4px 8px',fontSize:12}} value={s.status} onChange={e=>handleStatusChange(s.scheduleId,e.target.value)}>
                    <option>Planned</option><option>InProgress</option><option>Completed</option><option>Cancelled</option>
                  </select>
                  <button className="btn btn-danger btn-sm" onClick={()=>handleDelete(s.scheduleId)}>🗑</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header"><span className="modal-title">{editItem ? 'Edit Schedule' : 'New Production Schedule'}</span><button className="modal-close" onClick={() => setShowModal(false)}>✕</button></div>
            <form onSubmit={handleSubmit}>
              <div className="form-group"><label className="form-label">Product Name</label><input className="form-input" required value={form.productName} onChange={e => setForm({...form, productName: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Planned Quantity</label><input className="form-input" type="number" required value={form.plannedQuantity} onChange={e => setForm({...form, plannedQuantity: parseInt(e.target.value) || ''})} /></div>
              <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:12}}>
                <div className="form-group"><label className="form-label">Start Date</label><input className="form-input" type="date" required value={form.startDate} onChange={e => setForm({...form, startDate: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">End Date</label><input className="form-input" type="date" required value={form.endDate} onChange={e => setForm({...form, endDate: e.target.value})} /></div>
              </div>
              <div className="form-group"><label className="form-label">Priority</label><select className="form-select" value={form.priority} onChange={e => setForm({...form, priority: e.target.value})}><option>Low</option><option>Medium</option><option>High</option></select></div>
              <div className="form-group"><label className="form-label">Notes</label><textarea className="form-textarea" value={form.notes} onChange={e => setForm({...form, notes: e.target.value})} rows={3} /></div>
              <div className="modal-actions"><button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button><button type="submit" className="btn btn-primary">{editItem ? 'Save Changes' : 'Create'}</button></div>
            </form>
          </div>
        </div>
      )}
    </>
  );
}
