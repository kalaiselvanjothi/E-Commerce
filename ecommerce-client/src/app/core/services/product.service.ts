import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  ProductListItem, ProductDetail, PagedResult, ProductFilters,
  FilterMeta, SearchSuggestion, Category, Review
} from '../models/product.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly api = inject(ApiService);

  getProducts(filters: ProductFilters): Observable<PagedResult<ProductListItem>> {
    return this.api.get<PagedResult<ProductListItem>>('/products', filters as Record<string, unknown>);
  }

  getFeatured(): Observable<ProductListItem[]> {
    return this.api.get<ProductListItem[]>('/products/featured');
  }

  getBySlug(slug: string): Observable<ProductDetail> {
    return this.api.get<ProductDetail>(`/products/${slug}`);
  }

  searchSuggestions(query: string): Observable<SearchSuggestion[]> {
    return this.api.get<SearchSuggestion[]>('/products/suggestions', { q: query });
  }

  getFilterMeta(categorySlug?: string): Observable<FilterMeta> {
    return this.api.get<FilterMeta>('/products/filter-meta', { categorySlug });
  }

  getRecentlyViewed(): Observable<ProductListItem[]> {
    return this.api.get<ProductListItem[]>('/products/recently-viewed');
  }

  getCategories(): Observable<Category[]> {
    return this.api.get<Category[]>('/categories');
  }

  getCategoryBySlug(slug: string): Observable<Category> {
    return this.api.get<Category>(`/categories/${slug}`);
  }

  getReviews(productId: string, page = 1, pageSize = 10): Observable<PagedResult<Review>> {
    return this.api.get<PagedResult<Review>>(`/reviews/${productId}`, { page, pageSize });
  }

  createReview(productId: string, data: { rating: number; title: string; body: string }): Observable<Review> {
    return this.api.post<Review>(`/reviews/${productId}`, data);
  }

  markHelpful(reviewId: string): Observable<void> {
    return this.api.post<void>(`/reviews/${reviewId}/helpful`);
  }
}
