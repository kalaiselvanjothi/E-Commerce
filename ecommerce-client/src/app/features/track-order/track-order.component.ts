import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { ToastService } from '../../core/services/toast.service';

export interface OrderTrackResult {
  orderId: string;
  orderDate: string;
  customerName: string;
  email: string;
  status: string;
  paymentStatus?: string;
  currentStep: number;
  estimatedDeliveryDate: string;
  courierName: string;
  trackingNumber: string;
  shippingAddress: string;
  items: Array<{ name: string; quantity: number; price: number; image: string }>;
  timeline: Array<{ step: string; timestamp: string; location: string; completed: boolean; current?: boolean }>;
}

@Component({
  selector: 'app-track-order',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './track-order.component.html',
  styleUrl: './track-order.component.scss'
})
export class TrackOrderComponent implements OnInit {
  private readonly api   = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);

  orderNumber = signal('ORD-89412');
  email       = signal('');
  isLoading   = signal(false);
  hasSearched = signal(false);
  errorMessage = signal('');
  orderResult  = signal<OrderTrackResult | null>(null);

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const num = params['orderNumber'] || params['orderId'] || params['id'];
      if (num) {
        this.orderNumber.set(num);
        this.trackOrder();
      }
    });
  }

  steps = [
    { id: 1, label: 'Order Placed', icon: 'M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z' },
    { id: 2, label: 'Payment Confirmed', icon: 'M22 11.08V12a10 10 0 1 1-5.93-9.14' },
    { id: 3, label: 'Packed', icon: 'M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z' },
    { id: 4, label: 'Shipped', icon: 'M1 3h15v13H1z' },
    { id: 5, label: 'Out for Delivery', icon: 'M13 2 3 14h9l-1 8 10-12h-9l1-8z' },
    { id: 6, label: 'Delivered', icon: 'M22 11.08V12a10 10 0 1 1-5.93-9.14' }
  ];

  trackOrder(): void {
    const id = this.orderNumber().trim();
    if (!id) {
      this.toast.error('Please enter a valid Order Number.');
      return;
    }

    this.isLoading.set(true);
    this.hasSearched.set(true);
    this.errorMessage.set('');

    this.api.get<OrderTrackResult>('/orders/track', { orderId: id, email: this.email() }).subscribe({
      next: (data) => {
        this.isLoading.set(false);
        if (data) {
          this.orderResult.set(data);
        } else {
          this.orderResult.set(null);
          this.errorMessage.set(`No order found matching "${id}". Please check your order number and try again.`);
        }
      },
      error: () => {
        this.isLoading.set(false);
        this.orderResult.set(null);
        this.errorMessage.set(`No order found matching "${id}". Please check your order number and try again.`);
      }
    });
  }

  useSampleOrder(sampleId: string): void {
    this.orderNumber.set(sampleId);
    this.trackOrder();
  }
}
