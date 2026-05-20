import { useState, useEffect } from 'react';
import API from '../api/axios';

const fallback = [
  { mrpId: 1, materialName: 'Steel Sheet 4x8', materialCode: 'MAT-001', requiredQuantity: 100, availableQuantity: 150, shortfall: 0, supplierName: 'PhilSteel Corp', unitCost: 1500, estimatedCost: 0, status: 'Sufficient', workOrderNumber: 'WO-2026-001' },
  { mrpId: 2, materialName: 'Aluminum Bar 2m', materialCode: 'MAT-002', requiredQuantity: 200, availableQuantity: 250, shortfall: 0, supplierName: 'MetalWorks PH', unitCost: 850, estimatedCost: 0, status: 'Sufficient', workOrderNumber: 'WO-2026-001' },
  { mrpId: 3, materialName: 'Industrial Paint - Blue', materialCode: 'MAT-003', requiredQuantity: 60, availableQuantity: 35, shortfall: 25, supplierName: 'ColorTech Inc', unitCost: 320, estimatedCost: 8000, status: 'Shortage', workOrderNumber: 'WO-2026-001' },
  { mrpId: 4, materialName: 'Welding Wire 1.2mm', materialCode: 'MAT-004', requiredQuantity: 40, availableQuantity: 0, shortfall: 40, supplierName: 'WeldSupply Co', unitCost: 2200, estimatedCost: 88000, status: 'Shortage', workOrderNumber: 'N/A' },
];

export default function MrpPage() {
  const [records, setRecords] = useState([]);
  const [loading, setLoading] = useState(true);
  const [calculating, setCalculating] = useState(false);

  const load = () => { API.get('/mrp').then(r => setRecords(r.data)).catch(() => setRecords(fallback)).finally(() => setLoading(false)); };
  useEffect(load, []);

  const runCalculation = async () => {
    setCalculating(true);
    try { await API.post('/mrp/calculate'); load(); } catch { alert('MRP calculation failed'); }
    finally { setCalculating(false); }
  };

  const badge = (s) => s === 'Sufficient' ? <span className="badge badge-success">✅ Sufficient</span> : <span className="badge badge-danger">⚠ Shortage</span>;

  const totalShortfall = records.reduce((a, r) => a + (r.shortfall || 0), 0);
  const totalEstimatedCost = records.reduce((a, r) => a + (r.estimatedCost || 0), 0);

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  return (
    <>
      <div className="stats-grid">
        <div className="card stat-card"><div className="stat-icon blue">📦</div><div className="stat-value">{records.length}</div><div className="stat-label">Materials Tracked</div></div>
        <div className="card stat-card"><div className="stat-icon green">✅</div><div className="stat-value">{records.filter(r=>r.status==='Sufficient').length}</div><div className="stat-label">Sufficient</div></div>
        <div className="card stat-card"><div className="stat-icon red">⚠️</div><div className="stat-value">{records.filter(r=>r.status==='Shortage').length}</div><div className="stat-label">Shortages</div></div>
        <div className="card stat-card"><div className="stat-icon amber">💰</div><div className="stat-value">₱{totalEstimatedCost.toLocaleString()}</div><div className="stat-label">Est. Procurement Cost</div></div>
      </div>
      <div className="toolbar">
        <div><span style={{fontSize:13,color:'var(--text-muted)'}}>Total shortfall: <strong style={{color:'var(--danger)'}}>{totalShortfall} units</strong></span></div>
        <button className="btn btn-primary" onClick={runCalculation} disabled={calculating}>{calculating ? '⏳ Calculating...' : '🔄 Run MRP Calculation'}</button>
      </div>
      <div className="card">
        <div className="card-header"><span className="card-title">Material Requirements</span></div>
        <div className="table-container" style={{border:'none'}}>
          <table className="data-table">
            <thead><tr><th>Code</th><th>Material</th><th>Supplier</th><th>Required</th><th>Available</th><th>Shortfall</th><th>Unit Cost</th><th>Est. Cost</th><th>Status</th></tr></thead>
            <tbody>
              {records.map(r => (
                <tr key={r.mrpId}>
                  <td style={{fontWeight:600,color:'var(--accent)'}}>{r.materialCode}</td>
                  <td style={{fontWeight:600}}>{r.materialName}</td>
                  <td style={{fontSize:13,color:'var(--text-secondary)'}}>{r.supplierName || '—'}</td>
                  <td>{r.requiredQuantity}</td><td>{r.availableQuantity}</td>
                  <td style={{color: r.shortfall > 0 ? 'var(--danger)' : 'var(--success)', fontWeight:600}}>{r.shortfall}</td>
                  <td>₱{(r.unitCost || 0).toLocaleString()}</td>
                  <td style={{fontWeight:600}}>₱{(r.estimatedCost || 0).toLocaleString()}</td>
                  <td>{badge(r.status)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}
