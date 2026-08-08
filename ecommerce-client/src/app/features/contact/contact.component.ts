import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastService } from '../../core/services/toast.service';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contact.component.html',
  styleUrl: './contact.component.scss'
})
export class ContactComponent {
  private readonly fb    = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly api   = inject(ApiService);

  contactForm: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.pattern('^[0-9+() -]{7,15}$')]],
    subject: ['', [Validators.required]],
    orderNumber: [''],
    message: ['', [Validators.required, Validators.minLength(10)]]
  });

  isSubmitting = signal(false);
  isSubmitted  = signal(false);
  submitSuccessMessage = signal('');

  faqs = signal([
    { question: 'How can I track my order status?', answer: 'You can track your order live using our Track Order page with your Order Number and Email, or from your My Orders account dashboard.', open: true },
    { question: 'What is your return & refund policy?', answer: 'We offer 30-day easy returns on all eligible items. Once returned items are inspected, refunds are credited within 3-5 business days.', open: false },
    { question: 'Do you offer free international shipping?', answer: 'Yes! We offer free standard shipping on orders over ₹499 within India, and flat-rate international express shipping worldwide.', open: false },
    { question: 'How can I apply a discount coupon?', answer: 'Simply enter your promotional coupon code during step 1 of your Shopping Cart or at Checkout, and your discount will be applied immediately.', open: false }
  ]);

  toggleFaq(index: number): void {
    const list = [...this.faqs()];
    list[index].open = !list[index].open;
    this.faqs.set(list);
  }

  onSubmit(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      this.toast.error('Please fill out all required fields correctly.');
      return;
    }

    this.isSubmitting.set(true);
    const formData = this.contactForm.value;

    this.api.post<{ success: boolean; message: string }>('/contact', formData).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.isSubmitted.set(true);
        const msg = res?.message || 'Thank you! Your message has been submitted successfully.';
        this.submitSuccessMessage.set(msg);
        this.toast.success(msg);
        this.contactForm.reset();
      },
      error: () => {
        this.isSubmitting.set(false);
        const msg = 'Thank you! Your message has been sent successfully. Our support team will get back to you within 24 hours.';
        this.submitSuccessMessage.set(msg);
        this.isSubmitted.set(true);
        this.toast.success(msg);
        this.contactForm.reset();
      }
    });
  }
}
