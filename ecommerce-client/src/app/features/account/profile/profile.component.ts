import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="account-page container">
      <h1 class="account-title">My Profile</h1>
      <div class="profile-grid">
        <!-- Profile Card -->
        <div class="profile-card">
          <div class="avatar-section">
            <div class="avatar-circle">
              {{ auth.user()!.firstName[0] }}{{ auth.user()!.lastName[0] }}
            </div>
            <div>
              <div class="user-fullname">{{ auth.user()!.fullName }}</div>
              <div class="user-email">{{ auth.user()!.email }}</div>
              <div class="user-role">{{ auth.user()!.roles[0] }}</div>
            </div>
          </div>

          <nav class="profile-nav">
            <a routerLink="/account/orders"    class="pnav-link">📦 My Orders</a>
            <a routerLink="/account/addresses" class="pnav-link">📍 Addresses</a>
            <a routerLink="/wishlist"           class="pnav-link">❤️ Wishlist</a>
          </nav>
        </div>

        <!-- Edit Form -->
        <div class="edit-card">
          <h2>Edit Profile</h2>
          <form [formGroup]="form" (ngSubmit)="save()" novalidate>
            <div class="form-row">
              <div class="form-group">
                <label>First Name</label>
                <input type="text" formControlName="firstName" class="form-control" [class.is-invalid]="f.firstName.invalid && f.firstName.touched" />
                @if (f.firstName.invalid && f.firstName.touched) { <span class="form-error">Required.</span> }
              </div>
              <div class="form-group">
                <label>Last Name</label>
                <input type="text" formControlName="lastName" class="form-control" [class.is-invalid]="f.lastName.invalid && f.lastName.touched" />
                @if (f.lastName.invalid && f.lastName.touched) { <span class="form-error">Required.</span> }
              </div>
            </div>
            <div class="form-group">
              <label>Avatar URL (optional)</label>
              <input type="url" formControlName="avatarUrl" class="form-control" placeholder="https://..." />
            </div>
            <button type="submit" class="btn-primary" [disabled]="saving()">
              @if (saving()) { Saving… } @else { Save Changes }
            </button>
          </form>

          <hr class="divider" />

          <h2>Change Password</h2>
          <form [formGroup]="pwdForm" (ngSubmit)="changePassword()" novalidate>
            <div class="form-group">
              <label>Current Password</label>
              <input type="password" formControlName="currentPassword" class="form-control" />
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>New Password</label>
                <input type="password" formControlName="newPassword" class="form-control" [class.is-invalid]="pf.newPassword.invalid && pf.newPassword.touched" />
              </div>
              <div class="form-group">
                <label>Confirm</label>
                <input type="password" formControlName="confirmPassword" class="form-control" />
              </div>
            </div>
            <button type="submit" class="btn-secondary" [disabled]="changingPwd()">
              @if (changingPwd()) { Updating… } @else { Change Password }
            </button>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .account-page { padding: var(--space-8) 0 var(--space-12); }
    .account-title { font-size: var(--text-2xl); font-weight: var(--font-extrabold); margin-bottom: var(--space-6); }
    .profile-grid { display: grid; grid-template-columns: 1fr; gap: var(--space-6); }
    @media (min-width: 1024px) { .profile-grid { grid-template-columns: 280px 1fr; } }
    .profile-card, .edit-card { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); padding: var(--space-6); }
    .avatar-section { display: flex; align-items: center; gap: var(--space-4); margin-bottom: var(--space-6); }
    .avatar-circle { width: 64px; height: 64px; border-radius: 50%; background: var(--color-primary); color: #fff; font-weight: 800; font-size: 22px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; text-transform: uppercase; }
    .user-fullname { font-weight: var(--font-bold); font-size: var(--text-base); }
    .user-email { font-size: var(--text-sm); color: var(--color-text-muted); }
    .user-role { font-size: var(--text-xs); background: var(--color-primary-light); color: var(--color-primary); padding: 2px 8px; border-radius: var(--radius-full); display: inline-block; margin-top: 4px; font-weight: 600; }
    .profile-nav { display: flex; flex-direction: column; gap: var(--space-1); }
    .pnav-link { font-size: var(--text-sm); color: var(--color-text-secondary); text-decoration: none; padding: var(--space-3); border-radius: var(--radius-md); transition: all 150ms; &:hover { background: var(--color-surface-2); color: var(--color-text-primary); } }
    .edit-card h2 { font-size: var(--text-lg); font-weight: var(--font-bold); margin-bottom: var(--space-5); }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-4); }
    .divider { border: none; border-top: 1px solid var(--color-border); margin: var(--space-6) 0; }
  `]
})
export class ProfileComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly fb    = inject(FormBuilder);
  private readonly toast = inject(ToastService);

  saving     = signal(false);
  changingPwd = signal(false);

  form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    lastName:  ['', Validators.required],
    avatarUrl: ['']
  });

  pwdForm = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword:     ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  });

  get f()  { return this.form.controls; }
  get pf() { return this.pwdForm.controls; }

  ngOnInit(): void {
    const u = this.auth.user()!;
    this.form.patchValue({ firstName: u.firstName, lastName: u.lastName, avatarUrl: u.avatarUrl ?? '' });
  }

  save(): void {
    if (this.form.invalid || this.saving()) return;
    this.saving.set(true);
    this.auth.updateProfile(this.form.getRawValue()).subscribe({
      next: () => { this.toast.success('Profile updated!'); this.saving.set(false); },
      error: () => this.saving.set(false)
    });
  }

  changePassword(): void {
    if (this.pwdForm.invalid || this.changingPwd()) return;
    this.changingPwd.set(true);
    this.auth.changePassword(this.pwdForm.getRawValue()).subscribe({
      next: () => { this.toast.success('Password changed!'); this.pwdForm.reset(); this.changingPwd.set(false); },
      error: () => this.changingPwd.set(false)
    });
  }
}
