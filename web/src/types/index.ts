// Mirrors the backend DTOs exactly (OnlineShop.Modules.*.Application.Dtos) — see the .NET
// project when a field looks unfamiliar, rather than guessing the shape here.

export interface AuthResult {
  userId: string;
  username: string;
  fullName: string;
  role: 'Admin' | 'Seller' | 'Buyer';
  token: string;
  expiresAtUtc: string;
}

export interface TelegramLinkCode {
  code: string;
  expiresAtUtc: string;
}

export interface RestockSuggestion {
  productName: string;
  action: string;
  reason: string;
}

export interface RestockReport {
  summary: string;
  suggestions: RestockSuggestion[];
}

export interface AppNotification {
  id: string;
  type: string;
  title: string;
  message: string;
  relatedOrderId: string | null;
  relatedProductId: string | null;
  isRead: boolean;
  createdAtUtc: string;
}

export interface Category {
  id: string;
  name: string;
  description: string | null;
  nameEn: string | null;
  nameVi: string | null;
  descriptionEn: string | null;
  descriptionVi: string | null;
}

export interface ProductListItem {
  id: string;
  name: string;
  nameEn: string | null;
  nameVi: string | null;
  price: number;
  currency: string;
  imageUrl: string | null;
  stockQuantity: number;
  categoryId: string;
  categoryName: string;
  categoryNameEn: string | null;
  categoryNameVi: string | null;
  averageRating: number | null;
  reviewCount: number;
  soldCount: number;
}

export interface ProductImage {
  id: string;
  url: string;
  sortOrder: number;
}

export interface ProductDetail extends ProductListItem {
  description: string | null;
  descriptionEn: string | null;
  descriptionVi: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  images: ProductImage[];
}

export interface ProductReview {
  id: string;
  productId: string;
  buyerId: string;
  buyerName: string;
  rating: number;
  comment: string | null;
  createdAtUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CartItemLine {
  productId: string;
  productName: string;
  imageUrl: string | null;
  unitPrice: number;
  currency: string;
  quantity: number;
  lineTotal: number;
}

export interface Cart {
  userId: string;
  items: CartItemLine[];
  total: number;
  currency: string;
}

export interface WishlistItemLine {
  productId: string;
  productName: string;
  imageUrl: string | null;
  price: number;
  currency: string;
  stockQuantity: number;
  addedAtUtc: string;
}

export interface Wishlist {
  userId: string;
  items: WishlistItemLine[];
}

export type OrderStatus = 'Pending' | 'Approved' | 'Shipping' | 'Completed' | 'Cancelled' | 'Returned';
export type PaymentMethod = 'Cod' | 'BankTransfer';
export type PaymentStatus = 'Unpaid' | 'Paid' | 'Refunded';

export interface OrderDetailLine {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: string;
  userId: string;
  status: OrderStatus;
  paymentMethod: PaymentMethod;
  paymentStatus: PaymentStatus;
  subtotal: number;
  discountAmount: number;
  voucherCode: string | null;
  totalAmount: number;
  currency: string;
  details: OrderDetailLine[];
  orderDateUtc: string;
  updatedAtUtc: string;
  recipientName: string;
  phoneNumber: string;
  addressLine: string;
  city: string;
}

export type DiscountType = 'Percentage' | 'FixedAmount';

export interface PublicVoucher {
  code: string;
  discountType: DiscountType;
  discountValue: number;
  expiresAtUtc: string | null;
}

export interface Voucher {
  id: string;
  code: string;
  discountType: DiscountType;
  discountValue: number;
  maxUses: number | null;
  usedCount: number;
  expiresAtUtc: string | null;
  isActive: boolean;
  createdAtUtc: string;
}

export interface VoucherPreview {
  isValid: boolean;
  errorMessage: string | null;
  discountAmount: number;
  newTotal: number;
  currency: string;
}

export interface OrderSummary {
  id: string;
  userId: string;
  status: OrderStatus;
  paymentMethod: PaymentMethod;
  paymentStatus: PaymentStatus;
  totalAmount: number;
  currency: string;
  itemCount: number;
  orderDateUtc: string;
}

export interface ProblemDetails {
  title: string;
  status: number;
  detail: string;
}

export interface UserSummary {
  id: string;
  username: string;
  fullName: string;
  role: 'Admin' | 'Seller' | 'Buyer';
  hasTelegramLinked: boolean;
  createdAtUtc: string;
}

export type InventoryTransactionType = 'In' | 'Out' | 'Return';

export interface InventoryTransaction {
  id: string;
  type: InventoryTransactionType;
  quantity: number;
  orderId: string | null;
  performedByUserId: string;
  note: string | null;
  occurredAtUtc: string;
}

export interface ProductStock {
  productId: string;
  quantityOnHand: number;
  recentTransactions: InventoryTransaction[];
}
