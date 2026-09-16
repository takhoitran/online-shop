export function formatCurrency(amount: number, currency: string): string {
  if (currency === 'VND') {
    return `${amount.toLocaleString('en-US')} ₫`;
  }
  return `${amount.toLocaleString('en-US')} ${currency}`;
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString('en-US');
}

export const ORDER_STATUS_LABELS: Record<string, string> = {
  Pending: 'Pending',
  Approved: 'Approved',
  Shipping: 'Shipping',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  Returned: 'Returned',
};

export const PAYMENT_METHOD_LABELS: Record<string, string> = {
  Cod: 'Cash on delivery (COD)',
  BankTransfer: 'Bank transfer',
};

export const PAYMENT_STATUS_LABELS: Record<string, string> = {
  Unpaid: 'Unpaid',
  Paid: 'Paid',
  Refunded: 'Refunded',
};
