import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../header/header.component';
import { FooterComponent } from '../footer/footer.component';
import { ToastContainerComponent } from '../../shared/components/toast/toast-container.component';
import { CartService } from '../../core/services/cart.service';
import { WishlistService } from '../../core/services/wishlist.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, FooterComponent, ToastContainerComponent],
  template: `
    <app-header />
    <main class="page-wrapper">
      <router-outlet />
    </main>
    <app-footer />
    <app-toast-container />
  `
})
export class ShellComponent implements OnInit {
  private readonly auth      = inject(AuthService);
  private readonly cart      = inject(CartService);
  private readonly wishlist  = inject(WishlistService);

  ngOnInit(): void {
    if (this.auth.isLoggedIn()) {
      this.cart.getCart().subscribe({ error: () => {} });
      this.wishlist.getWishlist().subscribe({ error: () => {} });
    }
  }
}
