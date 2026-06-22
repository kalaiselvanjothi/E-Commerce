import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { AdminProductListItem, CreateProductRequest } from '../../../core/models/admin.models';
import { PagedResult } from '../../../core/models/product.models';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './admin-products.component.html',
  styleUrl: './admin-products.component.scss'
})
export class AdminProductsComponent implements OnInit {
  private readonly api   = inject(ApiService);
  private readonly toast = inject(ToastService);

  result  = signal<PagedResult<AdminProductListItem> | null>(null);
  loading = signal(true);
  page    = 1;
  search  = '';
  confirmDeleteId = signal<string | null>(null);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    const params: Record<string, unknown> = { page: this.page, pageSize: 20 };
    if (this.search) params['search'] = this.search;
    this.api.get<PagedResult<AdminProductListItem>>('/admin/products', params).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  onSearch(): void { this.page = 1; this.load(); }

  toggleActive(id: string): void {
    this.api.patch<void>(`/admin/products/${id}/toggle-active`).subscribe({
      next: () => { this.toast.success('Visibility updated.'); this.load(); }
    });
  }

  delete(id: string): void {
    this.api.delete<void>(`/admin/products/${id}`).subscribe({
      next: () => { this.toast.success('Product deleted.'); this.confirmDeleteId.set(null); this.load(); }
    });
  }

  onPageChange(p: number): void { this.page = p; this.load(); }
}
