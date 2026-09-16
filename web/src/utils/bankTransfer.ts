/** Demo bank details shown to buyers after choosing bank transfer (dev/demo only). */
export const DEMO_BANK_TRANSFER = {
  bankName: 'Vietcombank',
  accountName: 'CONG TY TNHH ONLINESHOP DEMO',
  accountNumber: '0123456789',
  branch: 'Hanoi branch',
} as const;

export function buildTransferNote(orderRef: string): string {
  return `ONLINESHOP ${orderRef}`;
}
