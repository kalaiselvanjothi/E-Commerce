import { Component, OnInit, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { ProductService } from '../../../core/services/product.service';
import { ProductListItem, PagedResult, FilterMeta } from '../../../core/models/product.models';
import { ProductCardComponent } from '../../../shared/components/product-card/product-card.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumb/breadcrumb.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ProductCardComponent, PaginationComponent, BreadcrumbComponent, EmptyStateComponent, SkeletonComponent],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss'
})
export class ProductListComponent implements OnInit, OnDestroy {
  private readonly productService = inject(ProductService);
  private readonly route          = inject(ActivatedRoute);
  private readonly router         = inject(Router);
  private destroy$ = new Subject<void>();

  result    = signal<PagedResult<ProductListItem> | null>(null);
  filterMeta = signal<FilterMeta | null>(null);
  loading   = signal(true);
  showFilters = signal(false);

  // Filter state
  categorySlug = '';
  search   = '';
  brand    = '';
  minPrice = '';
  maxPrice = '';
  minRating = '';
  inStock  = false;
  isFeatured = false;
  sortBy   = 'newest';
  page     = 1;
  pageSize = 24;

  readonly sortOptions = [
    { value: 'newest',    label: 'Newest First' },
    { value: 'popular',   label: 'Most Popular' },
    { value: 'rating',    label: 'Top Rated' },
    { value: 'price_asc', label: 'Price: Low to High' },
    { value: 'price_desc',label: 'Price: High to Low' },
  ];

  readonly skeletonItems = Array(12).fill(0);

  ngOnInit(): void {
    this.route.params.pipe(takeUntil(this.destroy$)).subscribe(p => {
      this.categorySlug = p['slug'] ?? '';
      this.page = 1;
      this.loadProducts();
      if (this.categorySlug) {
        this.productService.getFilterMeta(this.categorySlug).subscribe(m => this.filterMeta.set(m));
      } else {
        this.productService.getFilterMeta().subscribe(m => this.filterMeta.set(m));
      }
    });

    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(q => {
      this.search     = q['search'] ?? '';
      this.isFeatured = q['isFeatured'] === 'true';
      this.brand      = q['brand'] ?? '';
      this.sortBy     = q['sortBy'] ?? 'newest';
      this.page       = +q['page'] || 1;
      this.loadProducts();
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  loadProducts(): void {
    this.loading.set(true);
    const filters: Record<string, unknown> = {
      categorySlug: this.categorySlug || undefined,
      search:    this.search || undefined,
      brand:     this.brand || undefined,
      minPrice:  this.minPrice || undefined,
      maxPrice:  this.maxPrice || undefined,
      minRating: this.minRating || undefined,
      inStock:   this.inStock || undefined,
      isFeatured:this.isFeatured || undefined,
      sortBy:    this.sortBy,
      page:      this.page,
      pageSize:  this.pageSize,
    };
    this.productService.getProducts(filters as any).subscribe({
      next: r  => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  applyFilters(): void { this.page = 1; this.loadProducts(); this.showFilters.set(false); }

  clearFilters(): void {
    this.brand = ''; this.minPrice = ''; this.maxPrice = '';
    this.minRating = ''; this.inStock = false;
    this.applyFilters();
  }

  onSortChange(val: string): void { this.sortBy = val; this.loadProducts(); }

  onPageChange(p: number): void {
    this.page = p;
    this.loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  get pageTitle(): string {
    if (this.search)       return `Results for "${this.search}"`;
    if (this.categorySlug) return this.categorySlug.replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
    if (this.isFeatured)   return 'Featured Products';
    return 'All Products';
  }
}
