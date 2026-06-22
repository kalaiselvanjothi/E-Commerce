import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-center">
      <div class="simple-card">
        <a routerLink="/" class="auth-logo">
          <span class="logo-icon">SV</span>
          <span class="logo-text">ShopVerse</span>
        </a>

        @if (!sent()) {
          <h1 class="auth-title">Forgot password?</h1>
          <p class="auth-subtitle">Enter your email and we'll send you a reset link.</p>

          <form [formGroup]="form" (ngSubmit)="submit()" novalidate class="auth-form">
            <div class="form-group">
              <label for="email">Email address</label>
              <input id="email" type="email" formControlName="email"
                class="form-control" [class.is-invalid]="email.invalid && email.touched"
                placeholder="you@example.com" />
              @if (email.invalid && email.touched) {
                <span class="form-error">Enter a valid email.</span>
              }
            </div>
            <button type="submit" class="btn-primary btn-full" [disabled]="loading()">
              @if (loading()) { Sending… } @else { Send Reset Link }
            </button>
          </form>
        } @else {
          <div class="success-state">
            <div class="success-icon">✉️</div>
            <h2>Check your email</h2>
            <p>We sent a password reset link to <strong>{{ form.value.email }}</strong></p>
            <p class="hint">In development mode, the reset token is logged to the API console.</p>
          </div>
        }

        <p class="auth-switch"><a routerLink="/auth/login">← Back to Sign In</a></p>
      </div>
    </div>
  `,
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  private readonly fb   = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  loading = signal(false);
  sent    = signal(false);

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]]
  });

  submit(): void {
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);
    this.auth.forgotPassword(this.form.getRawValue()).subscribe({
      next: () => { this.sent.set(true); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  get email() { return this.form.controls.email; }
}
