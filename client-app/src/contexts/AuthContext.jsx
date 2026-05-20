import { createContext, useContext, useState, useEffect } from 'react';
import API from '../api/axios';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem('optiflow_user');
    if (stored) {
      try {
        setUser(JSON.parse(stored));
      } catch { /* corrupted data – clear */ 
        localStorage.removeItem('optiflow_user');
        localStorage.removeItem('optiflow_token');
      }
    }
    setLoading(false);
  }, []);

  const login = async (username, password) => {
    const res = await API.post('/auth/login', { username, password });
    // Backend returns { token, expiresAt, user: { userId, username, email, ... } }
    const { token, expiresAt, user: userInfo } = res.data;
    localStorage.setItem('optiflow_token', token);
    const userData = {
      userId: userInfo.userId,
      username: userInfo.username,
      email: userInfo.email,
      firstName: userInfo.firstName,
      lastName: userInfo.lastName,
      fullName: `${userInfo.firstName || ''} ${userInfo.lastName || ''}`.trim() || userInfo.username,
      roles: userInfo.roles || [],
      permissions: userInfo.permissions || [],
      companyId: userInfo.companyId,
      companyName: userInfo.companyName,
      expiresAt,
    };
    localStorage.setItem('optiflow_user', JSON.stringify(userData));
    setUser(userData);
    return userData;
  };

    const register = async (data) => {
      const res = await API.post('/auth/register', data);
      const { token, expiresAt, user: userInfo } = res.data;
      localStorage.setItem('optiflow_token', token);
      const userData = {
        userId: userInfo.userId,
        username: userInfo.username,
        email: userInfo.email,
        firstName: userInfo.firstName,
        lastName: userInfo.lastName,
        fullName: `${userInfo.firstName || ''} ${userInfo.lastName || ''}`.trim() || userInfo.username,
        roles: userInfo.roles || [],
        permissions: userInfo.permissions || [],
        companyId: userInfo.companyId,
        companyName: userInfo.companyName,
        expiresAt,
      };
      localStorage.setItem('optiflow_user', JSON.stringify(userData));
      setUser(userData);
      return userData;
    };

    const registerAdmin = async (data) => {
      const res = await API.post('/auth/register-admin', data);
      const { token, expiresAt, user: userInfo } = res.data;
      localStorage.setItem('optiflow_token', token);
      const userData = {
        userId: userInfo.userId,
        username: userInfo.username,
        email: userInfo.email,
        firstName: userInfo.firstName,
        lastName: userInfo.lastName,
        fullName: `${userInfo.firstName || ''} ${userInfo.lastName || ''}`.trim() || userInfo.username,
        roles: userInfo.roles || [],
        permissions: userInfo.permissions || [],
        companyId: userInfo.companyId,
        companyName: userInfo.companyName,
        expiresAt,
      };
      localStorage.setItem('optiflow_user', JSON.stringify(userData));
      setUser(userData);
      return userData;
    };

  const logout = () => {
    localStorage.removeItem('optiflow_token');
    localStorage.removeItem('optiflow_user');
    setUser(null);
  };

  const hasRole = (role) => user?.roles?.includes(role);
  const hasPermission = (perm) => user?.permissions?.includes(perm);
  const hasAnyRole = (...roles) => roles.some(r => user?.roles?.includes(r));

  return (
    <AuthContext.Provider value={{ user, loading, login, register, registerAdmin, logout, hasRole, hasPermission, hasAnyRole }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
