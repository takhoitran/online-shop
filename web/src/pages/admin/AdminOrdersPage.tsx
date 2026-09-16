import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { getOrders } from '../../api/orders';
import type { OrderStatus, OrderSummary, PagedResult } from '../../types';
import { formatCurrency, formatDateTime, ORDER_STATUS_LABELS } from '../../utils/format';

const STATUS_TABS: { value: OrderStatus | ''; label: string }[] = [
  { value: 'Pending', label: 'Pending' },
  { value: 'Approved', label: 'Approved' },
  { value: 'Shipping', label: 'Shipping' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Cancelled', label: 'Cancelled' },
  { value: 'Returned', label: 'Returned' },
  { value: '', label: 'All' },
];

export function AdminOrdersPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const status = (searchParams.get('status') ?? 'Pending') as OrderStatus | '';
  const [result, setResult] = useState<PagedResult<OrderSummary> | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    getOrders({ status: status || undefined, pageSize: 50 })
      .then(setResult)
      .finally(() => setLoading(false));
  }, [status]);

  return (
    <div>
      <h1>Orders</h1>
      <div className="admin-tabs">
        {STATUS_TABS.map((tab) => (
          <button
            key={tab.value || 'all'}
            className={`admin-tab ${status === tab.value ? 'active' : ''}`}
            onClick={() => setSearchParams(tab.value ? { status: tab.value } : {})}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {loading && <div className="loading-state">Loading…</div>}

      {!loading && (!result || result.items.length === 0) && (
        <div className="empty-state">No orders in this status.</div>
      )}

      {!loading && result && result.items.length > 0 && (
        <div className="order-list">
          {result.items.map((order) => (
            <Link to={`/admin/orders/${order.id}`} className="order-row" key={order.id}>
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
      )}
    </div>
  );
}
