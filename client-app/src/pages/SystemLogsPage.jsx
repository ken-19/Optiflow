import { useState, useEffect } from 'react';
import API from '../api/axios';

const fallbackLogs = [
  { logId: 1, action: 'User Login', userId: 1, username: 'superadmin', logLevel: 'Info', message: 'Successful login from 192.168.1.1', createdAt: '2026-04-26T14:30:00' },
  { logId: 2, action: 'Work Order Created', userId: 2, username: 'planner1', logLevel: 'Info', message: 'Created WO-2026-005', createdAt: '2026-04-26T14:15:00' },
  { logId: 3, action: 'Low Stock Alert', userId: null, username: 'System', logLevel: 'Warning', message: 'Industrial Paint below reorder level', createdAt: '2026-04-26T13:00:00' },
  { logId: 4, action: 'Failed Login', userId: null, username: 'unknown', logLevel: 'Error', message: 'Invalid credentials for user test', createdAt: '2026-04-26T12:45:00' },
  { logId: 5, action: 'Database Backup', userId: null, username: 'System', logLevel: 'Info', message: 'Automated daily backup completed', createdAt: '2026-04-26T02:00:00' },
];

export default function SystemLogsPage() {
  const [logs, setLogs] = useState([]);
  const [backups, setBackups] = useState([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState('logs');
  const [filter, setFilter] = useState('');
  const [backingUp, setBackingUp] = useState(false);

  const load = () => {
    Promise.all([
      API.get('/system-logs').catch(() => ({ data: { logs: fallbackLogs } })),
      API.get('/system-logs/backups').catch(() => ({ data: [] })),
    ]).then(([logRes, bkRes]) => {
      setLogs(logRes.data?.logs || logRes.data || []);
      setBackups(bkRes.data || []);
    }).finally(() => setLoading(false));
  };
  useEffect(load, []);

  const createBackup = async () => {
    setBackingUp(true);
    try { await API.post('/system-logs/backups'); load(); } catch { alert('Backup failed'); }
    finally { setBackingUp(false); }
  };

  const lvlBadge = (l) => { const m = { Info: 'badge-info', Warning: 'badge-warning', Error: 'badge-danger' }; return <span className={`badge ${m[l]||'badge-info'}`}>{l}</span>; };

  const filtered = filter ? logs.filter(l => l.logLevel === filter) : logs;

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  return (
    <>
      <div className="stats-grid">
        <div className="card stat-card"><div className="stat-icon blue">📝</div><div className="stat-value">{logs.length}</div><div className="stat-label">Total Logs</div></div>
        <div className="card stat-card"><div className="stat-icon amber">⚠️</div><div className="stat-value">{logs.filter(l=>l.logLevel==='Warning').length}</div><div className="stat-label">Warnings</div></div>
        <div className="card stat-card"><div className="stat-icon red">❌</div><div className="stat-value">{logs.filter(l=>l.logLevel==='Error').length}</div><div className="stat-label">Errors</div></div>
        <div className="card stat-card"><div className="stat-icon green">💾</div><div className="stat-value">{backups.length}</div><div className="stat-label">Backups</div></div>
      </div>
      <div className="toolbar">
        <div style={{display:'flex',gap:8}}>
          <button className={`btn ${tab==='logs'?'btn-primary':'btn-ghost'}`} onClick={()=>setTab('logs')}>📝 Activity Logs</button>
          <button className={`btn ${tab==='backups'?'btn-primary':'btn-ghost'}`} onClick={()=>setTab('backups')}>💾 Backups</button>
        </div>
        <div style={{display:'flex',gap:8}}>
          {tab==='logs' && <select className="form-select" style={{width:130}} value={filter} onChange={e=>setFilter(e.target.value)}><option value="">All Levels</option><option value="Info">Info</option><option value="Warning">Warning</option><option value="Error">Error</option></select>}
          {tab==='backups' && <button className="btn btn-primary" onClick={createBackup} disabled={backingUp}>{backingUp ? '⏳ Creating...' : '💾 Create Backup'}</button>}
        </div>
      </div>
      {tab === 'logs' ? (
        <div className="card">
          <div className="table-container" style={{border:'none'}}>
            <table className="data-table">
              <thead><tr><th>Time</th><th>Action</th><th>User & Role</th><th>Level</th><th>Details</th></tr></thead>
              <tbody>{filtered.map(l => (
                <tr key={l.logId}>
                  <td style={{fontSize:12,color:'var(--text-muted)',whiteSpace:'nowrap'}}>{l.createdAt?.replace('T',' ').substring(0,19)}</td>
                  <td style={{fontWeight:600}}>{l.action}</td>
                  <td>
                    <div>{l.username || 'System'}</div>
                    <div style={{fontSize:11,color:'var(--text-muted)'}}>{l.role || 'Automated'}</div>
                  </td>
                  <td>{lvlBadge(l.logLevel)}</td>
                  <td style={{fontSize:13,color:'var(--text-secondary)',maxWidth:300,overflow:'hidden',textOverflow:'ellipsis'}}>{l.message}</td>
                </tr>
              ))}</tbody>
            </table>
          </div>
        </div>
      ) : (
        <div className="card">
          <div className="card-header"><span className="card-title">Database Backups</span></div>
          {backups.length === 0 ? (
            <div style={{textAlign:'center',padding:40,color:'var(--text-muted)'}}>
              <div style={{fontSize:48,marginBottom:16}}>💾</div>
              <p>No backups found. Click "Create Backup" to generate one.</p>
            </div>
          ) : (
            <div className="table-container" style={{border:'none'}}>
              <table className="data-table">
                <thead><tr><th>Name</th><th>Status</th><th>Created</th><th>Path</th></tr></thead>
                <tbody>{backups.map(b => (
                  <tr key={b.backupId}>
                    <td style={{fontWeight:600}}>{b.backupName}</td>
                    <td><span className="badge badge-success">{b.status}</span></td>
                    <td style={{fontSize:12,color:'var(--text-muted)'}}>{b.createdAt?.replace('T',' ').substring(0,19)}</td>
                    <td style={{fontSize:12,color:'var(--text-muted)'}}>{b.filePath}</td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </>
  );
}
