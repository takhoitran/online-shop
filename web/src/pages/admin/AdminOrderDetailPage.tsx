import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import {
  approveOrder,
  cancelOrder,
  completeOrder,
  getOrderById,
  markOrderPaid,
  returnOrder,
  shipOrder,
} from '../../api/orders';
import { extractErrorMessage } from '../../api/client';
import { OrderStatusTimeline } from '../../components/OrderStatusTimeline';
import type { Order } from '../../types';
import {
  formatCurrency,
  formatDateTime,
  ORDER_STATUS_LABELS,
  PAYMENT_METHOD_LABELS,
  PAYMENT_STATUS_LABELS,
} from '../../utils/format';

export function AdminOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [order, setOrder] = useState<Order | null>(null);
  const [notFound, setNotFound] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  function load() {
    if (!id) return;
    getOrderById(id)
      .then(setOrder)
      .catch(() => setNotFound(true));
  }

  useEffect(load, [id]);

  if (notFound) return <div className="empty-state">Order not found.</div>;
  if (!order) return <div className="loading-state">Loading…</div>;

  async function runAction(action: () => Promise<unknown>, confirmMessage?: string) {
    if (confirmMessage && !window.confirm(confirmMessage)) return;
    setBusy(true);
    setError(null);
    try {
      await action();
      load();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div>
      <div className="order-detail-header">
        <div>
          <h1 style={{ marginBottom: 4 }}>Order #{order.id.slice(0, 8).toUpperCase()}</h1>
          <span className="product-stock">
            Buyer: {order.userId.slice(0, 8)}… · Placed on {formatDateTime(order.orderDateUtc)}
          </span>
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
          <h3 style={{ marginTop: 0 }}>Ship to</h3>
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

          <div style={{ display: 'flex', flexDirection: 'column', gap: 8, marginTop: 16 }}>
            {order.status === 'Pending' && (
              <button className="btn" disabled={busy} onClick={() => runAction(() => approveOrder(order.id))}>
                Approve order
              </button>
            )}
            {order.paymentStatus === 'Unpaid' && (order.status === 'Pending' || order.status === 'Approved' || order.status === 'Shipping' || order.status === 'Completed') && (
              <button className="btn secondary" disabled={busy} onClick={() => runAction(() => markOrderPaid(order.id))}>
                Confirm payment received
              </button>
            )}
            {order.status === 'Approved' && (
              <button className="btn secondary" disabled={busy} onClick={() => runAction(() => shipOrder(order.id))}>
                Mark as Shipping
              </button>
            )}
            {order.status === 'Shipping' && (
              <button className="btn secondary" disabled={busy} onClick={() => runAction(() => completeOrder(order.id))}>
                Confirm delivered (Complete)
              </button>
            )}
            {order.status === 'Completed' && (
              <button
                className="btn secondary"
                disabled={busy}
                onClick={() => runAction(() => returnOrder(order.id, 'Processed as a return'), 'Confirm this order as returned?')}
              >
                Process return
              </button>
            )}
            {(order.status === 'Pending' || order.status === 'Approved') && (
              <button
                className="btn danger"
                disabled={busy}
                onClick={() => runAction(() => cancelOrder(order.id, 'Cancelled by seller'), 'Confirm cancelling this order?')}
              >
                Cancel order
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
