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
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly fb    = inject(FormBuilder);
  private readonly toast = inject(ToastService);

  saving      = signal(false);
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
      next: () => { this.toast.success('Profile updated!'); this.saving.set(false); this.form.markAsPristine(); },
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
