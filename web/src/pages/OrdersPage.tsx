import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getOrders } from '../api/orders';
import type { OrderSummary, PagedResult } from '../types';
import { formatCurrency, formatDateTime, ORDER_STATUS_LABELS } from '../utils/format';
import { Breadcrumbs } from '../components/Breadcrumbs';

export function OrdersPage() {
  const [result, setResult] = useState<PagedResult<OrderSummary> | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getOrders()
      .then(setResult)
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="loading-state">Loading orders…</div>;

  if (!result || result.items.length === 0) {
    return (
      <div>
        <Breadcrumbs items={[{ label: 'Home', to: '/' }, { label: 'My orders' }]} />
        <div className="empty-state">
          <p>You don't have any orders yet.</p>
          <Link to="/" className="btn">
            Start shopping
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div>
      <Breadcrumbs items={[{ label: 'Home', to: '/' }, { label: 'My orders' }]} />
      <h1>My orders</h1>
      <div className="order-list">
        {result.items.map((order) => (
          <Link to={`/orders/${order.id}`} className="order-row" key={order.id}>
            <div className="meta">
              <div className="order-code">#{order.id.slice(0, 8).toUpperCase()}</div>
              <div>{formatDateTime(order.orderDateUtc)}</div>
              <div className="product-stock">{order.itemCount} item(s)</div>
            </div>
            <span className={`status-badge status-${order.status}`}>{ORDER_STATUS_LABELS[order.status]}</span>
            <strong>{formatCurrency(order.totalAmount, order.currency)}</strong>
          </Link>
        ))}
      </div>
    </div>
  );
}
