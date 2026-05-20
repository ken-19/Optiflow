import { useState, useEffect } from 'react';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend } from 'chart.js';
import { Bar, Doughnut } from 'react-chartjs-2';
import API from '../api/axios';

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend);

const fallback = [
  { costId: 1, workOrderId: 1, workOrderNumber: 'WO-2026-001', costType: 'Labor', description: 'Assembly labor', amount: 5200, currency: 'PHP', incurredDate: '2026-04-15' },
  { costId: 2, workOrderId: 1, workOrderNumber: 'WO-2026-001', costType: 'Material', description: 'Raw materials', amount: 12800, currency: 'PHP', incurredDate: '2026-04-15' },
  { costId: 3, workOrderId: 1, workOrderNumber: 'WO-2026-001', costType: 'Overhead', description: 'Facility costs', amount: 3100, currency: 'PHP', incurredDate: '2026-04-15' },
  { costId: 4, workOrderId: 2, workOrderNumber: 'WO-2026-002', costType: 'Labor', description: 'Setup labor', amount: 3400, currency: 'PHP', incurredDate: '2026-04-18' },
  { costId: 5, workOrderId: 2, workOrderNumber: 'WO-2026-002', costType: 'Material', description: 'Components', amount: 8900, currency: 'PHP', incurredDate: '2026-04-18' },
];

export default function CostingPage() {
  const [costs, setCosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState({ workOrderId: '', costType: 'Labor', description: '', amount: '', incurredDate: '' });

  const load = () => { API.get('/production-costs').then(r => setCosts(r.data)).catch(() => setCosts(fallback)).finally(() => setLoading(false)); };
  useEffect(load, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await API.post('/production-costs', { ...form, workOrderId: parseInt(form.workOrderId), amount: parseFloat(form.amount) });
      setShowModal(false); setForm({ workOrderId: '', costType: 'Labor', description: '', amount: '', incurredDate: '' }); load();
    } catch { alert('Error adding cost entry'); }
  };

  const handleDelete = async (id) => { if (confirm('Delete this cost entry?')) { try { await API.delete(`/production-costs/${id}`); load(); } catch { alert('Error'); } } };

  const totalCost = costs.reduce((a, c) => a + c.amount, 0);
  const byType = costs.reduce((a, c) => { a[c.costType] = (a[c.costType] || 0) + c.amount; return a; }, {});

  const chartOpts = { responsive: true, maintainAspectRatio: false, plugins: { legend: { labels: { color: '#94a3b8' } } }, scales: { x: { ticks: { color: '#64748b' }, grid: { color: '#2a2f45' } }, y: { ticks: { color: '#64748b' }, grid: { color: '#2a2f45' } } } };
  const dOpts = { responsive: true, maintainAspectRatio: false, plugins: { legend: { position: 'bottom', labels: { color: '#94a3b8', padding: 16 } } } };

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  return (
    <>
      <div className="stats-grid">
        <div className="card stat-card"><div className="stat-icon purple">💰</div><div className="stat-value">₱{totalCost.toLocaleString()}</div><div className="stat-label">Total Cost</div></div>
        <div className="card stat-card"><div className="stat-icon blue">👷</div><div className="stat-value">₱{(byType.Labor||0).toLocaleString()}</div><div className="stat-label">Labor Cost</div></div>
        <div className="card stat-card"><div className="stat-icon green">📦</div><div className="stat-value">₱{(byType.Material||0).toLocaleString()}</div><div className="stat-label">Material Cost</div></div>
        <div className="card stat-card"><div className="stat-icon amber">🏢</div><div className="stat-value">₱{(byType.Overhead||0).toLocaleString()}</div><div className="stat-label">Overhead</div></div>
      </div>
      <div className="charts-grid">
        <div className="card chart-card"><div className="card-header"><span className="card-title">Cost Distribution</span></div><div style={{height:260}}><Doughnut data={{labels:Object.keys(byType), datasets:[{data:Object.values(byType), backgroundColor:['#6366f1','#22c55e','#f59e0b','#ef4444','#8b5cf6'], borderWidth:0}]}} options={dOpts} /></div></div>
        <div className="card chart-card"><div className="card-header"><span className="card-title">Cost by Work Order</span></div><div style={{height:260}}><Bar data={{labels:[...new Set(costs.map(c=>c.workOrderNumber))], datasets:[{label:'Amount (₱)',data:[...new Set(costs.map(c=>c.workOrderNumber))].map(wo=>costs.filter(c=>c.workOrderNumber===wo).reduce((a,c)=>a+c.amount,0)),backgroundColor:'#6366f1',borderRadius:6}]}} options={chartOpts} /></div></div>
      </div>
      <div className="card">
        <div className="card-header"><span className="card-title">Cost Entries</span><button className="btn btn-primary btn-sm" onClick={()=>setShowModal(true)}>+ Add Cost Entry</button></div>
        <div className="table-container" style={{border:'none'}}>
          <table className="data-table">
            <thead><tr><th>Work Order</th><th>Type</th><th>Description</th><th>Amount</th><th>Date</th><th>Actions</th></tr></thead>
            <tbody>{costs.map(c => (
              <tr key={c.costId}><td style={{fontWeight:600}}>{c.workOrderNumber}</td>
              <td><span className={`badge ${c.costType==='Labor'?'badge-purple':c.costType==='Material'?'badge-success':'badge-warning'}`}>{c.costType}</span></td>
              <td style={{fontSize:13,color:'var(--text-secondary)'}}>{c.description||'—'}</td>
              <td style={{fontWeight:600}}>₱{c.amount.toLocaleString()}</td>
              <td>{c.incurredDate?.split('T')[0]}</td>
              <td><button className="btn btn-danger btn-sm" onClick={()=>handleDelete(c.costId)}>🗑</button></td></tr>
            ))}</tbody>
          </table>
        </div>
      </div>
      {showModal && (
        <div className="modal-overlay" onClick={()=>setShowModal(false)}>
          <div className="modal" onClick={e=>e.stopPropagation()}>
            <div className="modal-header"><span className="modal-title">Add Cost Entry</span><button className="modal-close" onClick={()=>setShowModal(false)}>✕</button></div>
            <form onSubmit={handleSubmit}>
              <div className="form-group"><label className="form-label">Work Order ID</label><input className="form-input" type="number" required value={form.workOrderId} onChange={e=>setForm({...form,workOrderId:e.target.value})} placeholder="e.g. 1" /></div>
              <div className="form-group"><label className="form-label">Cost Type</label><select className="form-select" value={form.costType} onChange={e=>setForm({...form,costType:e.target.value})}><option>Labor</option><option>Material</option><option>Overhead</option><option>Equipment</option><option>Other</option></select></div>
              <div className="form-group"><label className="form-label">Description</label><input className="form-input" value={form.description} onChange={e=>setForm({...form,description:e.target.value})} placeholder="Cost description" /></div>
              <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:12}}>
                <div className="form-group"><label className="form-label">Amount (₱)</label><input className="form-input" type="number" step="0.01" required value={form.amount} onChange={e=>setForm({...form,amount:e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Date</label><input className="form-input" type="date" value={form.incurredDate} onChange={e=>setForm({...form,incurredDate:e.target.value})} /></div>
              </div>
              <div className="modal-actions"><button type="button" className="btn btn-ghost" onClick={()=>setShowModal(false)}>Cancel</button><button type="submit" className="btn btn-primary">Add</button></div>
            </form>
          </div>
        </div>
      )}
    </>
  );
}
