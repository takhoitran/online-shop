import { useEffect, useRef, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function UserMenu() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    function onDocClick(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', onDocClick);
    return () => document.removeEventListener('mousedown', onDocClick);
  }, [open]);

  if (!user) return null;

  const initial = user.fullName.trim().charAt(0).toUpperCase() || '?';

  function handleLogout() {
    logout();
    setOpen(false);
    navigate('/');
  }

  return (
    <div ref={rootRef} className="user-menu">
      <button
        type="button"
        className="user-avatar user-avatar-btn"
        aria-label="Account menu"
        aria-expanded={open}
        onClick={() => setOpen((v) => !v)}
      >
        {initial}
      </button>

      {open && (
        <div className="user-menu-panel">
          <div className="user-menu-header">
            <div className="user-avatar user-avatar-lg">{initial}</div>
            <div>
              <div className="user-menu-name">{user.fullName}</div>
              <div className="user-menu-meta">@{user.username}</div>
              <span className="status-badge status-Approved">{user.role}</span>
            </div>
          </div>
          <Link to="/profile" className="user-menu-item" onClick={() => setOpen(false)}>
            👤 View profile
          </Link>
          <button className="user-menu-item danger" onClick={handleLogout}>
            🚪 Log out
          </button>
        </div>
      )}
    </div>
  );
}
