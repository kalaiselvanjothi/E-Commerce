import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../core/services/product.service';
import { ProductListItem, Category } from '../../core/models/product.models';
import { ProductCardComponent } from '../../shared/components/product-card/product-card.component';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, ProductCardComponent, SkeletonComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  private readonly productService = inject(ProductService);

  featured    = signal<ProductListItem[]>([]);
  categories  = signal<Category[]>([]);
  recentlyViewed = signal<ProductListItem[]>([]);
  loading     = signal(true);

  ngOnInit(): void {
    this.productService.getFeatured().subscribe({
      next: p => { this.featured.set(p); this.loading.set(false); },
      error: () => this.loading.set(false)
    });

    this.productService.getCategories().subscribe(cats => {
      this.categories.set(cats.filter(c => !c.parentId).slice(0, 8));
    });

    this.productService.getRecentlyViewed().subscribe({
      next: p => this.recentlyViewed.set(p),
      error: () => {}
    });
  }

  readonly skeletonItems = Array(10).fill(0);
}
