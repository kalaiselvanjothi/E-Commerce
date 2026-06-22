import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../core/services/cart.service';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/services/auth.service';
import { Cart } from '../../core/models/cart.models';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, EmptyStateComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss'
})
export class CartComponent implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly toast       = inject(ToastService);
  readonly auth                = inject(AuthService);

  cart    = signal<Cart | null>(null);
  loading = signal(true);
  couponCode = '';
  applyingCoupon = signal(false);

  ngOnInit(): void {
    this.cartService.getCart().subscribe({
      next: c => { this.cart.set(c); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  updateQty(itemId: string, qty: number): void {
    if (qty < 1) return;
    this.cartService.updateItem(itemId, { quantity: qty }).subscribe({
      next: c => this.cart.set(c),
      error: () => {}
    });
  }

  removeItem(itemId: string): void {
    this.cartService.removeItem(itemId).subscribe({
      next: c => { this.cart.set(c); this.toast.success('Item removed.'); }
    });
  }

  applyCoupon(): void {
    if (!this.couponCode.trim()) return;
    this.applyingCoupon.set(true);
    this.cartService.applyCoupon({ code: this.couponCode.toUpperCase() }).subscribe({
      next: c => { this.cart.set(c); this.toast.success('Coupon applied!'); this.applyingCoupon.set(false); },
      error: () => this.applyingCoupon.set(false)
    });
  }

  removeCoupon(): void {
    this.cartService.removeCoupon().subscribe({
      next: c => { this.cart.set(c); this.couponCode = ''; this.toast.success('Coupon removed.'); }
    });
  }
}
