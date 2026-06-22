import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { CartService } from '../../../core/services/cart.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { ProductDetail, ProductVariant } from '../../../core/models/product.models';
import { StarRatingComponent } from '../../../shared/components/star-rating/star-rating.component';
import { ProductCardComponent } from '../../../shared/components/product-card/product-card.component';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumb/breadcrumb.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, StarRatingComponent, ProductCardComponent, BreadcrumbComponent, SkeletonComponent],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss'
})
export class ProductDetailComponent implements OnInit {
  private readonly route    = inject(ActivatedRoute);
  private readonly router   = inject(Router);
  private readonly products = inject(ProductService);
  private readonly cart     = inject(CartService);
  private readonly wishlist = inject(WishlistService);
  private readonly auth     = inject(AuthService);
  private readonly toast    = inject(ToastService);

  product       = signal<ProductDetail | null>(null);
  loading       = signal(true);
  activeImage   = signal(0);
  selectedVariant = signal<ProductVariant | null>(null);
  quantity      = signal(1);
  addingToCart  = signal(false);
  activeTab     = signal<'description' | 'specs' | 'reviews'>('description');
  newRating     = signal(0);
  reviewTitle   = '';
  reviewBody    = '';
  submittingReview = signal(false);

  get isWishlisted() { return this.product() ? this.wishlist.isInWishlist(this.product()!.id) : false; }
  get isLoggedIn()   { return this.auth.isLoggedIn(); }

  ngOnInit(): void {
    this.route.params.subscribe(p => {
      this.loading.set(true);
      this.products.getBySlug(p['slug']).subscribe({
        next: prod => { this.product.set(prod); this.loading.set(false); },
        error: () => { this.loading.set(false); this.router.navigate(['/']); }
      });
    });
  }

  selectImage(i: number): void { this.activeImage.set(i); }

  selectVariant(v: ProductVariant): void {
    this.selectedVariant.set(this.selectedVariant()?.id === v.id ? null : v);
  }

  adjustQty(delta: number): void {
    const max = this.product()?.stock ?? 1;
    this.quantity.set(Math.min(max, Math.max(1, this.quantity() + delta)));
  }

  addToCart(): void {
    if (!this.isLoggedIn) { this.router.navigate(['/auth/login']); return; }
    if (this.addingToCart()) return;
    this.addingToCart.set(true);
    this.cart.addItem({
      productId: this.product()!.id,
      variantId: this.selectedVariant()?.id,
      quantity:  this.quantity()
    }).subscribe({
      next: () => { this.toast.success('Added to cart!'); this.addingToCart.set(false); },
      error: () => this.addingToCart.set(false)
    });
  }

  toggleWishlist(): void {
    if (!this.isLoggedIn) { this.router.navigate(['/auth/login']); return; }
    this.wishlist.toggle(this.product()!.id).subscribe({
      next: () => this.toast.success(this.isWishlisted ? 'Removed from wishlist' : 'Added to wishlist!')
    });
  }

  submitReview(): void {
    if (!this.newRating() || !this.reviewBody) return;
    this.submittingReview.set(true);
    this.products.createReview(this.product()!.id, {
      rating: this.newRating(), title: this.reviewTitle, body: this.reviewBody
    }).subscribe({
      next: () => {
        this.toast.success('Review submitted!');
        this.submittingReview.set(false);
        this.newRating.set(0); this.reviewTitle = ''; this.reviewBody = '';
        this.products.getBySlug(this.product()!.slug).subscribe(p => this.product.set(p));
      },
      error: () => this.submittingReview.set(false)
    });
  }

  get effectivePrice(): number {
    const p = this.product();
    if (!p) return 0;
    return p.price + (this.selectedVariant()?.priceModifier ?? 0);
  }

  get breadcrumbs() {
    const p = this.product();
    if (!p) return [];
    return [
      { label: 'Home', link: '/' },
      { label: p.categoryName, link: `/category/${p.categorySlug}` },
      { label: p.name }
    ];
  }
}
