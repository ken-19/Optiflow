import { useState, useEffect } from 'react';
import API from '../api/axios';
import { useAuth } from '../contexts/AuthContext';

export default function UsersPage() {
  const [users, setUsers] = useState([]);
  const [availableRoles, setAvailableRoles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editUser, setEditUser] = useState(null);
  const [search, setSearch] = useState('');
  const [form, setForm] = useState({ username: '', email: '', password: '', firstName: '', lastName: '', roleId: 8 });
  const { hasRole } = useAuth();

  const load = () => {
    Promise.all([
      API.get('/users').catch(() => ({ data: [] })),
      API.get('/users/roles').catch(() => ({ data: [] }))
    ]).then(([usersRes, rolesRes]) => {
      setUsers(usersRes.data);
      setAvailableRoles(rolesRes.data);
    }).finally(() => setLoading(false));
  };
  useEffect(load, []);

  const openCreate = () => {
    const defaultRoleId = availableRoles.length > 0 ? availableRoles[availableRoles.length - 1].roleId : 8;
    setForm({ username: '', email: '', password: '', firstName: '', lastName: '', roleId: defaultRoleId });
    setEditUser(null);
    setShowModal(true);
  };
  const openEdit = (u) => { setForm({ email: u.email, firstName: u.firstName || '', lastName: u.lastName || '', phone: u.phone || '', isActive: u.isActive }); setEditUser(u); setShowModal(true); };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (editUser) {
        await API.put(`/users/${editUser.userId}`, { email: form.email, firstName: form.firstName, lastName: form.lastName, phone: form.phone, isActive: form.isActive });
      } else {
        await API.post('/users', { username: form.username, email: form.email, password: form.password, firstName: form.firstName, lastName: form.lastName, roleId: parseInt(form.roleId) });
      }
      setShowModal(false); load();
    } catch (err) { alert(err.response?.data?.message || err.response?.data || 'Error saving user'); }
  };

  const toggleActive = async (u) => { try { await API.put(`/users/${u.userId}`, { isActive: !u.isActive }); load(); } catch { alert('Error'); } };
  const deleteUser = async (id) => { if (confirm('Delete this user?')) { try { await API.delete(`/users/${id}`); load(); } catch { alert('Error'); } } };

  const filtered = users.filter(u => (u.firstName + ' ' + u.lastName + ' ' + u.username + ' ' + u.email).toLowerCase().includes(search.toLowerCase()));

  if (loading) return <div className="page-loading"><div className="spinner"></div></div>;

  return (
    <>
      <div className="stats-grid">
        <div className="card stat-card"><div className="stat-icon purple">👥</div><div className="stat-value">{users.length}</div><div className="stat-label">Total Users</div></div>
        <div className="card stat-card"><div className="stat-icon green">🟢</div><div className="stat-value">{users.filter(u=>u.isActive).length}</div><div className="stat-label">Active</div></div>
        <div className="card stat-card"><div className="stat-icon red">🔴</div><div className="stat-value">{users.filter(u=>!u.isActive).length}</div><div className="stat-label">Inactive</div></div>
        <div className="card stat-card"><div className="stat-icon blue">🔐</div><div className="stat-value">{new Set(users.flatMap(u=>u.roles||[])).size}</div><div className="stat-label">Roles in Use</div></div>
      </div>
      <div className="toolbar">
        <div className="search-box"><span className="search-icon">🔍</span><input className="form-input" placeholder="Search users..." value={search} onChange={e=>setSearch(e.target.value)} /></div>
        <button className="btn btn-primary" onClick={openCreate}>+ Add User</button>
      </div>
      <div className="card">
        <div className="table-container" style={{border:'none'}}>
          <table className="data-table">
            <thead><tr><th>User</th><th>Email</th><th>Role</th><th>Status</th><th>Joined</th><th>Actions</th></tr></thead>
            <tbody>{filtered.map(u => (
              <tr key={u.userId}>
                <td><div style={{display:'flex',alignItems:'center',gap:10}}><div className="user-avatar" style={{width:32,height:32,fontSize:12}}>{(u.firstName||u.username)?.[0]?.toUpperCase()||'U'}</div><div><div style={{fontWeight:600}}>{u.firstName||''} {u.lastName||''}</div><div style={{fontSize:12,color:'var(--text-muted)'}}>@{u.username}</div></div></div></td>
                <td>{u.email}</td>
                <td><span className="badge badge-purple">{u.roles?.[0] || 'No role'}</span></td>
                <td>{u.isActive ? <span className="badge badge-success">Active</span> : <span className="badge badge-danger">Inactive</span>}</td>
                <td style={{fontSize:12,color:'var(--text-muted)'}}>{u.createdAt?.split('T')[0]}</td>
                <td>
                  <div style={{display:'flex',gap:4}}>
                    <button className="btn btn-ghost btn-sm" onClick={()=>openEdit(u)}>✏️</button>
                    <button className="btn btn-ghost btn-sm" onClick={()=>toggleActive(u)}>{u.isActive?'🔒':'🔓'}</button>
                    {hasRole('Super Admin') && <button className="btn btn-danger btn-sm" onClick={()=>deleteUser(u.userId)}>🗑</button>}
                  </div>
                </td>
              </tr>
            ))}</tbody>
          </table>
        </div>
      </div>
      {showModal && (
        <div className="modal-overlay" onClick={()=>setShowModal(false)}>
          <div className="modal" onClick={e=>e.stopPropagation()}>
            <div className="modal-header"><span className="modal-title">{editUser ? 'Edit User' : 'Add New User'}</span><button className="modal-close" onClick={()=>setShowModal(false)}>✕</button></div>
            <form onSubmit={handleSubmit}>
              {!editUser && <div className="form-group"><label className="form-label">Username</label><input className="form-input" required value={form.username} onChange={e=>setForm({...form,username:e.target.value})} /></div>}
              <div style={{display:'grid',gridTemplateColumns:'1fr 1fr',gap:12}}>
                <div className="form-group"><label className="form-label">First Name</label><input className="form-input" value={form.firstName} onChange={e=>setForm({...form,firstName:e.target.value})} /></div>
                <div className="form-group"><label className="form-label">Last Name</label><input className="form-input" value={form.lastName} onChange={e=>setForm({...form,lastName:e.target.value})} /></div>
              </div>
              <div className="form-group"><label className="form-label">Email</label><input className="form-input" type="email" required value={form.email} onChange={e=>setForm({...form,email:e.target.value})} /></div>
              {!editUser && <div className="form-group"><label className="form-label">Password</label><input className="form-input" type="password" required minLength={6} value={form.password} onChange={e=>setForm({...form,password:e.target.value})} /></div>}
              {!editUser && <div className="form-group"><label className="form-label">Role</label><select className="form-select" value={form.roleId} onChange={e=>setForm({...form,roleId:e.target.value})}>
                {availableRoles.map(r => <option key={r.roleId} value={r.roleId}>{r.roleName}</option>)}
              </select></div>}
              {editUser && <div className="form-group"><label className="form-label" style={{display:'flex',alignItems:'center',gap:8}}><input type="checkbox" checked={form.isActive} onChange={e=>setForm({...form,isActive:e.target.checked})} /> Active</label></div>}
              <div className="modal-actions"><button type="button" className="btn btn-ghost" onClick={()=>setShowModal(false)}>Cancel</button><button type="submit" className="btn btn-primary">{editUser?'Save':'Create User'}</button></div>
            </form>
          </div>
        </div>
      )}
    </>
  );
}
