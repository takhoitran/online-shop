import { useEffect, useState, type FormEvent } from 'react';
import { createStaffUser, getUsers } from '../../api/admin';
import { extractErrorMessage } from '../../api/client';
import type { UserSummary } from '../../types';
import { formatDateTime } from '../../utils/format';

export function AdminUsersPage() {
  const [users, setUsers] = useState<UserSummary[]>([]);
  const [loading, setLoading] = useState(true);

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [fullName, setFullName] = useState('');
  const [roleName, setRoleName] = useState<'Seller' | 'Admin'>('Seller');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  function load() {
    getUsers()
      .then(setUsers)
      .finally(() => setLoading(false));
  }

  useEffect(load, []);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await createStaffUser(username, password, fullName, roleName);
      setUsername('');
      setPassword('');
      setFullName('');
      load();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div>
      <h1>Users</h1>
      <p className="product-stock" style={{ marginTop: -8, marginBottom: 20 }}>
        Buyer accounts sign up publicly. Seller/Admin accounts can only be created here.
      </p>

      <div className="form-card" style={{ margin: '0 0 28px', maxWidth: 460 }}>
        <h1>Create staff account</h1>
        {error && <div className="alert error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label htmlFor="fullName">Full name</label>
            <input id="fullName" value={fullName} onChange={(e) => setFullName(e.target.value)} required />
          </div>
          <div className="field">
            <label htmlFor="username">Username</label>
            <input id="username" value={username} onChange={(e) => setUsername(e.target.value)} required minLength={4} maxLength={32} />
          </div>
          <div className="field">
            <label htmlFor="password">Password</label>
            <input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={8} />
          </div>
          <div className="field">
            <label htmlFor="roleName">Role</label>
            <select id="roleName" value={roleName} onChange={(e) => setRoleName(e.target.value as 'Seller' | 'Admin')}>
              <option value="Seller">Seller</option>
              <option value="Admin">Admin</option>
            </select>
          </div>
          <button className="btn" type="submit" disabled={submitting} style={{ width: '100%' }}>
            {submitting ? 'Creating…' : 'Create account'}
          </button>
        </form>
      </div>

      {loading ? (
        <div className="loading-state">Loading…</div>
      ) : (
        <table className="admin-table">
          <thead>
            <tr>
              <th>Full name</th>
              <th>Username</th>
              <th>Role</th>
              <th>Telegram</th>
              <th>Created</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id}>
                <td>{u.fullName}</td>
                <td className="product-stock">{u.username}</td>
                <td>
                  <span className={`status-badge status-${u.role === 'Admin' ? 'Cancelled' : u.role === 'Seller' ? 'Approved' : 'Completed'}`}>
                    {u.role}
                  </span>
                </td>
                <td className="product-stock">{u.hasTelegramLinked ? 'Linked' : '—'}</td>
                <td className="product-stock">{formatDateTime(u.createdAtUtc)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
