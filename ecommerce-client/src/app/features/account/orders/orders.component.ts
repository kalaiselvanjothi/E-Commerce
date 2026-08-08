import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { OrderSummary } from '../../../core/models/order.models';
import { PagedResult } from '../../../core/models/product.models';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.scss'
})
export class OrdersComponent implements OnInit {
  private readonly orderService = inject(OrderService);
  result  = signal<PagedResult<OrderSummary> | null>(null);
  loading = signal(true);
  page    = 1;

  ngOnInit(): void { this.loadPage(1); }

  loadPage(p: number): void {
    this.page = p;
    this.loading.set(true);
    this.orderService.getMyOrders(p, 10).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      placed: 'status-placed',
      confirmed: 'status-confirmed',
      packed: 'status-packed',
      shipped: 'status-shipped',
      outfordelivery: 'status-outfordelivery',
      delivered: 'status-delivered',
      cancelled: 'status-cancelled',
      returned: 'status-returned',
    };
    return map[status.toLowerCase()] || 'status-placed';
  }

  getStatusStep(status: string): number {
    const steps: Record<string, number> = {
      placed: 1, confirmed: 2, packed: 3, shipped: 4,
      outfordelivery: 5, delivered: 6
    };
    return steps[status.toLowerCase()] || 1;
  }
}
