import { Component, Input, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ProductListItem } from '../../../core/models/product.models';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { StarRatingComponent } from '../star-rating/star-rating.component';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, RouterLink, StarRatingComponent],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.scss'
})
export class ProductCardComponent {
  @Input({ required: true }) product!: ProductListItem;

  private readonly cart     = inject(CartService);
  private readonly wishlist = inject(WishlistService);
  private readonly toast    = inject(ToastService);
  private readonly auth     = inject(AuthService);
  private readonly router   = inject(Router);

  addingToCart  = false;
  togglingWish  = false;

  readonly fallbackImage = 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=600&q=80';

  get isWishlisted(): boolean {
    return this.wishlist.isInWishlist(this.product.id);
  }

  onImageError(e: Event): void {
    const target = e.target as HTMLImageElement;
    if (target && target.src !== this.fallbackImage) {
      target.src = this.fallbackImage;
    }
  }

  addToCart(e: Event): void {
    e.preventDefault(); e.stopPropagation();
    if (!this.auth.isLoggedIn()) { this.router.navigate(['/auth/login']); return; }
    if (this.addingToCart) return;
    this.addingToCart = true;
    this.cart.addItem({ productId: this.product.id, quantity: 1 }).subscribe({
      next: () => { this.toast.success('Added to cart!'); this.addingToCart = false; },
      error: () => { this.addingToCart = false; }
    });
  }

  buyNow(e: Event): void {
    e.preventDefault(); e.stopPropagation();
    if (!this.auth.isLoggedIn()) { this.router.navigate(['/auth/login']); return; }
    this.cart.addItem({ productId: this.product.id, quantity: 1 }).subscribe({
      next: () => { this.router.navigate(['/checkout']); },
      error: () => { this.router.navigate(['/checkout']); }
    });
  }

  toggleWishlist(e: Event): void {
    e.preventDefault(); e.stopPropagation();
    if (!this.auth.isLoggedIn()) { this.router.navigate(['/auth/login']); return; }
    if (this.togglingWish) return;
    this.togglingWish = true;
    this.wishlist.toggle(this.product.id).subscribe({
      next: () => {
        this.toast.success(this.isWishlisted ? 'Removed from wishlist' : 'Added to wishlist!');
        this.togglingWish = false;
      },
      error: () => { this.togglingWish = false; }
    });
  }
}
