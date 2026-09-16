import { useEffect, useMemo, useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { useCart } from '../auth/CartContext';
import { checkout } from '../api/orders';
import { getPublicVouchers, previewVoucher } from '../api/vouchers';
import { extractErrorMessage } from '../api/client';
import { formatCurrency, formatDateTime } from '../utils/format';
import { Breadcrumbs } from '../components/Breadcrumbs';
import { useLanguage } from '../i18n/LanguageContext';
import { estimateDeliveryDays, estimateShippingFee } from '../utils/shipping';
import { DEMO_BANK_TRANSFER } from '../utils/bankTransfer';
import { loadSavedAddresses, saveAddressFromCheckout, type SavedAddress } from '../utils/savedAddresses';
import { useToast } from '../components/ToastProvider';
import type { PaymentMethod, PublicVoucher, VoucherPreview } from '../types';

const PAYMENT_OPTIONS: { value: PaymentMethod; title: string; description: string }[] = [
  { value: 'Cod', title: 'Cash on delivery (COD)', description: 'Pay cash to the delivery person.' },
  {
    value: 'BankTransfer',
    title: 'Bank transfer',
    description: 'The seller confirms manually once payment is received.',
  },
];

function describeVoucher(voucher: PublicVoucher): string {
  if (voucher.discountType === 'Percentage') return `${voucher.discountValue}% off`;
  return `${voucher.discountValue.toLocaleString('en-US')} off`;
}

export function CheckoutPage() {
  const { user } = useAuth();
  const { cart, loading: cartLoading, refresh } = useCart();
  const navigate = useNavigate();
  const location = useLocation();
  const { showToast } = useToast();
  const { t } = useLanguage();
  const selectedProductIds = (location.state as { selectedProductIds?: string[] } | null)?.selectedProductIds ?? null;
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('Cod');
  const [savedAddresses, setSavedAddresses] = useState<SavedAddress[]>(() => loadSavedAddresses());
  const [selectedAddressId, setSelectedAddressId] = useState<string | 'new'>(() => {
    const list = loadSavedAddresses();
    const def = list.find((a) => a.isDefault) ?? list[0];
    return def?.id ?? 'new';
  });
  const [saveForLater, setSaveForLater] = useState(true);
  const [recipientName, setRecipientName] = useState(user?.fullName ?? '');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [addressLine, setAddressLine] = useState('');
  const [city, setCity] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const prefilledVoucher = (location.state as { voucherCode?: string } | null)?.voucherCode ?? '';
  const [voucherMode, setVoucherMode] = useState<'none' | 'apply'>(prefilledVoucher ? 'apply' : 'none');
  const [availableVouchers, setAvailableVouchers] = useState<PublicVoucher[]>([]);
  const [loadingVouchers, setLoadingVouchers] = useState(false);
  const [voucherCode, setVoucherCode] = useState(prefilledVoucher);
  const [voucherPreview, setVoucherPreview] = useState<VoucherPreview | null>(null);
  const [voucherError, setVoucherError] = useState<string | null>(null);
  const [applyingVoucher, setApplyingVoucher] = useState(false);

  useEffect(() => {
    if (!prefilledVoucher) return;
    setVoucherMode('apply');
    setVoucherCode(prefilledVoucher);
  }, [prefilledVoucher]);

  useEffect(() => {
    if (voucherMode !== 'apply') return;
    setLoadingVouchers(true);
    setVoucherError(null);
    getPublicVouchers()
      .then(setAvailableVouchers)
      .catch((err) => setVoucherError(extractErrorMessage(err, 'Could not load available vouchers.')))
      .finally(() => setLoadingVouchers(false));
  }, [voucherMode]);

  useEffect(() => {
    if (selectedAddressId === 'new') return;
    const picked = savedAddresses.find((a) => a.id === selectedAddressId);
    if (!picked) return;
    setRecipientName(picked.recipientName);
    setPhoneNumber(picked.phoneNumber);
    setAddressLine(picked.addressLine);
    setCity(picked.city);
  }, [selectedAddressId, savedAddresses]);

  const checkoutItems = useMemo(() => {
    if (!cart) return [];
    if (!selectedProductIds || selectedProductIds.length === 0) return cart.items;
    const idSet = new Set(selectedProductIds);
    return cart.items.filter((i) => idSet.has(i.productId));
  }, [cart, selectedProductIds]);

  if (cartLoading && !cart) {
    return <div className="loading-state">Loading checkout…</div>;
  }

  if (!cart || cart.items.length === 0 || checkoutItems.length === 0) {
    return <Navigate to="/cart" replace />;
  }

  const checkoutSubtotal = checkoutItems.reduce((sum, i) => sum + i.lineTotal, 0);
  const shippingFee = estimateShippingFee(city, checkoutSubtotal);
  const goodsTotal = voucherPreview ? voucherPreview.newTotal : checkoutSubtotal;
  const orderTotal = goodsTotal + shippingFee;
  const addressValid = recipientName.trim() && phoneNumber.trim() && addressLine.trim() && city.trim();

  function handleVoucherModeChange(mode: 'none' | 'apply') {
    setVoucherMode(mode);
    setVoucherError(null);
    if (mode === 'none') {
      setVoucherCode('');
      setVoucherPreview(null);
    }
  }

  async function handleApplyVoucher(code = voucherCode) {
    const trimmedCode = code.trim();
    if (!trimmedCode) return;
    setApplyingVoucher(true);
    setVoucherError(null);
    setVoucherPreview(null);
    setVoucherCode(trimmedCode);
    try {
      const preview = await previewVoucher(trimmedCode, checkoutItems.map((i) => i.productId));
      if (preview.isValid) {
        setVoucherPreview(preview);
        showToast('Voucher applied', 'success');
      } else {
        setVoucherError(preview.errorMessage ?? 'This voucher code is invalid.');
      }
    } catch (err) {
      setVoucherError(extractErrorMessage(err, 'Could not check this voucher code.'));
    } finally {
      setApplyingVoucher(false);
    }
  }

  function handleRemoveVoucher() {
    setVoucherPreview(null);
    setVoucherCode('');
    setVoucherError(null);
  }

  async function handleConfirm() {
    if (!addressValid) {
      setError('Please fill in all shipping address fields.');
      return;
    }
    setSubmitting(true);
    setError(null);
    try {
      if (saveForLater) {
        saveAddressFromCheckout({ recipientName, phoneNumber, addressLine, city });
        setSavedAddresses(loadSavedAddresses());
      }

      const orderId = await checkout({
        paymentMethod,
        recipientName,
        phoneNumber,
        addressLine,
        city,
        voucherCode: voucherMode === 'apply' && voucherPreview ? voucherCode.trim() : null,
        productIds: checkoutItems.map((i) => i.productId),
      });
      navigate('/checkout/success', { replace: true, state: { orderId, paymentMethod, shippingFee } });
      void refresh({ silent: true });
    } catch (err) {
      setError(extractErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div>
      <Breadcrumbs items={[{ label: t.home, to: '/' }, { label: t.cart, to: '/cart' }, { label: t.checkout }]} />
      <div className="cart-layout">
        <div>
          <h1>Confirm order</h1>
          {error && <div className="alert error">{error}</div>}

          <h3>Items ({checkoutItems.length})</h3>
          {checkoutItems.map((item) => (
            <div className="summary-row" key={item.productId}>
              <span>
                {item.productName} × {item.quantity}
              </span>
              <span>{formatCurrency(item.lineTotal, item.currency)}</span>
            </div>
          ))}

          <h3 style={{ marginTop: 24 }}>{t.shippingAddress}</h3>

          {savedAddresses.length > 0 && (
            <div className="saved-address-list" style={{ marginBottom: 16 }}>
              <div className="header-search-label">{t.savedAddresses}</div>
              {savedAddresses.map((addr) => (
                <label key={addr.id} className={`payment-option ${selectedAddressId === addr.id ? 'selected' : ''}`}>
                  <input
                    type="radio"
                    name="savedAddress"
                    checked={selectedAddressId === addr.id}
                    onChange={() => setSelectedAddressId(addr.id)}
                  />
                  <span>
                    <strong>{addr.recipientName}</strong>
                    <span>
                      {addr.phoneNumber} · {addr.addressLine}, {addr.city}
                    </span>
                  </span>
                </label>
              ))}
              <label className={`payment-option ${selectedAddressId === 'new' ? 'selected' : ''}`}>
                <input
                  type="radio"
                  name="savedAddress"
                  checked={selectedAddressId === 'new'}
                  onChange={() => setSelectedAddressId('new')}
                />
                <span>
                  <strong>{t.newAddress}</strong>
                </span>
              </label>
            </div>
          )}

          {(selectedAddressId === 'new' || savedAddresses.length === 0) && (
            <>
              <div className="field">
                <label htmlFor="recipientName">Recipient name</label>
                <input
                  id="recipientName"
                  value={recipientName}
                  onChange={(e) => setRecipientName(e.target.value)}
                  placeholder="Full name"
                />
              </div>
              <div className="field">
                <label htmlFor="phoneNumber">Phone number</label>
                <input
                  id="phoneNumber"
                  value={phoneNumber}
                  onChange={(e) => setPhoneNumber(e.target.value)}
                  placeholder="e.g. 0912345678"
                />
              </div>
              <div className="field">
                <label htmlFor="addressLine">Address</label>
                <input
                  id="addressLine"
                  value={addressLine}
                  onChange={(e) => setAddressLine(e.target.value)}
                  placeholder="Street address"
                />
              </div>
              <div className="field">
                <label htmlFor="city">City</label>
                <input id="city" value={city} onChange={(e) => setCity(e.target.value)} placeholder="City / Province" />
              </div>
              <label className="checkbox-row" style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 8 }}>
                <input type="checkbox" checked={saveForLater} onChange={(e) => setSaveForLater(e.target.checked)} />
                <span>{t.saveAddress}</span>
              </label>
            </>
          )}

          {addressValid && city.trim() && (
            <div className="alert info" style={{ marginTop: 8 }}>
              {t.deliveryEta}: <strong>{estimateDeliveryDays(city)}</strong>
            </div>
          )}

          <h3 style={{ marginTop: 24 }}>Voucher</h3>
          <div className="payment-options voucher-mode-options">
            <label className={`payment-option ${voucherMode === 'none' ? 'selected' : ''}`}>
              <input
                type="radio"
                name="voucherMode"
                checked={voucherMode === 'none'}
                onChange={() => handleVoucherModeChange('none')}
              />
              <span>
                <strong>Do not apply voucher</strong>
                <span>Continue checkout without a discount code.</span>
              </span>
            </label>
            <label className={`payment-option ${voucherMode === 'apply' ? 'selected' : ''}`}>
              <input
                type="radio"
                name="voucherMode"
                checked={voucherMode === 'apply'}
                onChange={() => handleVoucherModeChange('apply')}
              />
              <span>
                <strong>Apply voucher</strong>
                <span>Choose one of the currently available voucher codes.</span>
              </span>
            </label>
          </div>

          {voucherMode === 'apply' && (
            <div className="voucher-picker">
              {voucherError && <div className="alert error">{voucherError}</div>}
              {loadingVouchers && <div className="loading-state compact">Loading vouchers...</div>}
              {!loadingVouchers && availableVouchers.length === 0 && !voucherError && (
                <div className="empty-state compact">No vouchers are available right now.</div>
              )}
              {!loadingVouchers && availableVouchers.length > 0 && (
                <div className="checkout-voucher-list">
                  {availableVouchers.map((voucher) => {
                    const selected = voucherCode.trim().toUpperCase() === voucher.code.toUpperCase();
                    return (
                      <button
                        key={voucher.code}
                        type="button"
                        className={`checkout-voucher-card ${selected ? 'selected' : ''}`}
                        disabled={applyingVoucher}
                        onClick={() => handleApplyVoucher(voucher.code)}
                      >
                        <span className="checkout-voucher-code">{voucher.code}</span>
                        <span>
                          <strong>{describeVoucher(voucher)}</strong>
                          {voucher.expiresAtUtc && <small>Expires {formatDateTime(voucher.expiresAtUtc)}</small>}
                        </span>
                        <span className="checkout-voucher-action">
                          {selected && voucherPreview ? 'Applied' : applyingVoucher && selected ? 'Checking...' : 'Use'}
                        </span>
                      </button>
                    );
                  })}
                </div>
              )}

              <div className="voucher-manual-entry">
                <input
                  value={voucherCode}
                  onChange={(e) => {
                    setVoucherCode(e.target.value);
                    setVoucherPreview(null);
                  }}
                  placeholder="Or enter a voucher code"
                />
                <button className="btn secondary" disabled={applyingVoucher || !voucherCode.trim()} onClick={() => handleApplyVoucher()}>
                  {applyingVoucher ? 'Checking...' : 'Apply'}
                </button>
              </div>

              {voucherPreview && (
                <div className="alert info" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span>
                    Voucher <strong>{voucherCode.trim().toUpperCase()}</strong> applied: -
                    {formatCurrency(voucherPreview.discountAmount, voucherPreview.currency)}
                  </span>
                  <button className="btn secondary small" onClick={handleRemoveVoucher}>
                    Remove
                  </button>
                </div>
              )}
            </div>
          )}

          <h3 style={{ marginTop: 24 }}>Payment method</h3>
          <div className="payment-options">
            {PAYMENT_OPTIONS.map((opt) => (
              <label key={opt.value} className={`payment-option ${paymentMethod === opt.value ? 'selected' : ''}`}>
                <input
                  type="radio"
                  name="paymentMethod"
                  checked={paymentMethod === opt.value}
                  onChange={() => setPaymentMethod(opt.value)}
                />
                <span>
                  <strong>{opt.title}</strong>
                  <span>{opt.description}</span>
                </span>
              </label>
            ))}
          </div>

          {paymentMethod === 'BankTransfer' && (
            <div className="bank-transfer-panel alert info" style={{ marginTop: 12 }}>
              <strong>{t.bankTransferTitle}</strong>
              <p className="product-stock" style={{ marginTop: 8 }}>
                {t.bankTransferHint}
              </p>
              <ul style={{ margin: '8px 0 0', paddingLeft: 18, lineHeight: 1.6 }}>
                <li>
                  Bank: <strong>{DEMO_BANK_TRANSFER.bankName}</strong> — {DEMO_BANK_TRANSFER.branch}
                </li>
                <li>
                  Account name: <strong>{DEMO_BANK_TRANSFER.accountName}</strong>
                </li>
                <li>
                  Account number: <strong>{DEMO_BANK_TRANSFER.accountNumber}</strong>
                </li>
              </ul>
            </div>
          )}
        </div>

        <div className="summary-card">
          <div className="summary-row">
            <span>{t.subtotal}</span>
            <span>{formatCurrency(checkoutSubtotal, cart.currency)}</span>
          </div>
          {voucherPreview && (
            <div className="summary-row">
              <span>{t.discount}</span>
              <span>−{formatCurrency(voucherPreview.discountAmount, voucherPreview.currency)}</span>
            </div>
          )}
          <div className="summary-row">
            <span>{t.shippingFee}</span>
            <span>{city.trim() ? formatCurrency(shippingFee, cart.currency) : '—'}</span>
          </div>
          <div className="summary-row total">
            <span>{t.total}</span>
            <span>{formatCurrency(city.trim() ? orderTotal : goodsTotal, cart.currency)}</span>
          </div>
          <button
            className="btn"
            style={{ width: '100%', marginTop: 12 }}
            disabled={submitting || !addressValid}
            onClick={handleConfirm}
          >
            {submitting ? t.placingOrder : t.placeOrder}
          </button>
        </div>
      </div>
    </div>
  );
}
