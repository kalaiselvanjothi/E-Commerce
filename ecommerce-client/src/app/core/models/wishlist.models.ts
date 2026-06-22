export interface WishlistItem {
  id: string;
  productId: string;
  productName: string;
  productSlug: string;
  productImage?: string;
  brand: string;
  price: number;
  compareAtPrice?: number;
  discountPercent?: number;
  stock: number;
  averageRating: number;
  addedAt: string;
}

export interface Wishlist {
  id: string;
  items: WishlistItem[];
  itemCount: number;
}
