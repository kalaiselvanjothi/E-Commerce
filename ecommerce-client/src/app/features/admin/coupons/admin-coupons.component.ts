import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { CouponAdminItem } from '../../../core/models/admin.models';

@Component({
  selector: 'app-admin-coupons',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="admin-page">
      <div class="page-header">
        <h1 class="page-title">Coupons</h1>
        <button class="btn-primary" (click)="showForm.set(!showForm())">+ New Coupon</button>
      </div>

      @if (showForm()) {
        <div class="form-card">
          <h3>{{ editId() ? 'Edit' : 'New' }} Coupon</h3>
          <form [formGroup]="form" (ngSubmit)="save()" novalidate>
            <div class="form-row">
              <div class="form-group"><label>Code</label><input type="text" formControlName="code" class="form-control" placeholder="SUMMER25" style="text-transform:uppercase" /></div>
              <div class="form-group"><label>Type</label>
                <select formControlName="couponType" class="form-control">
                  <option value="Percentage">Percentage (%)</option>
                  <option value="FixedAmount">Fixed Amount (₹)</option>
                  <option value="FreeShipping">Free Shipping</option>
                </select>
              </div>
              <div class="form-group"><label>Value</label><input type="number" formControlName="discountValue" class="form-control" /></div>
            </div>
            <div class="form-row">
              <div class="form-group"><label>Max Discount (₹)</label><input type="number" formControlName="maxDiscountAmount" class="form-control" placeholder="Optional cap" /></div>
              <div class="form-group"><label>Min Order (₹)</label><input type="number" formControlName="minimumOrderAmount" class="form-control" /></div>
              <div class="form-group"><label>Usage Limit</label><input type="number" formControlName="usageLimit" class="form-control" /></div>
            </div>
            <div class="form-row">
              <div class="form-group"><label>Expiry Date</label><input type="date" formControlName="expiryDate" class="form-control" /></div>
              <div class="form-group"><label>Description</label><input type="text" formControlName="description" class="form-control" /></div>
              <div class="form-group" style="display:flex;align-items:flex-end;padding-bottom:4px"><label style="display:flex;gap:8px;align-items:center;cursor:pointer"><input type="checkbox" formControlName="isActive" style="accent-color:var(--color-primary)" /> Active</label></div>
            </div>
            <div class="form-btns">
              <button type="button" class="btn-ghost" (click)="cancelForm()">Cancel</button>
              <button type="submit" class="btn-primary" [disabled]="form.invalid || saving()">
                @if (saving()) { Saving… } @else { Save }
              </button>
            </div>
          </form>
        </div>
      }

      <div class="table-card">
        <table class="data-table">
          <thead><tr><th>Code</th><th>Type</th><th>Value</th><th>Min Order</th><th>Used</th><th>Expiry</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>
            @for (c of coupons(); track c.id) {
              <tr>
                <td class="mono coupon-code">{{ c.code }}</td>
                <td>{{ c.couponType }}</td>
                <td>{{ c.couponType === 'Percentage' ? c.discountValue + '%' : c.couponType === 'FreeShipping' ? 'Free Ship' : '₹' + c.discountValue }}</td>
                <td>{{ c.minimumOrderAmount ? '₹' + c.minimumOrderAmount : '—' }}</td>
                <td>{{ c.usageCount }}{{ c.usageLimit ? '/' + c.usageLimit : '' }}</td>
                <td>{{ c.expiryDate ? (c.expiryDate | date:'mediumDate') : '—' }}</td>
                <td><span class="status-chip" [class.active]="c.isActive" [class.inactive]="!c.isActive">{{ c.isActive ? 'Active' : 'Inactive' }}</span></td>
                <td>
                  <div class="action-btns">
                    <button class="icon-btn" (click)="toggleActive(c.id)" title="Toggle">{{ c.isActive ? '⏸' : '▶' }}</button>
                    <button class="icon-btn" (click)="edit(c)" title="Edit">✏️</button>
                    <button class="icon-btn danger" (click)="delete(c.id)" title="Delete">🗑️</button>
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>
  `,
  styleUrl: '../products/admin-products.component.scss'
})
export class AdminCouponsComponent implements OnInit {
  private readonly api   = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly fb    = inject(FormBuilder);

  coupons  = signal<CouponAdminItem[]>([]);
  showForm = signal(false);
  saving   = signal(false);
  editId   = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    code:               ['', Validators.required],
    couponType:         ['Percentage', Validators.required],
    discountValue:      [0, [Validators.required, Validators.min(1)]],
    maxDiscountAmount:  [null as number | null],
    minimumOrderAmount: [null as number | null],
    usageLimit:         [null as number | null],
    isActive:           [true],
    expiryDate:         [''],
    description:        ['']
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.api.get<CouponAdminItem[]>('/admin/coupons').subscribe(c => this.coupons.set(c));
  }

  edit(c: CouponAdminItem): void {
    this.editId.set(c.id);
    this.form.patchValue({ ...c, expiryDate: c.expiryDate ? c.expiryDate.substring(0, 10) : '' } as any);
    this.showForm.set(true);
  }

  cancelForm(): void { this.showForm.set(false); this.editId.set(null); this.form.reset({ couponType: 'Percentage', isActive: true }); }

  save(): void {
    if (this.form.invalid || this.saving()) return;
    this.saving.set(true);
    const body = this.form.getRawValue();
    const req = this.editId()
      ? this.api.put<void>(`/admin/coupons/${this.editId()}`, body)
      : this.api.post<void>('/admin/coupons', body);
    req.subscribe({
      next: () => { this.toast.success('Coupon saved!'); this.cancelForm(); this.load(); this.saving.set(false); },
      error: () => this.saving.set(false)
    });
  }

  toggleActive(id: string): void {
    this.api.patch<void>(`/admin/coupons/${id}/toggle-active`).subscribe({
      next: () => { this.toast.success('Coupon status toggled.'); this.load(); }
    });
  }

  delete(id: string): void {
    this.api.delete<void>(`/admin/coupons/${id}`).subscribe({
      next: () => { this.toast.success('Coupon deleted.'); this.load(); }
    });
  }
}
