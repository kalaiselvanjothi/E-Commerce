export type OrderStatus =
  | 'Placed' | 'Confirmed' | 'Packed' | 'Shipped' | 'OutForDelivery' | 'Delivered'
  | 'Cancelled' | 'ReturnRequested' | 'Returned';

export type PaymentStatus = 'Pending' | 'Paid' | 'Failed' | 'Refunded';
export type PaymentMethod = 'Razorpay' | 'COD' | 'Wallet';
export type DeliveryOption = 'Standard' | 'Express' | 'SameDay';

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  productSlug: string;
  productImage?: string;
  brand: string;
  variantName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface OrderStatusHistory {
  status: OrderStatus;
  note?: string;
  createdAt: string;
}

export interface Address {
  id: string;
  fullName: string;
  phone: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  pincode: string;
  country: string;
  isDefault: boolean;
  addressType: string;
}

export interface Order {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  paymentMethod: PaymentMethod;
  deliveryOption: DeliveryOption;
  items: OrderItem[];
  shippingAddress: Address;
  subtotal: number;
  shippingCost: number;
  taxAmount: number;
  discount: number;
  couponCode?: string;
  total: number;
  statusHistory: OrderStatusHistory[];
  estimatedDelivery?: string;
  razorpayOrderId?: string;
  cancelReason?: string;
  returnReason?: string;
  createdAt: string;
  updatedAt: string;
}

export interface OrderSummary {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  paymentMethod: PaymentMethod;
  total: number;
  itemCount: number;
  primaryImage?: string;
  createdAt: string;
}

export interface CreateOrderRequest {
  addressId: string;
  paymentMethod: PaymentMethod;
  deliveryOption: DeliveryOption;
  couponCode?: string;
}

export interface CancelOrderRequest {
  reason: string;
}

export interface ReturnOrderRequest {
  reason: string;
}

export interface RazorpayOrderResponse {
  razorpayOrderId: string;
  amount: number;
  currency: string;
  orderId: string;
  keyId: string;
}

export interface PaymentVerifyRequest {
  orderId: string;
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
}
