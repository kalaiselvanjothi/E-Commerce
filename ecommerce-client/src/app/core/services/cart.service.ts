import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { Cart, AddToCartRequest, UpdateCartItemRequest, ApplyCouponRequest, CouponValidation } from '../models/cart.models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly api = inject(ApiService);

  private _cart = signal<Cart | null>(null);
  readonly cart     = this._cart.asReadonly();
  readonly itemCount = () => this._cart()?.itemCount ?? 0;

  getCart(): Observable<Cart> {
    return this.api.get<Cart>('/cart').pipe(
      tap(c => this._cart.set(c))
    );
  }

  addItem(dto: AddToCartRequest): Observable<Cart> {
    return this.api.post<Cart>('/cart/items', dto).pipe(
      tap(c => this._cart.set(c))
    );
  }

  updateItem(itemId: string, dto: UpdateCartItemRequest): Observable<Cart> {
    return this.api.put<Cart>(`/cart/items/${itemId}`, dto).pipe(
      tap(c => this._cart.set(c))
    );
  }

  removeItem(itemId: string): Observable<Cart> {
    return this.api.delete<Cart>(`/cart/items/${itemId}`).pipe(
      tap(c => this._cart.set(c))
    );
  }

  applyCoupon(dto: ApplyCouponRequest): Observable<Cart> {
    return this.api.post<Cart>('/cart/coupon', dto).pipe(
      tap(c => this._cart.set(c))
    );
  }

  removeCoupon(): Observable<Cart> {
    return this.api.delete<Cart>('/cart/coupon').pipe(
      tap(c => this._cart.set(c))
    );
  }

  validateCoupon(code: string): Observable<CouponValidation> {
    return this.api.post<CouponValidation>('/cart/coupon/validate', { code });
  }

  clearLocal(): void {
    this._cart.set(null);
  }
}
