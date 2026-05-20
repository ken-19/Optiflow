import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import API from '../api/axios';

export default function PaymentSuccessPage() {
  const [searchParams] = useSearchParams();
  const [status, setStatus] = useState('verifying');
  const sessionId = searchParams.get('session_id');

  useEffect(() => {
    if (sessionId) {
      API.get(`/payments/success-handler?session_id=${sessionId}`)
        .then(() => setStatus('success'))
        .catch(() => setStatus('error'));
    }
  }, [sessionId]);

  return (
    <div style={{ 
      display: 'flex', 
      flexDirection: 'column', 
      alignItems: 'center', 
      justifyContent: 'center', 
      minHeight: '60vh',
      textAlign: 'center' 
    }}>
      {status === 'verifying' && (
        <>
          <div className="spinner" style={{ width: 60, height: 60, marginBottom: 20 }}></div>
          <h2>Verifying your payment...</h2>
        </>
      )}

      {status === 'success' && (
        <div className="card" style={{ padding: 50, maxWidth: 500 }}>
          <div style={{ fontSize: 64, marginBottom: 20 }}>Γ£ô∩╕Å</div>
          <h1 style={{ marginBottom: 16 }}>Payment Successful!</h1>
          <p style={{ color: 'var(--text-muted)', marginBottom: 32 }}>
            Your subscription has been updated. You now have access to all the features of your new plan.
          </p>
          <Link to="/dashboard" className="btn btn-primary">Go to Dashboard</Link>
        </div>
      )}

      {status === 'error' && (
        <div className="card" style={{ padding: 50, maxWidth: 500 }}>
          <div style={{ fontSize: 64, marginBottom: 20 }}>Γ¥î</div>
          <h1 style={{ marginBottom: 16 }}>Something went wrong</h1>
          <p style={{ color: 'var(--text-muted)', marginBottom: 32 }}>
            We couldn't verify your payment session. Please contact support if you believe this is an error.
          </p>
          <Link to="/pricing" className="btn btn-ghost">Back to Pricing</Link>
        </div>
      )}
    </div>
  );
}
