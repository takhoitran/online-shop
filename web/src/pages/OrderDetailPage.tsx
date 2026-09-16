import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { cancelOrder, getOrderById } from '../api/orders';
import { extractErrorMessage } from '../api/client';
import { OrderStatusTimeline } from '../components/OrderStatusTimeline';
import { Breadcrumbs } from '../components/Breadcrumbs';
import type { Order } from '../types';
import {
  formatCurrency,
  formatDateTime,
  ORDER_STATUS_LABELS,
  PAYMENT_METHOD_LABELS,
  PAYMENT_STATUS_LABELS,
} from '../utils/format';

export function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [order, setOrder] = useState<Order | null>(null);
  const [notFound, setNotFound] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [cancelling, setCancelling] = useState(false);

  function load() {
    if (!id) return;
    getOrderById(id)
      .then(setOrder)
      .catch(() => setNotFound(true));
  }

  useEffect(load, [id]);

  if (notFound) return <div className="empty-state">Order not found, or you don't have permission to view it.</div>;
  if (!order) return <div className="loading-state">Loading…</div>;

  const canCancel = order.status === 'Pending' || order.status === 'Approved';

  async function handleCancel() {
    if (!id || !window.confirm('Are you sure you want to cancel this order?')) return;
    setCancelling(true);
    setError(null);
    try {
      await cancelOrder(id, 'Cancelled by customer request');
      load();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setCancelling(false);
    }
  }

  return (
    <div>
      <Breadcrumbs items={[{ label: 'Home', to: '/' }, { label: 'My orders', to: '/orders' }, { label: `#${order.id.slice(0, 8).toUpperCase()}` }]} />
      <div className="order-detail-header">
        <div>
          <h1 style={{ marginBottom: 4 }}>Order #{order.id.slice(0, 8).toUpperCase()}</h1>
          <span className="product-stock">Placed on {formatDateTime(order.orderDateUtc)}</span>
        </div>
        <span className={`status-badge status-${order.status}`}>{ORDER_STATUS_LABELS[order.status]}</span>
      </div>

      <OrderStatusTimeline status={order.status} />

      {error && <div className="alert error">{error}</div>}

      <div className="cart-layout">
        <div>
          <table className="order-items">
            <thead>
              <tr>
                <th>Product</th>
                <th>Qty</th>
                <th>Unit price</th>
                <th style={{ textAlign: 'right' }}>Line total</th>
              </tr>
            </thead>
            <tbody>
              {order.details.map((line) => (
                <tr key={line.productId}>
                  <td>{line.productName}</td>
                  <td>{line.quantity}</td>
                  <td>{formatCurrency(line.unitPrice, order.currency)}</td>
                  <td style={{ textAlign: 'right' }}>{formatCurrency(line.lineTotal, order.currency)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="summary-card">
          <h3 style={{ marginTop: 0 }}>Shipping to</h3>
          <p style={{ margin: '0 0 16px' }}>
            <strong>{order.recipientName}</strong>
            <br />
            {order.phoneNumber}
            <br />
            {order.addressLine}, {order.city}
          </p>
          <div className="summary-row">
            <span>Method</span>
            <span>{PAYMENT_METHOD_LABELS[order.paymentMethod]}</span>
          </div>
          <div className="summary-row">
            <span>Payment</span>
            <span>{PAYMENT_STATUS_LABELS[order.paymentStatus]}</span>
          </div>
          <div className="summary-row">
            <span>Subtotal</span>
            <span>{formatCurrency(order.subtotal, order.currency)}</span>
          </div>
          {order.discountAmount > 0 && (
            <div className="summary-row">
              <span>Discount {order.voucherCode && `(${order.voucherCode})`}</span>
              <span>−{formatCurrency(order.discountAmount, order.currency)}</span>
            </div>
          )}
          <div className="summary-row total">
            <span>Total</span>
            <span>{formatCurrency(order.totalAmount, order.currency)}</span>
          </div>
          {canCancel && (
            <button className="btn danger" style={{ width: '100%', marginTop: 12 }} disabled={cancelling} onClick={handleCancel}>
              {cancelling ? 'Cancelling…' : 'Cancel order'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
