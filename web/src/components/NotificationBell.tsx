import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getNotifications, getUnreadNotificationCount, markNotificationAsRead } from '../api/notifications';
import { useAuth } from '../auth/AuthContext';
import type { AppNotification } from '../types';
import { formatDateTime } from '../utils/format';

export function NotificationBell() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState<AppNotification[]>([]);
  const [loading, setLoading] = useState(false);
  const panelRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!user) return;
    function refreshCount() {
      getUnreadNotificationCount().then(setUnreadCount).catch(() => {});
    }
    refreshCount();
    const timer = setInterval(refreshCount, 30000);
    return () => clearInterval(timer);
  }, [user]);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (panelRef.current && !panelRef.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  function toggleOpen() {
    const next = !open;
    setOpen(next);
    if (next) {
      setLoading(true);
      getNotifications()
        .then((r) => setNotifications(r.items))
        .finally(() => setLoading(false));
    }
  }

  async function handleClick(n: AppNotification) {
    if (!n.isRead) {
      await markNotificationAsRead(n.id);
      setNotifications((prev) => prev.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)));
      setUnreadCount((c) => Math.max(0, c - 1));
    }
    if (n.relatedOrderId) {
      const isStaff = user?.role === 'Seller' || user?.role === 'Admin';
      navigate(isStaff ? `/admin/orders/${n.relatedOrderId}` : `/orders/${n.relatedOrderId}`);
      setOpen(false);
    }
  }

  if (!user) return null;

  return (
    <div className="notification-bell-wrap" ref={panelRef}>
      <button className="notification-bell" onClick={toggleOpen} aria-label="Notifications">
        🔔
        {unreadCount > 0 && <span className="notification-badge">{unreadCount > 9 ? '9+' : unreadCount}</span>}
      </button>

      {open && (
        <div className="notification-panel">
          <div className="notification-panel-header">Notifications</div>
          {loading && <div className="loading-state">Loading…</div>}
          {!loading && notifications.length === 0 && <div className="empty-state">No notifications yet.</div>}
          {!loading &&
            notifications.map((n) => (
              <button
                key={n.id}
                className={`notification-item ${n.isRead ? '' : 'unread'}`}
                onClick={() => handleClick(n)}
              >
                <div className="notification-item-title">{n.title}</div>
                <div className="notification-item-message">{n.message}</div>
                <div className="notification-item-time">{formatDateTime(n.createdAtUtc)}</div>
              </button>
            ))}
        </div>
      )}
    </div>
  );
}
