import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { OrderSummary } from '../../../core/models/order.models';
import { PagedResult } from '../../../core/models/product.models';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent, EmptyStateComponent],
  template: `
    <div class="account-page">
      <h1 class="account-title">My Orders</h1>
      @if (loading()) {
        <div class="orders-list">
          @for (i of [1,2,3]; track i) {
            <div class="order-skeleton">
              <div class="skeleton" style="height:90px;border-radius:12px"></div>
            </div>
          }
        </div>
      } @else if (!result()?.items?.length) {
        <app-empty-state icon="📦" title="No orders yet" description="Your orders will appear here once you start shopping." actionLabel="Start Shopping" actionLink="/products" />
      } @else {
        <div class="orders-list">
          @for (order of result()!.items; track order.id) {
            <a [routerLink]="['/account/orders', order.id]" class="order-card">
              <div class="order-img">
                <img [src]="order.primaryImage || '/assets/placeholder.svg'" alt="Order" />
              </div>
              <div class="order-info">
                <div class="order-number">{{ order.orderNumber }}</div>
                <div class="order-date">{{ order.createdAt | date:'mediumDate' }}</div>
                <div class="order-items">{{ order.itemCount }} item{{ order.itemCount === 1 ? '' : 's' }}</div>
              </div>
              <div class="order-right">
                <span class="order-status status-{{ order.status.toLowerCase() }}">{{ order.status }}</span>
                <span class="order-total">₹{{ order.total | number }}</span>
                <span class="order-method">{{ order.paymentMethod }}</span>
              </div>
            </a>
          }
        </div>
        <app-pagination [currentPage]="page" [totalPages]="result()!.totalPages" (pageChange)="loadPage($event)" />
      }
    </div>
  `,
  styleUrl: './orders.component.scss'
})
export class OrdersComponent implements OnInit {
  private readonly orderService = inject(OrderService);
  result  = signal<PagedResult<OrderSummary> | null>(null);
  loading = signal(true);
  page    = 1;

  ngOnInit(): void { this.loadPage(1); }

  loadPage(p: number): void {
    this.page = p; this.loading.set(true);
    this.orderService.getMyOrders(p, 10).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
