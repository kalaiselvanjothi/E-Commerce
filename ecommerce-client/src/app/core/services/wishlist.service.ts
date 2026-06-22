import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { Wishlist } from '../models/wishlist.models';

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private readonly api = inject(ApiService);

  private _wishlist = signal<Wishlist | null>(null);
  private _productIds = signal<Set<string>>(new Set());

  readonly wishlist   = this._wishlist.asReadonly();
  readonly itemCount  = () => this._wishlist()?.itemCount ?? 0;

  isInWishlist(productId: string): boolean {
    return this._productIds().has(productId);
  }

  getWishlist(): Observable<Wishlist> {
    return this.api.get<Wishlist>('/wishlist').pipe(
      tap(w => {
        this._wishlist.set(w);
        this._productIds.set(new Set(w.items.map(i => i.productId)));
      })
    );
  }

  toggle(productId: string): Observable<unknown> {
    if (this.isInWishlist(productId)) {
      return this.remove(productId);
    }
    return this.add(productId);
  }

  add(productId: string): Observable<void> {
    return this.api.post<void>('/wishlist', { productId }).pipe(
      tap(() => {
        this._productIds.update(s => new Set([...s, productId]));
        if (this._wishlist()) this.getWishlist().subscribe();
      })
    );
  }

  remove(productId: string): Observable<void> {
    return this.api.delete<void>(`/wishlist/${productId}`).pipe(
      tap(() => {
        this._productIds.update(s => { const n = new Set(s); n.delete(productId); return n; });
        if (this._wishlist()) {
          this._wishlist.update(w => w ? { ...w, items: w.items.filter(i => i.productId !== productId), itemCount: w.itemCount - 1 } : null);
        }
      })
    );
  }

  moveToCart(productId: string): Observable<void> {
    return this.api.post<void>(`/wishlist/${productId}/move-to-cart`).pipe(
      tap(() => {
        this._productIds.update(s => { const n = new Set(s); n.delete(productId); return n; });
        if (this._wishlist()) this.getWishlist().subscribe();
      })
    );
  }
}
