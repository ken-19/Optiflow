import { useState, useEffect } from 'react';
import API from '../api/axios';

const fallbackInsp = [
  { inspectionId: 1, workOrderId: 1, workOrderNumber: 'WO-2026-001', inspectorName: 'John Smith', inspectionDate: '2026-04-20', sampleSize: 50, passedCount: 48, failedCount: 2, result: 'Pass', defectCount: 1, notes: '' },
  { inspectionId: 2, workOrderId: 2, workOrderNumber: 'WO-2026-002', inspectorName: 'Jane Doe', inspectionDate: '2026-04-21', sampleSize: 30, passedCount: 25, failedCount: 5, result: 'Fail', defectCount: 3, notes: 'Surface defects found' },
];
const fallbackDef = [
  { defectId: 1, inspectionId: 2, defectType: 'Surface Scratch', severity: 'Medium', defectCount: 2, description: 'Visible scratches on panel', status: 'Open', reportedAt: '2026-04-21' },
  { defectId: 2, inspectionId: 2, defectType: 'Dimensional Error', severity: 'High', defectCount: 1, description: 'Part exceeds tolerance', status: 'Open', reportedAt: '2026-04-21' },
];

export default function QualityPage() {
  const [inspections, setInspections] = useState([]);
  const [defects, setDefects] = useState([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState('inspections');
  const [showModal, setShowModal] = useState(false);
  const [modalType, setModalType] = useState('inspection');
  const [form, setForm] = useState({});

  const load = () => {
    Promise.all([
      API.get('/quality/inspections').catch(() => ({ data: fallbackInsp })),
      API.get('/quality/defects').catch(() => ({ data: fallbackDef })),
    ]).then(([iRes, dRes]) => { setInspections(iRes.data); setDefects(dRes.data); }).finally(() => setLoading(false));
  };
  useEffect(load, []);

  const openInspectionModal = () => { setModalType('inspection'); setForm({ workOrderId: '', inspectorName: '', sampleSize: '', passedCount: '', failedCount: '', result: 'Pass', notes: '' }); setShowModal(true); };
  const openDefectModal = () => { setModalType('defect'); setForm({ inspectionId: '', defectType: '', severity: 'Medium', defectCount: 1, description: '' }); setShowModal(true); };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (modalType === 'inspection') {
        await API.post('/quality/inspections', { ...form, workOrderId: parseInt(form.workOrderId), sampleSize: parseInt(form.sampleSize), passedCount: parseInt(form.passedCount), failedCount: parseInt(form.failedCount) });
      } else {
        await API.post('/quality/defects', { ...form, inspectionId: parseInt(form.inspectionId), defectCount: parseInt(form.defectCount) });
      }
      setShowModal(false); load();
    } catch { alert('Error saving'); }
  };

  const resolveDefect = async (id) => { const action = prompt('Enter corrective action:'); if (action) { try { await API.patch(`/quality/defects/${id}/resolve`, JSON.stringify(action), { headers: { 'Content-Type': 'application/json' } }); load(); } catch { alert('Error'); } } };

  const rBadge = (r) => r === 'Pass' ? <span className="badge badge-success">Pass</span> : <span className="badge badge-danger">Fail</span>;
  const sBadge = (s) => { const m = { Low: 'badge-info', Medium: 'badge-warning', High: 'badge-danger', Critical: 'badge-danger' }; return <span className={`badge ${m[s]||'badge-info'}`}>{s}</span>; };
  const stBadge = (s) => s === 'Open' ? <span className="badge badge-warning">Open</span> : <span className="badge badge-success">Resolved</span>;

  const passRate = inspections.length > 0 ? Math.round((inspections.filter(i => i.result === 'Pass').length / inspections.length) * 100) : 0;

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  return (
    <>
      <div className="stats-grid">
        <div className="card stat-card"><div className="stat-icon green">✅</div><div className="stat-value">{inspections.filter(i=>i.result==='Pass').length}</div><div className="stat-label">Passed</div></div>
        <div className="card stat-card"><div className="stat-icon red">❌</div><div className="stat-value">{inspections.filter(i=>i.result==='Fail').length}</div><div className="stat-label">Failed</div></div>
        <div className="card stat-card"><div className="stat-icon amber">🐛</div><div className="stat-value">{defects.filter(d=>d.status==='Open').length}</div><div className="stat-label">Open Defects</div></div>
        <div className="card stat-card"><div className="stat-icon blue">📊</div><div className="stat-value">{passRate}%</div><div className="stat-label">Pass Rate</div></div>
      </div>
      <div className="toolbar">
        <div style={{display:'flex',gap:8}}>
          <button className={`btn ${tab==='inspections'?'btn-primary':'btn-ghost'}`} onClick={()=>setTab('inspections')}>Inspections</button>
          <button className={`btn ${tab==='defects'?'btn-primary':'btn-ghost'}`} onClick={()=>setTab('defects')}>Defects</button>
        </div>
        <button className="btn btn-primary" onClick={tab==='inspections'?openInspectionModal:openDefectModal}>+ {tab==='inspections'?'New Inspection':'Report Defect'}</button>
      </div>
      {tab === 'inspections' ? (
        <div className="card"><div className="table-container" style={{border:'none'}}>
          <table className="data-table">
            <thead><tr><th>ID</th><th>Work Order</th><th>Inspector</th><th>Date</th><th>Sample</th><th>Pass/Fail</th><th>Defects</th><th>Result</th></tr></thead>
            <tbody>{inspections.map(i => (
              <tr key={i.inspectionId}><td>#{i.inspectionId}</td><td style={{fontWeight:600}}>{i.workOrderNumber}</td><td>{i.inspectorName}</td><td>{i.inspectionDate?.split('T')[0]}</td><td>{i.sampleSize}</td><td>{i.passedCount}/{i.failedCount}</td><td>{i.defectCount}</td><td>{rBadge(i.result)}</td></tr>
            ))}</tbody>
          </table>
        </div></div>
      ) : (
        <div className="card"><div className="table-container" style={{border:'none'}}>
          <table className="data-table">
            <thead><tr><th>ID</th><th>Type</th><th>Severity</th><th>Count</th><th>Description</th><th>Status</th><th>Date</th><th>Action</th></tr></thead>
            <tbody>{defects.map(d => (
              <tr key={d.defectId}><td>#{d.defectId}</td><td style={{fontWeight:600}}>{d.defectType}</td><td>{sBadge(d.severity)}</td><td>{d.defectCount}</td><td style={{fontSize:13,color:'var(--text-secondary)',maxWidth:200,overflow:'hidden',textOverflow:'ellipsis'}}>{d.description||'—'}</td><td>{stBadge(d.status)}</td><td>{d.reportedAt?.split('T')[0]}</td>
              <td>{d.status==='Open'&&<button className="btn btn-success btn-sm" onClick={()=>resolveDefect(d.defectId)}>Resolve</button>}</td></tr>
            ))}</tbody>
          </table>
        </div></div>
      )}
      {showModal && (
        <div className="modal-overlay" onClick={()=>setShowModal(false)}>
          <div className="modal" onClick={e=>e.stopPropagation()}>
            <div className="modal-header"><span className="modal-title">{modalType==='inspection'?'New Inspection':'Report Defect'}</span><button className="modal-close" onClick={()=>setShowModal(false)}>✕</button></div>
            <form onSubmit={handleSubmit}>
              {modalType === 'inspection' ? (<>
                <div className="form-group"><label className="form-label">Work Order ID</label><input className="form-input" type="number" required value={form.workOrderId} onChange={e=>setForm({...form,workOrderId:e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Inspector Name</label><input className="form-input" required value={form.inspectorName} onChange={e=>setForm({...form,inspectorName:e.target.value})} /></div>
                <div style={{display:'grid',gridTemplateColumns:'1fr 1fr 1fr',gap:12}}>
                  <div className="form-group"><label className="form-label">Sample Size</label><input className="form-input" type="number" required value={form.sampleSize} onChange={e=>setForm({...form,sampleSize:e.target.value})} /></div>
                  <div className="form-group"><label className="form-label">Passed</label><input className="form-input" type="number" required value={form.passedCount} onChange={e=>setForm({...form,passedCount:e.target.value})} /></div>
                  <div className="form-group"><label className="form-label">Failed</label><input className="form-input" type="number" required value={form.failedCount} onChange={e=>setForm({...form,failedCount:e.target.value})} /></div>
                </div>
                <div className="form-group"><label className="form-label">Result</label><select className="form-select" value={form.result} onChange={e=>setForm({...form,result:e.target.value})}><option>Pass</option><option>Fail</option><option>ConditionalPass</option></select></div>
                <div className="form-group"><label className="form-label">Notes</label><textarea className="form-textarea" value={form.notes} onChange={e=>setForm({...form,notes:e.target.value})} rows={2} /></div>
              </>) : (<>
                <div className="form-group"><label className="form-label">Inspection ID</label><input className="form-input" type="number" required value={form.inspectionId} onChange={e=>setForm({...form,inspectionId:e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Defect Type</label><input className="form-input" required value={form.defectType} onChange={e=>setForm({...form,defectType:e.target.value})} placeholder="e.g. Surface Scratch, Dimensional Error" /></div>
                <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:12}}>
                  <div className="form-group"><label className="form-label">Severity</label><select className="form-select" value={form.severity} onChange={e=>setForm({...form,severity:e.target.value})}><option>Low</option><option>Medium</option><option>High</option><option>Critical</option></select></div>
                  <div className="form-group"><label className="form-label">Count</label><input className="form-input" type="number" value={form.defectCount} onChange={e=>setForm({...form,defectCount:e.target.value})} /></div>
                </div>
                <div className="form-group"><label className="form-label">Description</label><textarea className="form-textarea" value={form.description} onChange={e=>setForm({...form,description:e.target.value})} rows={2} /></div>
              </>)}
              <div className="modal-actions"><button type="button" className="btn btn-ghost" onClick={()=>setShowModal(false)}>Cancel</button><button type="submit" className="btn btn-primary">Save</button></div>
            </form>
          </div>
        </div>
      )}
    </>
  );
}
