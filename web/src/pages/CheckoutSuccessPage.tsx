import { Link, Navigate, useLocation } from 'react-router-dom';
import { Breadcrumbs } from '../components/Breadcrumbs';
import { useLanguage } from '../i18n/LanguageContext';
import { formatCurrency } from '../utils/format';
import { DEMO_BANK_TRANSFER, buildTransferNote } from '../utils/bankTransfer';
import type { PaymentMethod } from '../types';

export function CheckoutSuccessPage() {
  const location = useLocation();
  const { t } = useLanguage();
  const state = (location.state as { orderId?: string; paymentMethod?: PaymentMethod; shippingFee?: number } | null) ?? null;
  const orderId = state?.orderId;

  if (!orderId) {
    return <Navigate to="/" replace />;
  }

  const orderRef = orderId.slice(0, 8).toUpperCase();
  const isBank = state?.paymentMethod === 'BankTransfer';

  return (
    <div className="checkout-success">
      <Breadcrumbs items={[{ label: t.home, to: '/' }, { label: 'Order placed' }]} />
      <div className="checkout-success-card">
        <div className="checkout-success-icon" aria-hidden>
          ✓
        </div>
        <h1>{t.orderSuccessTitle}</h1>
        <p className="checkout-success-lead">{t.orderSuccessLead}</p>
        <p className="product-stock">
          Order reference: <strong>#{orderRef}</strong>
        </p>
        {typeof state?.shippingFee === 'number' && state.shippingFee > 0 && (
          <p className="product-stock">
            Estimated shipping: <strong>{formatCurrency(state.shippingFee, 'VND')}</strong> (pay with your order or COD, depending
            on payment method).
          </p>
        )}

        {isBank && (
          <div className="bank-transfer-panel alert info" style={{ marginTop: 16, textAlign: 'left' }}>
            <strong>{t.bankTransferTitle}</strong>
            <ul style={{ margin: '8px 0 0', paddingLeft: 18, lineHeight: 1.6 }}>
              <li>
                Bank: <strong>{DEMO_BANK_TRANSFER.bankName}</strong>
              </li>
              <li>
                Account name: <strong>{DEMO_BANK_TRANSFER.accountName}</strong>
              </li>
              <li>
                Account number: <strong>{DEMO_BANK_TRANSFER.accountNumber}</strong>
              </li>
              <li>
                Transfer note: <strong>{buildTransferNote(orderRef)}</strong>
              </li>
            </ul>
          </div>
        )}

        <div className="checkout-success-actions">
          <Link to={`/orders/${orderId}`} className="btn">
            {t.viewOrder}
          </Link>
          <Link to="/products" className="btn secondary">
            {t.continueShopping}
          </Link>
        </div>
      </div>
    </div>
  );
}
