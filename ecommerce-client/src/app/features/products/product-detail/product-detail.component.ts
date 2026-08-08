import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
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
  imports: [CommonModule, FormsModule, StarRatingComponent, ProductCardComponent, BreadcrumbComponent, SkeletonComponent],
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
  selectedSize  = signal<ProductVariant | null>(null);
  selectedColor = signal<ProductVariant | null>(null);
  quantity      = signal(1);
  addingToCart  = signal(false);
  activeTab     = signal<'description' | 'specs' | 'reviews'>('description');
  newRating     = signal(0);
  reviewTitle   = '';
  reviewBody    = '';
  submittingReview = signal(false);
  fallbackImage = 'https://images.unsplash.com/photo-1498049794561-7780e7231661?w=600';

  get isWishlisted() { return this.product() ? this.wishlist.isInWishlist(this.product()!.id) : false; }
  get isLoggedIn()   { return this.auth.isLoggedIn(); }

  get sizeVariants(): ProductVariant[] {
    return this.product()?.variants?.filter(v => v.type === 'size' || v.name?.toLowerCase().includes('size')) ?? [];
  }

  get colorVariants(): ProductVariant[] {
    return this.product()?.variants?.filter(v => v.type === 'color' || v.name?.toLowerCase().includes('color')) ?? [];
  }

  get unitPrice(): number {
    const p = this.product();
    if (!p) return 0;
    const modifier = (this.selectedSize()?.priceModifier ?? 0) + (this.selectedColor()?.priceModifier ?? 0) + (this.selectedVariant()?.priceModifier ?? 0);
    return p.price + modifier;
  }

  get totalPrice(): number {
    return this.unitPrice * this.quantity();
  }

  get compareAtUnitPrice(): number {
    return this.product()?.compareAtPrice ?? 0;
  }

  get totalCompareAtPrice(): number {
    return this.compareAtUnitPrice * this.quantity();
  }

  get totalSavings(): number {
    return this.totalCompareAtPrice > this.totalPrice ? (this.totalCompareAtPrice - this.totalPrice) : 0;
  }

  get taxAmount(): number {
    return Math.round(this.totalPrice * 0.18 / 1.18);
  }

  get existingCartItem() {
    const p = this.product();
    if (!p) return null;
    const cartItems = this.cart.cart()?.items ?? [];
    return cartItems.find(item => item.productId === p.id);
  }

  syncQtyWithCart(): void {
    const item = this.existingCartItem;
    if (item) {
      this.quantity.set(item.quantity);
    }
  }

  ngOnInit(): void {
    this.route.params.subscribe(p => {
      this.loading.set(true);
      this.products.getBySlug(p['slug']).subscribe({
        next: prod => {
          this.product.set(prod);
          this.loading.set(false);
          if (this.sizeVariants.length > 0) {
            this.selectedSize.set(this.sizeVariants[0]);
          }
          if (this.colorVariants.length > 0) {
            this.selectedColor.set(this.colorVariants[0]);
          }
          this.syncQtyWithCart();
        },
        error: () => { this.loading.set(false); this.router.navigate(['/']); }
      });
    });
  }

  selectImage(i: number): void { this.activeImage.set(i); }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = this.fallbackImage;
  }

  selectSize(v: ProductVariant): void {
    this.selectedSize.set(this.selectedSize()?.id === v.id ? null : v);
    this.syncQtyWithCart();
  }

  selectColor(v: ProductVariant): void {
    this.selectedColor.set(this.selectedColor()?.id === v.id ? null : v);
    this.syncQtyWithCart();
  }

  get maxStock(): number {
    const p = this.product();
    if (!p) return 99;
    const selectedV = this.selectedSize() || this.selectedColor();
    if (selectedV) {
      const vStock = selectedV.stockQuantity ?? selectedV.stock;
      if (vStock !== undefined && vStock > 0) return vStock;
    }
    return p.stockQuantity ?? p.stock ?? 99;
  }

  adjustQty(delta: number): void {
    const max = this.maxStock;
    const current = this.quantity();
    const newQty = Math.min(max, Math.max(1, current + delta));
    this.quantity.set(newQty);

    const item = this.existingCartItem;
    if (item) {
      this.cart.updateItem(item.id, { quantity: newQty }).subscribe();
    }
  }

  private get activeVariantId(): string | undefined {
    return this.selectedSize()?.id ?? this.selectedColor()?.id ?? this.selectedVariant()?.id;
  }

  addToCart(): void {
    if (!this.isLoggedIn) { this.router.navigate(['/auth/login']); return; }
    if (this.addingToCart()) return;
    this.addingToCart.set(true);
    this.cart.addItem({
      productId: this.product()!.id,
      variantId: this.activeVariantId,
      quantity:  this.quantity()
    }).subscribe({
      next: () => { this.toast.success('Added to cart!'); this.addingToCart.set(false); },
      error: () => this.addingToCart.set(false)
    });
  }

  buyNow(): void {
    if (!this.isLoggedIn) { this.router.navigate(['/auth/login']); return; }
    this.cart.addItem({
      productId: this.product()!.id,
      variantId: this.activeVariantId,
      quantity:  this.quantity()
    }).subscribe({
      next: () => { this.router.navigate(['/checkout']); },
      error: () => { this.router.navigate(['/checkout']); }
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
      rating: this.newRating(),
      title: this.reviewTitle || 'Customer Review',
      body: this.reviewBody
    }).subscribe({
      next: () => {
        this.toast.success('Review submitted successfully!');
        this.submittingReview.set(false);
        this.newRating.set(0);
        this.reviewTitle = '';
        this.reviewBody = '';
        this.products.getBySlug(this.product()!.slug).subscribe(p => this.product.set(p));
      },
      error: (err) => {
        this.submittingReview.set(false);
        const errorMessage = err?.error?.message || err?.message || 'Only verified buyers who have received this product are eligible to leave a review.';
        this.toast.error(errorMessage);
      }
    });
  }

  get effectivePrice(): number {
    return this.unitPrice;
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
