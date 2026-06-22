import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { CustomerAdminItem } from '../../../core/models/admin.models';
import { PagedResult } from '../../../core/models/product.models';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-admin-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  template: `
    <div class="admin-page">
      <div class="page-header">
        <h1 class="page-title">Customers</h1>
        <div class="header-actions">
          <input type="text" class="form-control search-input" [(ngModel)]="search" placeholder="Search customers…" (keydown.enter)="onSearch()" />
          <button class="btn-primary" (click)="onSearch()">Search</button>
        </div>
      </div>
      @if (loading()) {
        <div class="skeleton" style="height:400px;border-radius:16px"></div>
      } @else {
        <div class="table-card">
          <table class="data-table">
            <thead><tr><th>Customer</th><th>Email</th><th>Orders</th><th>Spent</th><th>Joined</th><th>Status</th><th>Action</th></tr></thead>
            <tbody>
              @for (c of result()?.items; track c.id) {
                <tr>
                  <td>
                    <div class="customer-cell">
                      <div class="cust-avatar">{{ c.fullName[0] }}</div>
                      <span>{{ c.fullName }}</span>
                    </div>
                  </td>
                  <td>{{ c.email }}</td>
                  <td>{{ c.totalOrders }}</td>
                  <td>₹{{ c.totalSpent | number }}</td>
                  <td>{{ c.createdAt | date:'mediumDate' }}</td>
                  <td><span class="status-chip" [class.active]="c.isActive" [class.inactive]="!c.isActive">{{ c.isActive ? 'Active' : 'Banned' }}</span></td>
                  <td><button class="icon-btn" (click)="toggleActive(c.id)" [title]="c.isActive ? 'Ban' : 'Activate'">{{ c.isActive ? '🔒' : '🔓' }}</button></td>
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
export class AdminCustomersComponent implements OnInit {
  private readonly api   = inject(ApiService);
  private readonly toast = inject(ToastService);

  result  = signal<PagedResult<CustomerAdminItem> | null>(null);
  loading = signal(true);
  page    = 1;
  search  = '';

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    const params: Record<string, unknown> = { page: this.page, pageSize: 20 };
    if (this.search) params['search'] = this.search;
    this.api.get<PagedResult<CustomerAdminItem>>('/admin/customers', params).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  onSearch(): void { this.page = 1; this.load(); }
  onPageChange(p: number): void { this.page = p; this.load(); }

  toggleActive(id: string): void {
    this.api.patch<void>(`/admin/customers/${id}/toggle-active`).subscribe({
      next: () => { this.toast.success('Customer status updated.'); this.load(); }
    });
  }
}
