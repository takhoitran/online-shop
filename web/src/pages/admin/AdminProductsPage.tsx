import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { deleteProduct, searchProducts } from '../../api/catalog';
import { extractErrorMessage } from '../../api/client';
import type { PagedResult, ProductListItem } from '../../types';
import { formatCurrency } from '../../utils/format';

export function AdminProductsPage() {
  const [result, setResult] = useState<PagedResult<ProductListItem> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  function load() {
    setLoading(true);
    searchProducts({ pageSize: 100 })
      .then(setResult)
      .finally(() => setLoading(false));
  }

  useEffect(load, []);

  async function handleDelete(p: ProductListItem) {
    if (!window.confirm(`Delete "${p.name}"? This cannot be undone.`)) return;
    setError(null);
    setBusyId(p.id);
    try {
      await deleteProduct(p.id);
      load();
    } catch (err) {
      setError(extractErrorMessage(err, 'Could not delete this product.'));
    } finally {
      setBusyId(null);
    }
  }

  return (
    <div>
      <div className="admin-page-header">
        <h1>Products</h1>
        <Link to="/admin/products/new" className="btn">
          + Add product
        </Link>
      </div>

      {error && <div className="alert error">{error}</div>}

      {loading && <div className="loading-state">Loading…</div>}

      {!loading && result && (
        <table className="admin-table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Category</th>
              <th>Price</th>
              <th>Stock</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {result.items.map((p) => (
              <tr key={p.id}>
                <td>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    <div className="admin-table-thumb">{p.imageUrl ? <img src={p.imageUrl} alt="" /> : '—'}</div>
                    {p.name}
                  </div>
                </td>
                <td>{p.categoryName}</td>
                <td>{formatCurrency(p.price, p.currency)}</td>
                <td className={p.stockQuantity === 0 ? 'product-stock out' : ''}>{p.stockQuantity}</td>
                <td style={{ whiteSpace: 'nowrap' }}>
                  <Link to={`/admin/products/${p.id}`} className="btn secondary small" style={{ marginRight: 6 }}>
                    Edit
                  </Link>
                  <button className="btn danger small" disabled={busyId === p.id} onClick={() => handleDelete(p)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
