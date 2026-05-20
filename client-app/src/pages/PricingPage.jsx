import { useState, useEffect } from 'react';
import API from '../api/axios';
import { useAuth } from '../contexts/AuthContext';

const plans = [
  {
    name: 'Basic Plan',
    price: '999',
    period: '/mo',
    features: [
      { text: 'Limited users (up to 5)', included: true },
      { text: 'Basic ERP features', included: true },
      { text: 'Inventory tracking', included: true },
      { text: 'Full MES modules', included: false },
      { text: 'Advanced analytics', included: false },
    ],
    buttonText: 'Buy Now',
    color: 'rgba(255, 255, 255, 0.05)',
    btnClass: 'btn-outline'
  },
  {
    name: 'Standard Plan',
    price: '1,499',
    period: '/mo',
    features: [
      { text: 'Moderate users (up to 20)', included: true },
      { text: 'Full ERP modules', included: true },
      { text: 'Shop floor tracking', included: true },
      { text: 'Basic analytics', included: true },
      { text: 'Custom reporting', included: false },
    ],
    buttonText: 'Buy Now',
    color: 'rgba(255, 255, 255, 0.05)',
    popular: true,
    btnClass: 'btn-gradient'
  },
  {
    name: 'Premium Plan',
    price: '1,999',
    period: '/mo',
    features: [
      { text: 'Unlimited users', included: true },
      { text: 'All ERP + MES features', included: true },
      { text: 'Advanced analytics', included: true },
      { text: 'Custom reporting', included: true },
      { text: 'Priority 24/7 support', included: true },
    ],
    buttonText: 'Buy Now',
    color: 'rgba(255, 255, 255, 0.05)',
    btnClass: 'btn-outline'
  }
];

export default function PricingPage() {
  const { user } = useAuth();
  const [loading, setLoading] = useState(null);
  const [currentSubscription, setCurrentSubscription] = useState(null);

  useEffect(() => {
    const fetchSubscription = async () => {
      try {
        const response = await API.get('/payments/current-subscription');
        setCurrentSubscription(response.data);
      } catch (err) {
        console.error('Failed to fetch subscription status', err);
      }
    };
    fetchSubscription();
  }, []);

  const handleSubscribe = async (planName) => {
    setLoading(planName);
    try {
      const response = await API.post('/payments/create-checkout', { planName });
      if (response.data.url) {
        window.location.href = response.data.url; // Redirect to Stripe
      }
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to initiate checkout');
    } finally {
      setLoading(null);
    }
  };

  const isCurrentPlan = (planName) => currentSubscription?.subscriptionPlan === planName;

  return (
    <div className="pricing-wrapper" style={{ 
      minHeight: '100%', 
      background: '#0f172a', // Deep dark background
      color: 'white',
      padding: '60px 20px'
    }}>
      <div style={{ textAlign: 'center', marginBottom: 60 }}>
        <h1 style={{ fontSize: 42, fontWeight: 800, marginBottom: 16, background: 'linear-gradient(to right, #f472b6, #a78bfa)', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent' }}>
          Subscription Plans
        </h1>
        <p style={{ color: '#94a3b8', fontSize: 18 }}>Power up your manufacturing with OptiFlow</p>
        
        {currentSubscription && currentSubscription.subscriptionPlan !== 'None' && (
          <div style={{ 
            display: 'inline-block', 
            marginTop: 20, 
            padding: '12px 24px', 
            borderRadius: 12,
            border: '1px solid rgba(167, 139, 250, 0.3)',
            background: 'rgba(167, 139, 250, 0.1)'
          }}>
            <p style={{ margin: 0, fontWeight: 600 }}>
              Current Plan: <span style={{ color: '#c084fc' }}>{currentSubscription.subscriptionPlan}</span>
            </p>
            {currentSubscription.subscriptionExpiry && (
              <p style={{ margin: 0, fontSize: 13, color: '#94a3b8' }}>
                Ends on: {new Date(currentSubscription.subscriptionExpiry).toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' })}
              </p>
            )}
          </div>
        )}
      </div>

      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', 
        gap: 32,
        maxWidth: 1200,
        margin: '0 auto',
        padding: '0 20px'
      }}>
        {plans.map((plan) => (
          <div key={plan.name} className={`pricing-card-v2 ${plan.popular ? 'popular' : ''} ${isCurrentPlan(plan.name) ? 'active' : ''}`} style={{
            position: 'relative',
            padding: '50px 40px',
            borderRadius: 24,
            background: isCurrentPlan(plan.name) ? 'rgba(16, 185, 129, 0.05)' : 'rgba(30, 41, 59, 0.5)',
            border: isCurrentPlan(plan.name) ? '1px solid #10b981' : (plan.popular ? '1px solid #f472b6' : '1px solid rgba(255,255,255,0.1)'),
            display: 'flex',
            flexDirection: 'column',
            transition: 'transform 0.3s ease, box-shadow 0.3s ease'
          }}>
            {plan.popular && !isCurrentPlan(plan.name) && (
              <div style={{
                position: 'absolute',
                top: 0,
                left: '50%',
                transform: 'translate(-50%, -50%)',
                background: 'linear-gradient(to right, #f472b6, #a78bfa)',
                color: 'white',
                padding: '8px 20px',
                borderRadius: 20,
                fontSize: 14,
                fontWeight: 700,
                textTransform: 'uppercase',
                boxShadow: '0 10px 20px rgba(244, 114, 182, 0.3)'
              }}>Most Popular</div>
            )}

            {isCurrentPlan(plan.name) && (
              <div style={{
                position: 'absolute',
                top: 0,
                left: '50%',
                transform: 'translate(-50%, -50%)',
                background: '#10b981',
                color: 'white',
                padding: '8px 20px',
                borderRadius: 20,
                fontSize: 14,
                fontWeight: 700,
                textTransform: 'uppercase'
              }}>Active Plan</div>
            )}
            
            <div style={{ textAlign: 'center', marginBottom: 30 }}>
              <h3 style={{ fontSize: 18, color: '#94a3b8', textTransform: 'uppercase', letterSpacing: 1, marginBottom: 16 }}>{plan.name}</h3>
              <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'baseline' }}>
                <span style={{ fontSize: 56, fontWeight: 800 }}>₱{plan.price}</span>
                <span style={{ color: '#64748b', fontSize: 18, marginLeft: 4 }}>{plan.period}</span>
              </div>
            </div>

            <div style={{ borderTop: '1px solid rgba(255,255,255,0.05)', padding: '30px 0', flex: 1 }}>
              <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
                {plan.features.map((feature, idx) => (
                  <li key={idx} style={{ 
                    display: 'flex', 
                    alignItems: 'center', 
                    marginBottom: 16, 
                    fontSize: 15,
                    color: feature.included ? '#e2e8f0' : '#475569',
                    textDecoration: feature.included ? 'none' : 'line-through'
                  }}>
                    <span style={{ 
                      marginRight: 12, 
                      color: feature.included ? '#10b981' : '#ef4444',
                      fontSize: 18
                    }}>
                      {feature.included ? '✓' : '✕'}
                    </span>
                    {feature.text}
                  </li>
                ))}
              </ul>
            </div>

            <button 
              className={`buy-button ${plan.btnClass}`}
              style={{ 
                width: '100%', 
                padding: '16px',
                borderRadius: 14,
                fontSize: 16,
                fontWeight: 700,
                border: plan.btnClass === 'btn-outline' ? '1px solid #334155' : 'none',
                background: isCurrentPlan(plan.name) ? '#10b981' : (plan.btnClass === 'btn-gradient' ? 'linear-gradient(to right, #ec4899, #8b5cf6)' : 'rgba(30, 41, 59, 1)'),
                color: 'white',
                cursor: isCurrentPlan(plan.name) ? 'default' : 'pointer',
                transition: 'all 0.2s ease',
                boxShadow: plan.btnClass === 'btn-gradient' ? '0 10px 30px rgba(139, 92, 246, 0.2)' : 'none'
              }}
              onClick={() => !isCurrentPlan(plan.name) && handleSubscribe(plan.name)}
              disabled={loading !== null || isCurrentPlan(plan.name)}
            >
              {isCurrentPlan(plan.name) ? 'Plan Active' : (loading === plan.name ? 'Processing...' : plan.buttonText)}
            </button>
          </div>
        ))}
      </div>

      <style dangerouslySetInnerHTML={{ __html: `
        .pricing-card-v2:hover {
          transform: translateY(-10px);
          box-shadow: 0 20px 40px rgba(0,0,0,0.4);
        }
        .buy-button:hover:not(:disabled) {
          transform: scale(1.02);
          opacity: 0.9;
        }
        .btn-outline:hover {
          background: rgba(255,255,255,0.05) !important;
          border-color: #64748b !important;
        }
      `}} />
    </div>
  );
}
