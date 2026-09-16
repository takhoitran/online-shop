import { useEffect, useState, type FormEvent } from 'react';
import { useSearchParams } from 'react-router-dom';
import { searchProducts } from '../../api/catalog';
import { getProductStock, issueStock, receiveStock } from '../../api/inventory';
import { extractErrorMessage } from '../../api/client';
import type { ProductListItem, ProductStock } from '../../types';
import { formatDateTime } from '../../utils/format';

const TX_TYPE_LABELS: Record<string, string> = { In: 'In', Out: 'Out', Return: 'Return' };

export function AdminInventoryPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const productId = searchParams.get('productId') ?? '';

  const [products, setProducts] = useState<ProductListItem[]>([]);
  const [stock, setStock] = useState<ProductStock | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const [receiveQty, setReceiveQty] = useState('');
  const [receiveNote, setReceiveNote] = useState('');
  const [issueQty, setIssueQty] = useState('');
  const [issueNote, setIssueNote] = useState('');

  useEffect(() => {
    searchProducts({ pageSize: 100 }).then((r) => setProducts(r.items));
  }, []);

  function loadStock() {
    if (!productId) {
      setStock(null);
      return;
    }
    getProductStock(productId)
      .then(setStock)
      .catch(() => setStock(null));
  }

  useEffect(loadStock, [productId]);

  async function handleReceive(e: FormEvent) {
    e.preventDefault();
    if (!productId) return;
    setError(null);
    setSubmitting(true);
    try {
      await receiveStock(productId, Number(receiveQty), receiveNote || null);
      setReceiveQty('');
      setReceiveNote('');
      loadStock();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  async function handleIssue(e: FormEvent) {
    e.preventDefault();
    if (!productId) return;
    setError(null);
    setSubmitting(true);
    try {
      await issueStock(productId, Number(issueQty), issueNote);
      setIssueQty('');
      setIssueNote('');
      loadStock();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div>
      <h1>Inventory</h1>

      <div className="field" style={{ maxWidth: 380 }}>
        <label htmlFor="product">Select a product</label>
        <select id="product" value={productId} onChange={(e) => setSearchParams(e.target.value ? { productId: e.target.value } : {})}>
          <option value="">— Select a product —</option>
          {products.map((p) => (
            <option key={p.id} value={p.id}>
              {p.name}
            </option>
          ))}
        </select>
      </div>

      {error && <div className="alert error">{error}</div>}

      {productId && !stock && <div className="loading-state">This product has no stock record yet — receive stock to initialize it.</div>}

      {productId && (
        <div className="cart-layout" style={{ marginTop: 20 }}>
          <div>
            <h3>Transaction history</h3>
            {!stock || stock.recentTransactions.length === 0 ? (
              <div className="empty-state">No transactions yet.</div>
            ) : (
              <table className="order-items">
                <thead>
                  <tr>
                    <th>Type</th>
                    <th>Qty</th>
                    <th>Order</th>
                    <th>Note</th>
                    <th>Time</th>
                  </tr>
                </thead>
                <tbody>
                  {stock.recentTransactions.map((t) => (
                    <tr key={t.id}>
                      <td>{TX_TYPE_LABELS[t.type]}</td>
                      <td>{t.quantity}</td>
                      <td className="product-stock">{t.orderId ? `#${t.orderId.slice(0, 8).toUpperCase()}` : '—'}</td>
                      <td className="product-stock">{t.note || '—'}</td>
                      <td className="product-stock">{formatDateTime(t.occurredAtUtc)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>

          <div className="summary-card">
            <div className="summary-row total">
              <span>Current stock</span>
              <span>{stock?.quantityOnHand ?? 0}</span>
            </div>

            <form onSubmit={handleReceive} style={{ marginTop: 16 }}>
              <h4 style={{ margin: '0 0 8px' }}>Receive stock</h4>
              <div className="field">
                <input type="number" min="1" placeholder="Quantity" value={receiveQty} onChange={(e) => setReceiveQty(e.target.value)} required />
              </div>
              <div className="field">
                <input placeholder="Note (optional)" value={receiveNote} onChange={(e) => setReceiveNote(e.target.value)} />
              </div>
              <button className="btn" type="submit" disabled={submitting} style={{ width: '100%' }}>
                Confirm receive
              </button>
            </form>

            <form onSubmit={handleIssue} style={{ marginTop: 20 }}>
              <h4 style={{ margin: '0 0 8px' }}>Manual stock issue</h4>
              <p className="product-stock" style={{ marginTop: -4 }}>For write-offs, stocktaking — not tied to an order.</p>
              <div className="field">
                <input type="number" min="1" placeholder="Quantity" value={issueQty} onChange={(e) => setIssueQty(e.target.value)} required />
              </div>
              <div className="field">
                <input placeholder="Reason for issue (required)" value={issueNote} onChange={(e) => setIssueNote(e.target.value)} required />
              </div>
              <button className="btn secondary" type="submit" disabled={submitting} style={{ width: '100%' }}>
                Confirm issue
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
