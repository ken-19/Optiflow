import { useState, useEffect } from 'react';
import API from '../api/axios';

const fallback = [
  { workOrderId: 1, workOrderNumber: 'WO-2026-001', productName: 'Industrial Pump Assembly', quantity: 50, completedQuantity: 30, priority: 'High', status: 'InProgress', startDate: '2026-04-01', dueDate: '2026-04-15', assignedTo: 'John Smith' },
  { workOrderId: 2, workOrderNumber: 'WO-2026-002', productName: 'Conveyor Belt Module', quantity: 50, completedQuantity: 0, priority: 'High', status: 'Pending', startDate: '2026-04-15', dueDate: '2026-04-30', assignedTo: 'Jane Doe' },
];

export default function WorkOrdersPage() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [search, setSearch] = useState('');
  const [form, setForm] = useState({ productName: '', quantity: '', priority: 'Medium', startDate: '', dueDate: '', assignedTo: '', notes: '' });

  const load = () => { API.get('/work-orders').then(r => setOrders(r.data)).catch(() => setOrders(fallback)).finally(() => setLoading(false)); };
  useEffect(load, []);

  const handleSubmit = async (e) => { e.preventDefault(); try { await API.post('/work-orders', form); setShowModal(false); load(); } catch { alert('Error creating work order'); } };

  const handleStatusChange = async (id, status) => { try { await API.patch(`/work-orders/${id}/status`, JSON.stringify(status), { headers: { 'Content-Type': 'application/json' } }); load(); } catch { alert('Error'); } };

  const handleProgressUpdate = async (id, qty) => { try { await API.patch(`/work-orders/${id}/progress`, JSON.stringify(parseInt(qty)), { headers: { 'Content-Type': 'application/json' } }); load(); } catch { alert('Error'); } };

  const handleDelete = async (id) => { if (confirm('Delete this work order?')) { try { await API.delete(`/work-orders/${id}`); load(); } catch { alert('Error'); } } };

  const badge = (s) => { const m = { Pending: 'badge-warning', InProgress: 'badge-info', Completed: 'badge-success', OnHold: 'badge-purple', Cancelled: 'badge-danger' }; return <span className={`badge ${m[s] || 'badge-info'}`}>{s}</span>; };
  const priBadge = (p) => { const m = { High: 'badge-danger', Medium: 'badge-warning', Low: 'badge-success' }; return <span className={`badge ${m[p] || 'badge-info'}`}>{p}</span>; };

  const filtered = orders.filter(o => o.productName?.toLowerCase().includes(search.toLowerCase()) || o.workOrderNumber?.toLowerCase().includes(search.toLowerCase()));

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  return (
    <>
      <div className="stats-grid">
        <div className="card stat-card"><div className="stat-icon purple">📋</div><div className="stat-value">{orders.length}</div><div className="stat-label">Total Orders</div></div>
        <div className="card stat-card"><div className="stat-icon green">✅</div><div className="stat-value">{orders.filter(o=>o.status==='Completed').length}</div><div className="stat-label">Completed</div></div>
        <div className="card stat-card"><div className="stat-icon amber">⏳</div><div className="stat-value">{orders.filter(o=>o.status==='InProgress').length}</div><div className="stat-label">In Progress</div></div>
        <div className="card stat-card"><div className="stat-icon red">⏸️</div><div className="stat-value">{orders.filter(o=>o.status==='Pending').length}</div><div className="stat-label">Pending</div></div>
      </div>
      <div className="toolbar">
        <div className="search-box"><span className="search-icon">🔍</span><input className="form-input" placeholder="Search work orders..." value={search} onChange={e=>setSearch(e.target.value)} /></div>
        <button className="btn btn-primary" onClick={() => setShowModal(true)}>+ New Work Order</button>
      </div>
      <div className="table-container">
        <table className="data-table">
          <thead><tr><th>WO #</th><th>Product</th><th>Qty</th><th>Progress</th><th>Assigned</th><th>Due</th><th>Priority</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>
            {filtered.map(o => (
              <tr key={o.workOrderId}>
                <td style={{fontWeight:600}}>{o.workOrderNumber}</td><td>{o.productName}</td><td>{o.completedQuantity}/{o.quantity}</td>
                <td><div style={{background:'var(--bg-input)',borderRadius:8,height:8,width:100}}><div style={{background: o.completedQuantity >= o.quantity ? 'var(--success)' : 'var(--accent)',borderRadius:8,height:8,width:`${Math.min(100,(o.completedQuantity/o.quantity)*100)}%`}}></div></div></td>
                <td>{o.assignedTo || '—'}</td><td>{o.dueDate?.split('T')[0] || '—'}</td>
                <td>{priBadge(o.priority)}</td><td>{badge(o.status)}</td>
                <td style={{display:'flex',gap:4,flexWrap:'wrap'}}>
                  <select className="form-select" style={{width:100,padding:'4px 6px',fontSize:11}} value={o.status} onChange={e=>handleStatusChange(o.workOrderId,e.target.value)}>
                    <option>Pending</option><option>InProgress</option><option>Completed</option><option>OnHold</option><option>Cancelled</option>
                  </select>
                  <button className="btn btn-ghost btn-sm" onClick={()=>{const q=prompt('Enter completed qty:',o.completedQuantity);if(q!==null)handleProgressUpdate(o.workOrderId,q);}}>📊</button>
                  <button className="btn btn-danger btn-sm" onClick={()=>handleDelete(o.workOrderId)}>🗑</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header"><span className="modal-title">New Work Order</span><button className="modal-close" onClick={() => setShowModal(false)}>✕</button></div>
            <form onSubmit={handleSubmit}>
              <div className="form-group"><label className="form-label">Product Name</label><input className="form-input" required value={form.productName} onChange={e => setForm({...form, productName: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Quantity</label><input className="form-input" type="number" required value={form.quantity} onChange={e => setForm({...form, quantity: parseInt(e.target.value) || ''})} /></div>
              <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:12}}>
                <div className="form-group"><label className="form-label">Start Date</label><input className="form-input" type="date" value={form.startDate} onChange={e => setForm({...form, startDate: e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Due Date</label><input className="form-input" type="date" value={form.dueDate} onChange={e => setForm({...form, dueDate: e.target.value})} /></div>
              </div>
              <div className="form-group"><label className="form-label">Assigned To</label><input className="form-input" value={form.assignedTo} onChange={e => setForm({...form, assignedTo: e.target.value})} placeholder="Worker name" /></div>
              <div className="form-group"><label className="form-label">Priority</label><select className="form-select" value={form.priority} onChange={e => setForm({...form, priority: e.target.value})}><option>Low</option><option>Medium</option><option>High</option></select></div>
              <div className="modal-actions"><button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button><button type="submit" className="btn btn-primary">Create</button></div>
            </form>
          </div>
        </div>
      )}
    </>
  );
}
