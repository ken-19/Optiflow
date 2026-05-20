import { useState, useEffect } from 'react';
import API from '../api/axios';

const fallback = [
  { inventoryId: 1, materialName: 'Steel Sheet 4x8', materialCode: 'MAT-001', quantityOnHand: 200, quantityReserved: 50, quantityAvailable: 150, warehouseLocation: 'Warehouse A-1', batchNumber: 'BATCH-2026-001' },
  { inventoryId: 2, materialName: 'Aluminum Bar 2m', materialCode: 'MAT-002', quantityOnHand: 350, quantityReserved: 100, quantityAvailable: 250, warehouseLocation: 'Warehouse A-2', batchNumber: 'BATCH-2026-002' },
  { inventoryId: 3, materialName: 'Industrial Paint - Blue', materialCode: 'MAT-003', quantityOnHand: 45, quantityReserved: 10, quantityAvailable: 35, warehouseLocation: 'Warehouse B-1', batchNumber: 'BATCH-2026-003' },
];

export default function InventoryPage() {
  const [items, setItems] = useState([]);
  const [materials, setMaterials] = useState([]);
  const [showArchived, setShowArchived] = useState(false);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [search, setSearch] = useState('');
  const [form, setForm] = useState({ materialId: '', quantityOnHand: '', warehouseLocation: '', batchNumber: '' });

  const load = async () => {
    try {
      const [invRes, matRes] = await Promise.all([
        API.get(`/inventory?includeArchived=${showArchived}`).catch(() => ({ data: fallback })),
        API.get('/inventory/materials').catch(() => ({ data: [] })),
      ]);
      setItems(invRes.data);
      setMaterials(matRes.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [showArchived]);

  const handleSubmit = async (e) => { e.preventDefault(); try { await API.post('/inventory', { ...form, materialId: parseInt(form.materialId), quantityOnHand: parseInt(form.quantityOnHand) }); setShowModal(false); load(); } catch { alert('Error adding inventory'); } };

  const handleAdjust = async (id, change) => { try { await API.patch(`/inventory/${id}/adjust`, JSON.stringify(change), { headers: { 'Content-Type': 'application/json' } }); load(); } catch { alert('Error adjusting stock'); } };

  const handleArchive = async (id) => { if (confirm('Archive this item? It will be hidden from the active inventory.')) { try { await API.delete(`/inventory/${id}`); load(); } catch { alert('Error archiving item'); } } };

  const handleUnarchive = async (id) => { if (confirm('Restore this item to active inventory?')) { try { await API.patch(`/inventory/${id}/unarchive`); load(); } catch { alert('Error restoring item'); } } };

  const stockStatus = (item) => {
    const mat = materials.find(m => m.materialId === item.materialId);
    const reorder = mat?.reorderLevel || 50;
    if (item.quantityOnHand <= reorder * 0.5) return <span className="badge badge-danger">Critical</span>;
    if (item.quantityOnHand <= reorder) return <span className="badge badge-warning">Low Stock</span>;
    return <span className="badge badge-success">In Stock</span>;
  };

  const filtered = items.filter(i => i.materialName?.toLowerCase().includes(search.toLowerCase()) || i.materialCode?.toLowerCase().includes(search.toLowerCase()));

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  return (
    <>
      <div className="stats-grid">
        <div className="card stat-card"><div className="stat-icon blue">🗃️</div><div className="stat-value">{items.length}</div><div className="stat-label">Total Items</div></div>
        <div className="card stat-card"><div className="stat-icon green">✅</div><div className="stat-value">{items.reduce((a,i)=>a+i.quantityOnHand,0).toLocaleString()}</div><div className="stat-label">Total On Hand</div></div>
        <div className="card stat-card"><div className="stat-icon amber">📦</div><div className="stat-value">{items.reduce((a,i)=>a+i.quantityReserved,0).toLocaleString()}</div><div className="stat-label">Reserved</div></div>
        <div className="card stat-card"><div className="stat-icon purple">✨</div><div className="stat-value">{items.reduce((a,i)=>a+i.quantityAvailable,0).toLocaleString()}</div><div className="stat-label">Available</div></div>
      </div>
      <div className="toolbar">
        <div className="search-box"><span className="search-icon">🔍</span><input className="form-input" placeholder="Search inventory..." value={search} onChange={e => setSearch(e.target.value)} /></div>
        <div className="d-flex ms-auto" style={{ gap: '12px' }}>
          <button className={`btn ${showArchived ? 'btn-primary' : 'btn-outline-primary'}`} onClick={() => setShowArchived(!showArchived)}>
              {showArchived ? 'View Active' : 'View Archived'}
          </button>
          <button className="btn btn-primary" onClick={() => setShowModal(true)}>+ Add Item</button>
        </div>
      </div>
      <div className="table-container">
        <table className="data-table">
          <thead><tr><th>Code</th><th>Material</th><th>On Hand</th><th>Reserved</th><th>Available</th><th>Location</th><th>Batch</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>
            {filtered.map(i => (
              <tr key={i.inventoryId}>
                <td style={{fontWeight:600,color:'var(--accent)'}}>{i.materialCode}</td><td style={{fontWeight:600}}>{i.materialName}</td>
                <td>{i.quantityOnHand}</td><td>{i.quantityReserved}</td><td style={{fontWeight:600}}>{i.quantityAvailable}</td>
                <td>{i.warehouseLocation || '—'}</td><td style={{fontSize:12,color:'var(--text-muted)'}}>{i.batchNumber || '—'}</td>
                <td>{stockStatus(i)}</td>
                <td style={{display:'flex',gap:4}}>
                  {!i.isActive ? (
                    <button className="btn btn-info btn-sm" onClick={()=>handleUnarchive(i.inventoryId)} title="Unarchive">📤</button>
                  ) : (
                    <>
                      <button className="btn btn-success btn-sm" onClick={()=>{const q=prompt('Add stock (+qty):');if(q)handleAdjust(i.inventoryId,parseInt(q));}}>+</button>
                      <button className="btn btn-ghost btn-sm" onClick={()=>{const q=prompt('Remove stock (-qty):');if(q)handleAdjust(i.inventoryId,-Math.abs(parseInt(q)));}}>−</button>
                      <button className="btn btn-danger btn-sm" onClick={()=>handleArchive(i.inventoryId)} title="Archive">📁</button>
                    </>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {showModal && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <div className="modal-header"><span className="modal-title">Add Inventory Item</span><button className="modal-close" onClick={() => setShowModal(false)}>✕</button></div>
            <form onSubmit={handleSubmit}>
              <div className="form-group"><label className="form-label">Material</label>
                {materials.length > 0 ? (
                  <select className="form-select" required value={form.materialId} onChange={e => setForm({...form, materialId: e.target.value})}>
                    <option value="">Select material...</option>
                    {materials.map(m => <option key={m.materialId} value={m.materialId}>{m.materialCode} — {m.materialName}</option>)}
                  </select>
                ) : (
                  <input className="form-input" type="number" required value={form.materialId} onChange={e => setForm({...form, materialId: e.target.value})} placeholder="Material ID" />
                )}
              </div>
              <div className="form-group"><label className="form-label">Quantity On Hand</label><input className="form-input" type="number" required value={form.quantityOnHand} onChange={e => setForm({...form, quantityOnHand: e.target.value})} /></div>
              <div className="form-group"><label className="form-label">Warehouse Location</label><input className="form-input" value={form.warehouseLocation} onChange={e => setForm({...form, warehouseLocation: e.target.value})} placeholder="e.g. Warehouse A-1" /></div>
              <div className="form-group"><label className="form-label">Batch Number</label><input className="form-input" value={form.batchNumber} onChange={e => setForm({...form, batchNumber: e.target.value})} placeholder="e.g. BATCH-2026-001" /></div>
              <div className="modal-actions"><button type="button" className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button><button type="submit" className="btn btn-primary">Add</button></div>
            </form>
          </div>
        </div>
      )}
    </>
  );
}
