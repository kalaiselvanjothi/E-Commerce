import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';
import { Address } from '../../../core/models/order.models';

@Component({
  selector: 'app-addresses',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="account-page container">
      <h1 class="account-title">Saved Addresses</h1>

      <div class="addr-grid">
        @for (addr of addresses(); track addr.id) {
          <div class="addr-card" [class.default]="addr.isDefault">
            @if (addr.isDefault) { <span class="default-badge">Default</span> }
            <div class="addr-type-tag">{{ addr.addressType }}</div>
            <div class="addr-name">{{ addr.fullName }}</div>
            <div class="addr-text">{{ addr.addressLine1 }}@if (addr.addressLine2) {, {{ addr.addressLine2 }}}</div>
            <div class="addr-text">{{ addr.city }}, {{ addr.state }} - {{ addr.pincode }}</div>
            <div class="addr-phone">📞 {{ addr.phone }}</div>
            <div class="addr-actions">
              @if (!addr.isDefault) {
                <button class="btn-ghost btn-sm" (click)="setDefault(addr.id)">Set Default</button>
              }
              <button class="btn-ghost btn-sm danger" (click)="delete(addr.id)">Delete</button>
            </div>
          </div>
        }

        <!-- Add new -->
        <div class="add-card" (click)="showForm.set(!showForm())">
          <span class="add-icon">+</span>
          <span>Add New Address</span>
        </div>
      </div>

      @if (showForm()) {
        <div class="add-form-wrap">
          <h2>New Address</h2>
          <form [formGroup]="form" (ngSubmit)="save()" novalidate class="add-form">
            <div class="form-row">
              <div class="form-group"><label>Full Name</label><input type="text" formControlName="fullName" class="form-control" placeholder="Full Name" /></div>
              <div class="form-group"><label>Phone</label><input type="tel" formControlName="phone" class="form-control" placeholder="10-digit number" /></div>
            </div>
            <div class="form-group"><label>Address Line 1</label><input type="text" formControlName="addressLine1" class="form-control" placeholder="House/Flat No., Street" /></div>
            <div class="form-group"><label>Address Line 2</label><input type="text" formControlName="addressLine2" class="form-control" placeholder="Area, Landmark (optional)" /></div>
            <div class="form-row">
              <div class="form-group"><label>City</label><input type="text" formControlName="city" class="form-control" /></div>
              <div class="form-group"><label>State</label><input type="text" formControlName="state" class="form-control" /></div>
              <div class="form-group"><label>Pincode</label><input type="text" formControlName="pincode" class="form-control" /></div>
            </div>
            <div class="form-row">
              <div class="form-group"><label>Type</label><select formControlName="addressType" class="form-control"><option>Home</option><option>Work</option><option>Other</option></select></div>
              <div class="form-group" style="display:flex;align-items:flex-end;padding-bottom:4px"><label style="display:flex;gap:8px;align-items:center;cursor:pointer"><input type="checkbox" formControlName="isDefault" style="accent-color:var(--color-primary)" /> Set as default</label></div>
            </div>
            <div class="form-btns">
              <button type="button" class="btn-ghost" (click)="showForm.set(false)">Cancel</button>
              <button type="submit" class="btn-primary" [disabled]="form.invalid || saving()">
                @if (saving()) { Saving… } @else { Save Address }
              </button>
            </div>
          </form>
        </div>
      }
    </div>
  `,
  styles: [`
    .account-page { padding: var(--space-8) 0 var(--space-12); }
    .account-title { font-size: var(--text-2xl); font-weight: var(--font-extrabold); margin-bottom: var(--space-6); }
    .addr-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: var(--space-4); margin-bottom: var(--space-6); }
    .addr-card { background: var(--color-surface); border: 1.5px solid var(--color-border); border-radius: var(--radius-xl); padding: var(--space-5); position: relative; display: flex; flex-direction: column; gap: 4px; &.default { border-color: var(--color-primary); } }
    .default-badge { position: absolute; top: var(--space-3); right: var(--space-3); background: var(--color-primary); color: #fff; font-size: 10px; font-weight: 700; padding: 2px 8px; border-radius: var(--radius-full); }
    .addr-type-tag { font-size: var(--text-xs); color: var(--color-text-muted); font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 4px; }
    .addr-name { font-weight: var(--font-semibold); font-size: var(--text-sm); }
    .addr-text { font-size: var(--text-sm); color: var(--color-text-secondary); }
    .addr-phone { font-size: var(--text-xs); color: var(--color-text-muted); margin-top: 4px; }
    .addr-actions { display: flex; gap: var(--space-2); margin-top: var(--space-3); padding-top: var(--space-3); border-top: 1px solid var(--color-border); }
    .danger { color: var(--color-error) !important; &:hover { background: var(--color-error-light) !important; } }
    .add-card { border: 2px dashed var(--color-border); border-radius: var(--radius-xl); padding: var(--space-8); display: flex; flex-direction: column; align-items: center; justify-content: center; gap: var(--space-2); cursor: pointer; color: var(--color-text-muted); font-size: var(--text-sm); transition: all 150ms; &:hover { border-color: var(--color-primary); color: var(--color-primary); background: var(--color-primary-light); } }
    .add-icon { font-size: 28px; font-weight: 300; }
    .add-form-wrap { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); padding: var(--space-6); h2 { font-size: var(--text-lg); font-weight: var(--font-bold); margin-bottom: var(--space-5); } }
    .add-form { display: flex; flex-direction: column; gap: var(--space-2); }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-4); }
    .form-btns { display: flex; gap: var(--space-3); justify-content: flex-end; margin-top: var(--space-3); }
  `]
})
export class AddressesComponent implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly toast        = inject(ToastService);
  private readonly fb           = inject(FormBuilder);

  addresses = signal<Address[]>([]);
  showForm  = signal(false);
  saving    = signal(false);

  form = this.fb.nonNullable.group({
    fullName:     ['', Validators.required],
    phone:        ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    addressLine1: ['', Validators.required],
    addressLine2: [''],
    city:         ['', Validators.required],
    state:        ['', Validators.required],
    pincode:      ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    country:      ['India'],
    addressType:  ['Home'],
    isDefault:    [false]
  });

  ngOnInit(): void {
    this.orderService.getAddresses().subscribe(a => this.addresses.set(a));
  }

  save(): void {
    if (this.form.invalid || this.saving()) return;
    this.saving.set(true);
    this.orderService.addAddress(this.form.getRawValue() as any).subscribe({
      next: a => {
        this.addresses.update(list => [...list, a]);
        this.form.reset({ country: 'India', addressType: 'Home' });
        this.showForm.set(false);
        this.toast.success('Address added!');
        this.saving.set(false);
      },
      error: () => this.saving.set(false)
    });
  }

  setDefault(id: string): void {
    this.orderService.setDefaultAddress(id).subscribe({
      next: () => {
        this.addresses.update(list => list.map(a => ({ ...a, isDefault: a.id === id })));
        this.toast.success('Default address updated.');
      }
    });
  }

  delete(id: string): void {
    this.orderService.deleteAddress(id).subscribe({
      next: () => {
        this.addresses.update(list => list.filter(a => a.id !== id));
        this.toast.success('Address removed.');
      }
    });
  }
}
