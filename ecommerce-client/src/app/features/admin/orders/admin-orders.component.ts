import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { PagedResult } from '../../../core/models/product.models';
import { OrderSummary, OrderStatus } from '../../../core/models/order.models';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  template: `
    <div class="admin-page">
      <div class="page-header">
        <h1 class="page-title">Orders</h1>
        <select class="form-control" style="width:180px" [(ngModel)]="statusFilter" (ngModelChange)="onFilter()">
          <option value="">All Statuses</option>
          @for (s of statuses; track s) { <option [value]="s">{{ s }}</option> }
        </select>
      </div>

      @if (loading()) {
        <div class="skeleton" style="height:400px;border-radius:16px"></div>
      } @else {
        <div class="table-card">
          <table class="data-table">
            <thead><tr><th>Order</th><th>Customer</th><th>Total</th><th>Payment</th><th>Status</th><th>Date</th><th>Update</th></tr></thead>
            <tbody>
              @for (o of result()?.items; track o.id) {
                <tr>
                  <td class="mono">{{ o.orderNumber }}</td>
                  <td>{{ o.id | slice:0:8 }}…</td>
                  <td>₹{{ o.total | number }}</td>
                  <td>{{ o.paymentMethod }}</td>
                  <td><span class="status-chip status-{{ o.status.toLowerCase() }}">{{ o.status }}</span></td>
                  <td>{{ o.createdAt | date:'mediumDate' }}</td>
                  <td>
                    <select class="mini-select" (change)="updateStatus(o.id, $any($event.target).value)">
                      <option value="">Change…</option>
                      @for (s of statuses; track s) { <option [value]="s">{{ s }}</option> }
                    </select>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <app-pagination [currentPage]="result()!.page" [totalPages]="result()!.totalPages" (pageChange)="onPageChange($event)" />
      }
    </div>
  `,
  styleUrl: '../products/admin-products.component.scss'
})
export class AdminOrdersComponent implements OnInit {
  private readonly api   = inject(ApiService);
  private readonly toast = inject(ToastService);

  result       = signal<PagedResult<OrderSummary> | null>(null);
  loading      = signal(true);
  page         = 1;
  statusFilter = '';

  readonly statuses: OrderStatus[] = ['Placed','Confirmed','Packed','Shipped','OutForDelivery','Delivered','Cancelled','ReturnRequested','Returned'];

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    const params: Record<string, unknown> = { page: this.page, pageSize: 20 };
    if (this.statusFilter) params['status'] = this.statusFilter;
    this.api.get<PagedResult<OrderSummary>>('/orders/admin', params).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  onFilter(): void { this.page = 1; this.load(); }
  onPageChange(p: number): void { this.page = p; this.load(); }

  updateStatus(orderId: string, status: string): void {
    if (!status) return;
    this.api.patch<void>(`/orders/admin/${orderId}/status`, { status }).subscribe({
      next: () => { this.toast.success('Order status updated.'); this.load(); }
    });
  }
}
