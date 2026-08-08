import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink, FormsModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss'
})
export class FooterComponent {
  private readonly toast = inject(ToastService);
  private readonly api   = inject(ApiService);
  readonly year = new Date().getFullYear();

  newsletterEmail = signal('');
  isSubmitting    = signal(false);

  subscribeNewsletter(): void {
    const email = this.newsletterEmail().trim();
    if (!email || !email.includes('@')) {
      this.toast.error('Please enter a valid email address.');
      return;
    }

    if (this.isSubmitting()) return;
    this.isSubmitting.set(true);

    this.api.post<{ success: boolean; message: string }>('/newsletter/subscribe', { email }).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        const msg = res?.message || 'Thank you for subscribing to ShopVerse newsletter!';
        this.toast.success(msg);
        this.newsletterEmail.set('');
      },
      error: (err) => {
        this.isSubmitting.set(false);
        const msg = err?.error?.message || 'Subscription failed. Please check your email address.';
        this.toast.error(msg);
      }
    });
  }
}
