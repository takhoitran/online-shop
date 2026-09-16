import { useState, type FormEvent } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { extractErrorMessage } from '../api/client';

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await login(username, password);
      const from = (location.state as { from?: string | { pathname?: string; search?: string } })?.from;
      let redirectTo = '/';
      if (typeof from === 'string') {
        redirectTo = from;
      } else if (from && typeof from === 'object' && from.pathname) {
        redirectTo = from.pathname + (from.search ?? '');
      }
      navigate(redirectTo, { replace: true });
    } catch (err) {
      setError(extractErrorMessage(err, 'Login failed. Please check your username/password.'));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="form-card">
      <h1>Log in</h1>
      {error && <div className="alert error">{error}</div>}
      <form onSubmit={handleSubmit}>
        <div className="field">
          <label htmlFor="username">Username</label>
          <input id="username" value={username} onChange={(e) => setUsername(e.target.value)} required autoFocus />
        </div>
        <div className="field">
          <label htmlFor="password">Password</label>
          <input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </div>
        <button className="btn" type="submit" disabled={submitting} style={{ width: '100%' }}>
          {submitting ? 'Logging in…' : 'Log in'}
        </button>
      </form>
      <p className="form-footer-note">
        Don't have an account? <Link to="/register">Sign up</Link>
      </p>
    </div>
  );
}
