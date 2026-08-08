import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductCardComponent } from '../../shared/components/product-card/product-card.component';
import { ProductListItem } from '../../core/models/product.models';
import { ProductService } from '../../core/services/product.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-offers',
  standalone: true,
  imports: [CommonModule, ProductCardComponent],
  templateUrl: './offers.component.html',
  styleUrl: './offers.component.scss'
})
export class OffersComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly toast = inject(ToastService);

  coupons = [
    { code: 'WELCOME10', discount: '10% OFF', title: 'Welcome Special', desc: 'Valid on your first order of ₹500 or more.', minOrder: '₹500' },
    { code: 'SAVE500', discount: '₹500 OFF', title: 'Big Savings Deal', desc: 'Flat ₹500 off on orders above ₹2,999.', minOrder: '₹2,999' },
    { code: 'SUMMER25', discount: '25% OFF', title: 'Summer Sale', desc: '25% discount across summer deals.', minOrder: '₹1,000' }
  ];

  saleProducts = signal<ProductListItem[]>([]);

  ngOnInit(): void {
    this.productService.getFeatured().subscribe({
      next: (products) => {
        if (products && products.length > 0) {
          this.saleProducts.set(products);
        } else {
          this.productService.getProducts({ pageSize: 12 }).subscribe(res => {
            this.saleProducts.set(res.items);
          });
        }
      },
      error: () => {
        this.productService.getProducts({ pageSize: 12 }).subscribe(res => {
          this.saleProducts.set(res.items);
        });
      }
    });
  }

  copyCode(code: string): void {
    navigator.clipboard.writeText(code);
    this.toast.success(`Coupon code ${code} copied to clipboard!`);
  }
}
