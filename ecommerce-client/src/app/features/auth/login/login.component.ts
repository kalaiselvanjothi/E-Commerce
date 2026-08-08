import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb      = inject(FormBuilder);
  private readonly auth    = inject(AuthService);
  private readonly router  = inject(Router);
  private readonly route   = inject(ActivatedRoute);
  private readonly toast   = inject(ToastService);

  loading      = signal(false);
  showPassword = signal(false);

  form = this.fb.nonNullable.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  submit(): void {
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.toast.success('Welcome back!');
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.message || 'Invalid email or password.';
        this.toast.error(msg);
      }
    });
  }

  loginGoogle(idToken: string): void {
    if (this.loading()) return;
    this.loading.set(true);
    this.auth.googleLogin(idToken).subscribe({
      next: () => {
        this.toast.success('Google login successful!');
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err?.error?.message || 'Google authentication failed.');
      }
    });
  }

  loginFacebook(accessToken: string): void {
    if (this.loading()) return;
    this.loading.set(true);
    this.auth.facebookLogin(accessToken).subscribe({
      next: () => {
        this.toast.success('Facebook login successful!');
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err?.error?.message || 'Facebook authentication failed.');
      }
    });
  }

  get email()    { return this.form.controls.email; }
  get password() { return this.form.controls.password; }
}
