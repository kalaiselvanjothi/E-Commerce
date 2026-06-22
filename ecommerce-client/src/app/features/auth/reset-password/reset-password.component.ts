import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

function passwordMatch(c: AbstractControl) {
  const p = c.get('newPassword'), cp = c.get('confirmPassword');
  return p && cp && p.value !== cp.value ? { mismatch: true } : null;
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-center">
      <div class="simple-card">
        <a routerLink="/" class="auth-logo">
          <span class="logo-icon">SV</span><span class="logo-text">ShopVerse</span>
        </a>
        <h1 class="auth-title">Reset password</h1>
        <p class="auth-subtitle">Enter your new password below.</p>

        <form [formGroup]="form" (ngSubmit)="submit()" novalidate class="auth-form">
          <div class="form-group">
            <label>New Password</label>
            <input type="password" formControlName="newPassword" class="form-control"
              [class.is-invalid]="f.newPassword.invalid && f.newPassword.touched"
              placeholder="Min 8 chars, 1 uppercase, 1 number, 1 special" />
            @if (f.newPassword.invalid && f.newPassword.touched) {
              <span class="form-error">Password must be at least 8 characters with uppercase, number & special char.</span>
            }
          </div>
          <div class="form-group">
            <label>Confirm Password</label>
            <input type="password" formControlName="confirmPassword" class="form-control"
              [class.is-invalid]="form.errors?.['mismatch'] && f.confirmPassword.touched"
              placeholder="Repeat new password" />
            @if (form.errors?.['mismatch'] && f.confirmPassword.touched) {
              <span class="form-error">Passwords do not match.</span>
            }
          </div>
          <button type="submit" class="btn-primary btn-full" [disabled]="loading()">
            @if (loading()) { Resetting… } @else { Reset Password }
          </button>
        </form>
        <p class="auth-switch"><a routerLink="/auth/login">← Back to Sign In</a></p>
      </div>
    </div>
  `,
  styleUrl: '../forgot-password/forgot-password.component.scss'
})
export class ResetPasswordComponent {
  private readonly fb     = inject(FormBuilder);
  private readonly auth   = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route  = inject(ActivatedRoute);
  private readonly toast  = inject(ToastService);

  loading = signal(false);

  form = this.fb.nonNullable.group({
    newPassword:     ['', [Validators.required, Validators.minLength(8), Validators.pattern(/(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9])/)]],
    confirmPassword: ['', Validators.required]
  }, { validators: passwordMatch });

  get f() { return this.form.controls; }

  submit(): void {
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);
    const { newPassword, confirmPassword } = this.form.getRawValue();
    const email = this.route.snapshot.queryParams['email'] ?? '';
    const token = this.route.snapshot.queryParams['token'] ?? '';

    this.auth.resetPassword({ email, token, newPassword, confirmPassword }).subscribe({
      next: () => {
        this.toast.success('Password reset successfully. Please sign in.');
        this.router.navigate(['/auth/login']);
      },
      error: () => { this.loading.set(false); }
    });
  }
}
