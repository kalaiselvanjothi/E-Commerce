import { PagedResult } from './product.models';

export interface DashboardStats {
  totalRevenue: number;
  revenueChange: number;
  totalOrders: number;
  ordersChange: number;
  totalCustomers: number;
  customersChange: number;
  totalProducts: number;
  pendingOrders: number;
  revenueChart: ChartDataPoint[];
  ordersChart: ChartDataPoint[];
  topProducts: TopProductItem[];
  recentOrders: RecentOrderItem[];
}

export interface ChartDataPoint {
  date: string;
  value: number;
}

export interface TopProductItem {
  id: string;
  name: string;
  imageUrl?: string;
  totalSold: number;
  revenue: number;
}

export interface RecentOrderItem {
  id: string;
  orderNumber: string;
  customerName: string;
  total: number;
  status: string;
  createdAt: string;
}

export interface AdminProductListItem {
  id: string;
  name: string;
  slug: string;
  brand: string;
  price: number;
  stock: number;
  categoryName: string;
  isActive: boolean;
  isFeatured: boolean;
  totalSold: number;
  primaryImageUrl?: string;
  createdAt: string;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  shortDescription?: string;
  brand: string;
  categoryId: string;
  price: number;
  compareAtPrice?: number;
  stock: number;
  sku?: string;
  weight?: number;
  isFeatured: boolean;
  isActive: boolean;
  tags: string[];
  specifications: Record<string, string>;
  images: { url: string; altText?: string; isPrimary: boolean; sortOrder: number }[];
  variants: { name: string; value: string; priceModifier: number; stock: number; sku?: string }[];
}

export interface UpdateProductRequest extends CreateProductRequest {}

export interface CustomerAdminItem {
  id: string;
  fullName: string;
  email: string;
  avatarUrl?: string;
  isActive: boolean;
  totalOrders: number;
  totalSpent: number;
  createdAt: string;
}

export interface CouponAdminItem {
  id: string;
  code: string;
  description?: string;
  couponType: string;
  discountValue: number;
  maxDiscountAmount?: number;
  minimumOrderAmount?: number;
  usageLimit?: number;
  usageCount: number;
  isActive: boolean;
  expiryDate?: string;
  createdAt: string;
}

export interface CreateCouponRequest {
  code: string;
  description?: string;
  couponType: 'Percentage' | 'FixedAmount' | 'FreeShipping';
  discountValue: number;
  maxDiscountAmount?: number;
  minimumOrderAmount?: number;
  usageLimit?: number;
  isActive: boolean;
  expiryDate?: string;
}
