import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { WishlistService } from '../../core/services/wishlist.service';
import { CartService } from '../../core/services/cart.service';
import { ToastService } from '../../core/services/toast.service';
import { WishlistItem } from '../../core/models/wishlist.models';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { StarRatingComponent } from '../../shared/components/star-rating/star-rating.component';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterLink, StarRatingComponent],
  templateUrl: './wishlist.component.html',
  styleUrl: './wishlist.component.scss'
})
export class WishlistComponent implements OnInit {
  private readonly wishlistService = inject(WishlistService);
  private readonly cartService     = inject(CartService);
  private readonly toast           = inject(ToastService);

  items   = signal<WishlistItem[]>([]);
  loading = signal(true);
  movingId = signal<string | null>(null);

  ngOnInit(): void {
    this.wishlistService.getWishlist().subscribe({
      next: w => { this.items.set(w.items); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  remove(productId: string): void {
    this.wishlistService.remove(productId).subscribe({
      next: () => {
        this.items.update(list => list.filter(i => i.productId !== productId));
        this.toast.success('Removed from wishlist.');
      }
    });
  }

  moveToCart(productId: string): void {
    this.movingId.set(productId);
    this.wishlistService.moveToCart(productId).subscribe({
      next: () => {
        this.items.update(list => list.filter(i => i.productId !== productId));
        this.toast.success('Moved to cart!');
        this.movingId.set(null);
        this.cartService.getCart().subscribe();
      },
      error: () => this.movingId.set(null)
    });
  }
}
