import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

function passwordMatchValidator(control: AbstractControl) {
  const password = control.get('password');
  const confirm  = control.get('confirmPassword');
  if (!password || !confirm) return null;
  return password.value !== confirm.value ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly fb     = inject(FormBuilder);
  private readonly auth   = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast  = inject(ToastService);

  loading        = signal(false);
  showPassword   = signal(false);
  acceptedTerms  = signal(false);

  form = this.fb.nonNullable.group({
    firstName:       ['', [Validators.required, Validators.minLength(2)]],
    lastName:        ['', [Validators.required, Validators.minLength(2)]],
    email:           ['', [Validators.required, Validators.email]],
    password:        ['', [Validators.required, Validators.minLength(8), Validators.pattern(/(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9])/)]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: passwordMatchValidator });

  submit(): void {
    if (this.form.invalid || this.loading()) return;
    this.loading.set(true);

    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => {
        this.toast.success('Account created! Welcome to ShopVerse.');
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err?.error?.message || 'Registration failed. Please try again.';
        this.toast.error(msg);
      }
    });
  }

  get f()               { return this.form.controls; }
  get passwordMismatch(){ return this.form.errors?.['passwordMismatch'] && this.f.confirmPassword.touched; }

  get passwordStrength(): 0 | 1 | 2 | 3 {
    const pw = this.f.password.value;
    if (!pw || pw.length < 4) return 0;
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw) && /[0-9]/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    return score as 0 | 1 | 2 | 3;
  }

  get strengthLabel(): string {
    return ['', 'Weak', 'Medium', 'Strong'][this.passwordStrength] || '';
  }
}
