export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  parentId?: string;
  sortOrder: number;
  isActive: boolean;
  productCount: number;
  children: Category[];
}

export interface ProductImage {
  id: string;
  url: string;
  altText?: string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface ProductVariant {
  id: string;
  name: string;
  value: string;
  priceModifier: number;
  stock: number;
  sku?: string;
}

export interface ReviewSummary {
  averageRating: number;
  totalReviews: number;
  ratingBreakdown: Record<number, number>;
}

export interface ProductListItem {
  id: string;
  name: string;
  slug: string;
  brand: string;
  shortDescription?: string;
  price: number;
  compareAtPrice?: number;
  discountPercent?: number;
  primaryImageUrl?: string;
  averageRating: number;
  totalReviews: number;
  totalSold: number;
  stock: number;
  isFeatured: boolean;
  isActive: boolean;
  categoryName: string;
  categorySlug: string;
  tags: string[];
}

export interface ProductDetail extends ProductListItem {
  description: string;
  sku: string;
  weight?: number;
  specifications: Record<string, string>;
  images: ProductImage[];
  variants: ProductVariant[];
  reviews: PagedResult<Review>;
  reviewSummary: ReviewSummary;
  relatedProducts: ProductListItem[];
  createdAt: string;
}

export interface Review {
  id: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  rating: number;
  title: string;
  body: string;
  isVerifiedPurchase: boolean;
  helpfulCount: number;
  isHidden: boolean;
  createdAt: string;
}

export interface ProductFilters {
  categorySlug?: string;
  search?: string;
  brand?: string;
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  inStock?: boolean;
  isFeatured?: boolean;
  sortBy?: 'newest' | 'price_asc' | 'price_desc' | 'rating' | 'popular';
  page?: number;
  pageSize?: number;
}

export interface FilterMeta {
  brands: string[];
  minPrice: number;
  maxPrice: number;
}

export interface SearchSuggestion {
  id: string;
  name: string;
  slug: string;
  primaryImageUrl?: string;
  price: number;
  categoryName: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
