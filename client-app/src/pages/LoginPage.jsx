import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import logo from '../assets/logo.png';

export default function LoginPage() {
  const [tab, setTab] = useState('login'); // login | register | registerAdmin
  const [form, setForm] = useState({ username: '', password: '', email: '', firstName: '', lastName: '', companyName: '', subscriptionPlan: 'Basic Plan' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login, register, registerAdmin, logout } = useAuth();
  const navigate = useNavigate();
  const formRef = useRef(null);

  const handleSelectPlan = (planName) => {
    setTab('registerAdmin');
    setForm((prev) => ({ ...prev, subscriptionPlan: planName }));
    setTimeout(() => {
      formRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 100);
  };

  const handleLogin = async (e) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      await login(form.username, form.password);
      navigate('/dashboard');
    } catch (err) {
      setError(err.response?.data?.message || 'Invalid credentials. Try superadmin / Admin@123');
    } finally { setLoading(false); }
  };

  const handleRegister = async (e) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      if (tab === 'registerAdmin') {
        await registerAdmin({ username: form.username, email: form.email, password: form.password, firstName: form.firstName, lastName: form.lastName, companyName: form.companyName, subscriptionPlan: form.subscriptionPlan });
        logout();
        setTab('login');
        alert('Subscription successful! Please log in with your new admin account.');
      } else {
        await register({ username: form.username, email: form.email, password: form.password, firstName: form.firstName, lastName: form.lastName });
        navigate('/dashboard');
      }
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed. Username or email may already exist.');
    } finally { setLoading(false); }
  };

  const u = (field, val) => setForm({ ...form, [field]: val });

  return (
    <div className="landing-page">
      <nav className="landing-nav">
        <div className="nav-logo">
          <img src={logo} alt="OptiFlow Logo" style={{ width: 45, height: 45, objectFit: 'contain' }} />
          <span className="logo-text">OptiFlow</span>
        </div>
        <div className="nav-links">
          <a href="#features">Features</a>
          <a href="#modules">Modules</a>
          <a href="#pricing">Pricing</a>
        </div>
        <div className="nav-actions">
          <button onClick={() => { setTab('login'); formRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' }); }} className="btn btn-ghost">Sign In</button>
          <button onClick={() => { setTab('registerAdmin'); formRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' }); }} className="btn btn-primary">Get Started</button>
        </div>
      </nav>

      <main className="landing-main">
        {/* HERO SECTION */}
        <section className="hero-section">
          <div className="hero-content">
            <div className="badge badge-purple hero-badge">🚀 v2.0 Enterprise Release</div>
            <h1 className="hero-title">
              Intelligent Manufacturing,<br/>
              <span className="text-gradient">Seamless Operations.</span>
            </h1>
            <p className="hero-subtitle">
              OptiFlow unites ERP and MES to give you unprecedented real-time control over production, inventory, MRP, and quality on the shop floor.
            </p>
            <div className="hero-bullets">
              <div className="hero-bullet"><div className="bullet-icon">✔</div> <span>Real-time Production Tracking</span></div>
              <div className="hero-bullet"><div className="bullet-icon">✔</div> <span>AI-Driven MRP Suggestions</span></div>
              <div className="hero-bullet"><div className="bullet-icon">✔</div> <span>End-to-End Traceability</span></div>
              <div className="hero-bullet"><div className="bullet-icon">✔</div> <span>Strict Role-Based Security</span></div>
            </div>
            <div className="hero-buttons">
              <button onClick={() => { setTab('registerAdmin'); formRef.current?.scrollIntoView({ behavior: 'smooth', block: 'center' }); }} className="btn btn-primary btn-lg">Start Free Trial</button>
              <a href="#features" className="btn btn-ghost btn-lg">Explore Features</a>
            </div>
          </div>
          
          <div className="hero-form" ref={formRef}>
            <div className="login-card glass-card">
              <div className="logo-area" style={{marginBottom: '20px'}}>
                <h2 style={{fontSize: '20px', fontWeight: 700}}>{tab === 'login' ? 'Welcome Back' : (tab === 'registerAdmin' ? 'Subscribe' : 'Create Account')}</h2>
                <p style={{fontSize: '13px', color: 'var(--text-muted)'}}>{tab === 'login' ? 'Sign in to access your workspace' : 'Join OptiFlow to optimize your operations'}</p>
              </div>
              <div style={{ display: 'flex', gap: 0, marginBottom: 20, borderRadius: 8, overflow: 'hidden', border: '1px solid var(--border)', background: 'var(--bg-input)' }}>
                <button type="button" onClick={() => { setTab('login'); setError(''); }} className={`btn ${tab === 'login' ? 'btn-primary' : 'btn-ghost'}`} style={{ flex: 1, borderRadius: 0, border: 'none' }}>Sign In</button>
                <button type="button" onClick={() => { setTab('registerAdmin'); setError(''); }} className={`btn ${tab === 'registerAdmin' ? 'btn-primary' : 'btn-ghost'}`} style={{ flex: 1, borderRadius: 0, border: 'none' }}>Register</button>
              </div>
              {error && <div className="login-error">{error}</div>}
              {tab === 'login' ? (
                <form onSubmit={handleLogin} className="auth-form">
                  <div className="form-group"><label className="form-label">Username or Email</label><input id="login-username" className="form-input" value={form.username} onChange={e => u('username', e.target.value)} placeholder="Enter username or email" required /></div>
                  <div className="form-group"><label className="form-label">Password</label><input id="login-password" className="form-input" type="password" value={form.password} onChange={e => u('password', e.target.value)} placeholder="Enter password" required /></div>
                  <button id="login-submit" className="btn btn-primary btn-lg w-100" disabled={loading} style={{width: '100%'}}>{loading ? 'Signing in...' : 'Sign In'}</button>
                </form>
              ) : (
                <form onSubmit={handleRegister} className="auth-form">
                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                    <div className="form-group"><label className="form-label">First Name</label><input className="form-input" value={form.firstName} onChange={e => u('firstName', e.target.value)} placeholder="First name" /></div>
                    <div className="form-group"><label className="form-label">Last Name</label><input className="form-input" value={form.lastName} onChange={e => u('lastName', e.target.value)} placeholder="Last name" /></div>
                  </div>
                  {tab === 'registerAdmin' && (
                    <>
                      <div className="form-group"><label className="form-label">Company Name</label><input className="form-input" value={form.companyName} onChange={e => u('companyName', e.target.value)} placeholder="Your Company Name" required /></div>
                      <div className="form-group"><label className="form-label">Subscription Plan</label>
                        <select className="form-input" value={form.subscriptionPlan} onChange={e => u('subscriptionPlan', e.target.value)} required>
                          <option value="Basic Plan">Basic Plan - Limited features</option>
                          <option value="Standard Plan">Standard Plan - Moderate features</option>
                          <option value="Premium Plan">Premium Plan - Full access</option>
                        </select>
                      </div>
                    </>
                  )}
                  <div className="form-group"><label className="form-label">Username</label><input className="form-input" value={form.username} onChange={e => u('username', e.target.value)} placeholder="Choose username" required /></div>
                  <div className="form-group"><label className="form-label">Email</label><input className="form-input" type="email" value={form.email} onChange={e => u('email', e.target.value)} placeholder="your@email.com" required /></div>
                  <div className="form-group"><label className="form-label">Password</label><input className="form-input" type="password" value={form.password} onChange={e => u('password', e.target.value)} placeholder="Min 6 characters" required minLength={6} /></div>
                  <button className="btn btn-primary btn-lg w-100" disabled={loading} style={{width: '100%'}}>{loading ? 'Processing...' : (tab === 'registerAdmin' ? 'Subscribe & Create Admin' : 'Create Account')}</button>
                </form>
              )}
              <p className="auth-hint" style={{ textAlign: 'center', marginTop: 16, fontSize: 12, color: 'var(--text-muted)' }}>Default: superadmin / Admin@123</p>
            </div>
          </div>
        </section>

        {/* LOGO TICKER / TRUSTED BY */}
        <section className="trusted-section">
          <p>Trusted by forward-thinking manufacturers worldwide</p>
          <div className="logo-ticker">
             <div className="ticker-item">GlobalTech Mfg</div>
             <div className="ticker-item">AeroSpace Dynamics</div>
             <div className="ticker-item">NexGen Automotives</div>
             <div className="ticker-item">Quantum Robotics</div>
             <div className="ticker-item">Pinnacle Synthetics</div>
          </div>
        </section>

        {/* FEATURES SECTION */}
        <section id="features" className="info-section">
          <div className="section-header text-center">
            <div className="badge badge-info mb-3" style={{marginBottom: '12px'}}>Capabilities</div>
            <h2>Everything you need to scale</h2>
            <p>OptiFlow delivers a suite of deeply integrated modules designed to eliminate bottlenecks and provide total visibility into your operations.</p>
          </div>
          
          <div className="features-grid-modern">
            <div className="feature-box">
              <div className="feature-icon-wrapper purple">
                <span className="material-icon">⚙️</span>
              </div>
              <h3>Production Management</h3>
              <p>Track work orders in real-time, monitor machine efficiency, and reduce downtime with shop-floor execution tools.</p>
            </div>
            
            <div className="feature-box">
              <div className="feature-icon-wrapper blue">
                <span className="material-icon">📦</span>
              </div>
              <h3>Advanced Inventory</h3>
              <p>Automate stock movements, trace materials end-to-end, and set intelligent reorder points to prevent shortages.</p>
            </div>
            
            <div className="feature-box">
              <div className="feature-icon-wrapper green">
                <span className="material-icon">📊</span>
              </div>
              <h3>MRP Engine</h3>
              <p>Calculate precise material requirements based on demand forecasts and current stock to optimize your supply chain.</p>
            </div>
            
            <div className="feature-box">
              <div className="feature-icon-wrapper amber">
                <span className="material-icon">🛡️</span>
              </div>
              <h3>Quality Assurance</h3>
              <p>Enforce strict quality checks at every production stage. Log defects, track non-conformances, and maintain compliance.</p>
            </div>
            
            <div className="feature-box">
              <div className="feature-icon-wrapper red">
                <span className="material-icon">💰</span>
              </div>
              <h3>Costing & Analytics</h3>
              <p>Analyze direct and indirect costs to find your true margins. Generate comprehensive reports and custom dashboards.</p>
            </div>

            <div className="feature-box">
              <div className="feature-icon-wrapper gray">
                <span className="material-icon">🔐</span>
              </div>
              <h3>Enterprise Security</h3>
              <p>Granular Role-Based Access Control (RBAC) ensures sensitive data is protected. Comprehensive audit logs for all actions.</p>
            </div>
          </div>
        </section>

        {/* HOW IT WORKS / MODULES HIGHLIGHT */}
        <section id="modules" className="showcase-section">
           <div className="showcase-content">
              <div className="badge badge-success mb-3" style={{marginBottom: '12px'}}>The Workflow</div>
              <h2>Connect the top floor to the shop floor.</h2>
              <ul className="showcase-list">
                 <li>
                   <div className="step-number">1</div>
                   <div>
                     <h4>Plan with Precision</h4>
                     <p>Use the MRP engine to forecast demand and order materials right on time.</p>
                   </div>
                 </li>
                 <li>
                   <div className="step-number">2</div>
                   <div>
                     <h4>Execute Seamlessly</h4>
                     <p>Dispatch Work Orders and track real-time progress on the dashboard.</p>
                   </div>
                 </li>
                 <li>
                   <div className="step-number">3</div>
                   <div>
                     <h4>Ensure Quality</h4>
                     <p>Perform integrated quality inspections during and after production runs.</p>
                   </div>
                 </li>
              </ul>
           </div>
           <div className="showcase-image">
             <div className="ui-mockup">
               <div className="ui-header"><span></span><span></span><span></span></div>
               <div className="ui-body">
                 <div className="ui-sidebar">
                    <div className="ui-line"></div>
                    <div className="ui-line"></div>
                    <div className="ui-line"></div>
                 </div>
                 <div className="ui-main">
                    <div className="ui-card title-card"></div>
                    <div className="ui-grid">
                      <div className="ui-card stat"></div>
                      <div className="ui-card stat"></div>
                      <div className="ui-card stat"></div>
                    </div>
                    <div className="ui-card large-card"></div>
                 </div>
               </div>
             </div>
           </div>
        </section>

        {/* PRICING SECTION */}
        <section id="pricing" className="pricing-section">
          <div className="section-header text-center">
             <h2>Choose Your Plan</h2>
             <p className="pricing-subtitle">Select the perfect ERP-MES plan for your manufacturing needs</p>
          </div>
          
          <div className="pricing-grid">
            {/* Basic Plan */}
            <div className="pricing-card plan-basic">
              <div className="pricing-header">
                <h3 className="pricing-name">Basic Plan</h3>
                <div className="pricing-price">₱999<span>/mo</span></div>
              </div>
              <ul className="pricing-features">
                <li><span className="feature-icon included">✔</span> Limited users (up to 5)</li>
                <li><span className="feature-icon included">✔</span> Basic ERP features</li>
                <li><span className="feature-icon included">✔</span> Inventory tracking</li>
                <li className="excluded-item"><span className="feature-icon excluded">✖</span> Full MES modules</li>
                <li className="excluded-item"><span className="feature-icon excluded">✖</span> Advanced analytics</li>
              </ul>
              <button className="pricing-btn" onClick={() => handleSelectPlan('Basic Plan')}>Buy Now</button>
            </div>

            {/* Standard Plan */}
            <div className="pricing-card plan-standard popular">
              <div className="popular-badge">Most Popular</div>
              <div className="pricing-header">
                <h3 className="pricing-name">Standard Plan</h3>
                <div className="pricing-price">₱1,499<span>/mo</span></div>
              </div>
              <ul className="pricing-features">
                <li><span className="feature-icon included">✔</span> Moderate users (up to 20)</li>
                <li><span className="feature-icon included">✔</span> Full ERP modules</li>
                <li><span className="feature-icon included">✔</span> Shop floor tracking</li>
                <li><span className="feature-icon included">✔</span> Basic analytics</li>
                <li className="excluded-item"><span className="feature-icon excluded">✖</span> Custom reporting</li>
              </ul>
              <button className="pricing-btn" onClick={() => handleSelectPlan('Standard Plan')}>Buy Now</button>
            </div>

            {/* Premium Plan */}
            <div className="pricing-card plan-premium">
              <div className="pricing-header">
                <h3 className="pricing-name">Premium Plan</h3>
                <div className="pricing-price">₱1,999<span>/mo</span></div>
              </div>
              <ul className="pricing-features">
                <li><span className="feature-icon included">✔</span> Unlimited users</li>
                <li><span className="feature-icon included">✔</span> All ERP + MES features</li>
                <li><span className="feature-icon included">✔</span> Advanced analytics</li>
                <li><span className="feature-icon included">✔</span> Custom reporting</li>
                <li><span className="feature-icon included">✔</span> Priority 24/7 support</li>
              </ul>
              <button className="pricing-btn" onClick={() => handleSelectPlan('Premium Plan')}>Buy Now</button>
            </div>
          </div>
        </section>
      </main>

      <footer className="landing-footer">
        <div className="footer-content">
          <div className="footer-brand">
             <div className="nav-logo" style={{marginBottom: '16px'}}>
               <img src={logo} alt="OptiFlow Logo" style={{ width: 45, height: 45, objectFit: 'contain' }} />
               <span className="logo-text">OptiFlow</span>
             </div>
             <p style={{color: 'var(--text-muted)', fontSize: '14px', maxWidth: '300px'}}>Empowering manufacturers with intelligent, real-time control over their shop floor and supply chain.</p>
          </div>
          <div className="footer-links">
             <div className="link-group">
                <h4>Product</h4>
                <a href="#features">Features</a>
                <a href="#modules">Modules</a>
                <a href="#pricing">Pricing</a>
             </div>
             <div className="link-group">
                <h4>Company</h4>
                <a href="#">About Us</a>
                <a href="#">Contact</a>
                <a href="#">Careers</a>
             </div>
             <div className="link-group">
                <h4>Legal</h4>
                <a href="#">Privacy Policy</a>
                <a href="#">Terms of Service</a>
             </div>
          </div>
        </div>
        <div className="footer-bottom">
           <p>&copy; 2026 OptiFlow Systems. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
}
