export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productSlug: string;
  productImage?: string;
  brand: string;
  variantId?: string;
  variantName?: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  stock: number;
}

export interface Cart {
  id: string;
  items: CartItem[];
  subtotal: number;
  shippingCost: number;
  taxAmount: number;
  discount: number;
  total: number;
  couponCode?: string;
  couponDiscount?: number;
  itemCount: number;
  isFreeShipping: boolean;
}

export interface AddToCartRequest {
  productId: string;
  variantId?: string;
  quantity: number;
}

export interface UpdateCartItemRequest {
  quantity: number;
}

export interface ApplyCouponRequest {
  code: string;
}

export interface CouponValidation {
  isValid: boolean;
  code: string;
  discountAmount: number;
  message: string;
  couponType: string;
  discountValue: number;
}
