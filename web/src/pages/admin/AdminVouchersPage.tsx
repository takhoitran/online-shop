import { useEffect, useState, type FormEvent } from 'react';
import { createVoucher, deactivateVoucher, getVouchers, updateVoucher } from '../../api/vouchers';
import { extractErrorMessage } from '../../api/client';
import type { DiscountType, Voucher } from '../../types';
import { formatCurrency, formatDateTime } from '../../utils/format';

function toDateInputValue(iso: string | null) {
  if (!iso) return '';
  // Format using the browser's local calendar date, not the UTC one a naive string-slice would
  // read off the ISO string — the two can disagree once the stored end-of-day-local timestamp
  // has been converted to UTC (see dateInputToEndOfDayIso below).
  const date = new Date(iso);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/** A plain "YYYY-MM-DD" from a <input type="date"> is midnight *UTC* if passed straight to
 * `new Date(...)` — so "expires on Sept 15" would make the voucher expire right as Sept 15
 * begins, not once it ends. Anchoring to 23:59:59.999 *local* time instead means the voucher
 * stays valid through the whole calendar day the admin actually picked. */
function dateInputToEndOfDayIso(dateInputValue: string) {
  return new Date(`${dateInputValue}T23:59:59.999`).toISOString();
}

export function AdminVouchersPage() {
  const [vouchers, setVouchers] = useState<Voucher[]>([]);
  const [loading, setLoading] = useState(true);

  const [code, setCode] = useState('');
  const [discountType, setDiscountType] = useState<DiscountType>('Percentage');
  const [discountValue, setDiscountValue] = useState('');
  const [maxUses, setMaxUses] = useState('');
  const [expiresAt, setExpiresAt] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const [editingId, setEditingId] = useState<string | null>(null);
  const [editDiscountType, setEditDiscountType] = useState<DiscountType>('Percentage');
  const [editDiscountValue, setEditDiscountValue] = useState('');
  const [editMaxUses, setEditMaxUses] = useState('');
  const [editExpiresAt, setEditExpiresAt] = useState('');

  function load() {
    getVouchers()
      .then(setVouchers)
      .finally(() => setLoading(false));
  }

  useEffect(load, []);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      await createVoucher({
        code,
        discountType,
        discountValue: Number(discountValue),
        maxUses: maxUses ? Number(maxUses) : null,
        expiresAtUtc: expiresAt ? dateInputToEndOfDayIso(expiresAt) : null,
      });
      setCode('');
      setDiscountValue('');
      setMaxUses('');
      setExpiresAt('');
      load();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  function startEdit(v: Voucher) {
    setError(null);
    setEditingId(v.id);
    setEditDiscountType(v.discountType);
    setEditDiscountValue(String(v.discountValue));
    setEditMaxUses(v.maxUses ? String(v.maxUses) : '');
    setEditExpiresAt(toDateInputValue(v.expiresAtUtc));
  }

  function cancelEdit() {
    setEditingId(null);
  }

  async function saveEdit(id: string) {
    setError(null);
    setBusyId(id);
    try {
      await updateVoucher(id, {
        discountType: editDiscountType,
        discountValue: Number(editDiscountValue),
        maxUses: editMaxUses ? Number(editMaxUses) : null,
        expiresAtUtc: editExpiresAt ? dateInputToEndOfDayIso(editExpiresAt) : null,
      });
      setEditingId(null);
      load();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  async function handleDeactivate(id: string) {
    if (!window.confirm('Deactivate this voucher? It can no longer be used at checkout.')) return;
    setBusyId(id);
    try {
      await deactivateVoucher(id);
      load();
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setBusyId(null);
    }
  }

  return (
    <div>
      <h1>Vouchers</h1>
      <p className="product-stock" style={{ marginTop: -8, marginBottom: 20 }}>
        Discount codes buyers can apply at checkout.
      </p>

      <div className="form-card" style={{ margin: '0 0 28px', maxWidth: 460 }}>
        <h1>Create voucher</h1>
        {error && <div className="alert error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="field">
            <label htmlFor="code">Code</label>
            <input id="code" value={code} onChange={(e) => setCode(e.target.value.toUpperCase())} required maxLength={50} placeholder="e.g. WELCOME10" />
          </div>
          <div className="field">
            <label htmlFor="discountType">Discount type</label>
            <select id="discountType" value={discountType} onChange={(e) => setDiscountType(e.target.value as DiscountType)}>
              <option value="Percentage">Percentage off</option>
              <option value="FixedAmount">Fixed amount off</option>
            </select>
          </div>
          <div className="field">
            <label htmlFor="discountValue">{discountType === 'Percentage' ? 'Percent off (1-100)' : 'Amount off (VND)'}</label>
            <input
              id="discountValue"
              type="number"
              min="1"
              max={discountType === 'Percentage' ? '100' : undefined}
              value={discountValue}
              onChange={(e) => setDiscountValue(e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="maxUses">Max uses (optional)</label>
            <input id="maxUses" type="number" min="1" value={maxUses} onChange={(e) => setMaxUses(e.target.value)} placeholder="Unlimited" />
          </div>
          <div className="field">
            <label htmlFor="expiresAt">Expires on (optional)</label>
            <input id="expiresAt" type="date" value={expiresAt} onChange={(e) => setExpiresAt(e.target.value)} />
          </div>
          <button className="btn" type="submit" disabled={submitting} style={{ width: '100%' }}>
            {submitting ? 'Creating…' : 'Create voucher'}
          </button>
        </form>
      </div>

      {loading ? (
        <div className="loading-state">Loading…</div>
      ) : (
        <table className="admin-table">
          <thead>
            <tr>
              <th>Code</th>
              <th>Discount</th>
              <th>Uses</th>
              <th>Expires</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {vouchers.map((v) => {
              const isEditing = editingId === v.id;
              return (
                <tr key={v.id}>
                  <td>
                    <strong>{v.code}</strong>
                  </td>
                  {isEditing ? (
                    <>
                      <td>
                        <div style={{ display: 'flex', gap: 6 }}>
                          <select
                            value={editDiscountType}
                            onChange={(e) => setEditDiscountType(e.target.value as DiscountType)}
                            style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6 }}
                          >
                            <option value="Percentage">%</option>
                            <option value="FixedAmount">VND</option>
                          </select>
                          <input
                            type="number"
                            min="1"
                            max={editDiscountType === 'Percentage' ? '100' : undefined}
                            value={editDiscountValue}
                            onChange={(e) => setEditDiscountValue(e.target.value)}
                            style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6, width: 90 }}
                          />
                        </div>
                      </td>
                      <td>
                        <input
                          type="number"
                          min="1"
                          value={editMaxUses}
                          onChange={(e) => setEditMaxUses(e.target.value)}
                          placeholder="Unlimited"
                          style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6, width: 90 }}
                        />
                      </td>
                      <td>
                        <input
                          type="date"
                          value={editExpiresAt}
                          onChange={(e) => setEditExpiresAt(e.target.value)}
                          style={{ padding: '6px 8px', border: '1px solid var(--color-border)', borderRadius: 6 }}
                        />
                      </td>
                      <td>
                        <span className={`status-badge status-${v.isActive ? 'Completed' : 'Cancelled'}`}>{v.isActive ? 'Active' : 'Inactive'}</span>
                      </td>
                      <td style={{ whiteSpace: 'nowrap' }}>
                        <button className="btn small" disabled={busyId === v.id} onClick={() => saveEdit(v.id)} style={{ marginRight: 6 }}>
                          Save
                        </button>
                        <button className="btn secondary small" onClick={cancelEdit}>
                          Cancel
                        </button>
                      </td>
                    </>
                  ) : (
                    <>
                      <td>{v.discountType === 'Percentage' ? `${v.discountValue}%` : formatCurrency(v.discountValue, 'VND')}</td>
                      <td className="product-stock">
                        {v.usedCount}
                        {v.maxUses ? ` / ${v.maxUses}` : ''}
                      </td>
                      <td className="product-stock">{v.expiresAtUtc ? formatDateTime(v.expiresAtUtc) : 'Never'}</td>
                      <td>
                        <span className={`status-badge status-${v.isActive ? 'Completed' : 'Cancelled'}`}>{v.isActive ? 'Active' : 'Inactive'}</span>
                      </td>
                      <td style={{ whiteSpace: 'nowrap' }}>
                        <button className="btn secondary small" onClick={() => startEdit(v)} style={{ marginRight: 6 }}>
                          Edit
                        </button>
                        {v.isActive && (
                          <button className="btn danger small" disabled={busyId === v.id} onClick={() => handleDeactivate(v.id)}>
                            Deactivate
                          </button>
                        )}
                      </td>
                    </>
                  )}
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
    </div>
  );
}
