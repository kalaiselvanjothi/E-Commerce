import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { ToastService } from '../../../core/services/toast.service';
import { Category } from '../../../core/models/product.models';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="admin-page">
      <div class="page-header">
        <h1 class="page-title">Categories</h1>
        <button class="btn-primary" (click)="showForm.set(!showForm())">+ Add Category</button>
      </div>

      @if (showForm()) {
        <div class="form-card">
          <h3>{{ editId() ? 'Edit' : 'New' }} Category</h3>
          <form [formGroup]="form" (ngSubmit)="save()" novalidate>
            <div class="form-row">
              <div class="form-group"><label>Name</label><input type="text" formControlName="name" class="form-control" placeholder="Category name" /></div>
              <div class="form-group"><label>Parent</label>
                <select formControlName="parentId" class="form-control">
                  <option [value]="null">None (Top-level)</option>
                  @for (c of categories(); track c.id) { <option [value]="c.id">{{ c.name }}</option> }
                </select>
              </div>
            </div>
            <div class="form-group"><label>Description</label><input type="text" formControlName="description" class="form-control" placeholder="Optional description" /></div>
            <div class="form-group"><label>Image URL</label><input type="url" formControlName="imageUrl" class="form-control" placeholder="https://…" /></div>
            <div class="form-row">
              <div class="form-group"><label>Sort Order</label><input type="number" formControlName="sortOrder" class="form-control" /></div>
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
          <thead><tr><th>Name</th><th>Parent</th><th>Products</th><th>Sort</th><th>Status</th><th>Actions</th></tr></thead>
          <tbody>
            @for (c of flatCategories(); track c.id) {
              <tr>
                <td><div class="cat-cell"><img [src]="c.imageUrl || '/assets/placeholder.svg'" class="cat-thumb" [alt]="c.name" /> <span [style.padding-left]="c.parentId ? '12px' : '0'">{{ c.parentId ? '↳ ' : '' }}{{ c.name }}</span></div></td>
                <td>{{ c.parentId ? 'Sub' : '—' }}</td>
                <td>{{ c.productCount }}</td>
                <td>{{ c.sortOrder }}</td>
                <td><span class="status-chip" [class.active]="c.isActive" [class.inactive]="!c.isActive">{{ c.isActive ? 'Active' : 'Inactive' }}</span></td>
                <td>
                  <div class="action-btns">
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
export class AdminCategoriesComponent implements OnInit {
  private readonly api   = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly fb    = inject(FormBuilder);

  categories     = signal<Category[]>([]);
  flatCategories = signal<Category[]>([]);
  loading        = signal(true);
  showForm       = signal(false);
  saving         = signal(false);
  editId         = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    name:        ['', Validators.required],
    description: [''],
    imageUrl:    [''],
    parentId:    [null as string | null],
    sortOrder:   [0],
    isActive:    [true]
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.api.get<Category[]>('/categories').subscribe(cats => {
      const topLevel = cats.filter(c => !c.parentId);
      this.categories.set(topLevel);
      const flat: Category[] = [];
      topLevel.forEach(c => { flat.push(c); c.children?.forEach(ch => flat.push(ch)); });
      this.flatCategories.set(flat);
      this.loading.set(false);
    });
  }

  edit(c: Category): void {
    this.editId.set(c.id);
    this.form.patchValue({ name: c.name, description: c.description ?? '', imageUrl: c.imageUrl ?? '', parentId: c.parentId ?? null, sortOrder: c.sortOrder, isActive: c.isActive });
    this.showForm.set(true);
  }

  cancelForm(): void { this.showForm.set(false); this.editId.set(null); this.form.reset({ isActive: true }); }

  save(): void {
    if (this.form.invalid || this.saving()) return;
    this.saving.set(true);
    const body = this.form.getRawValue();
    const req = this.editId()
      ? this.api.put<void>(`/admin/categories/${this.editId()}`, body)
      : this.api.post<void>('/admin/categories', body);
    req.subscribe({
      next: () => { this.toast.success(this.editId() ? 'Category updated!' : 'Category created!'); this.cancelForm(); this.load(); this.saving.set(false); },
      error: () => this.saving.set(false)
    });
  }

  delete(id: string): void {
    this.api.delete<void>(`/admin/categories/${id}`).subscribe({
      next: () => { this.toast.success('Category deleted.'); this.load(); }
    });
  }
}
