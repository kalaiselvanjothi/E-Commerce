import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';
import { Order } from '../../../core/models/order.models';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.scss'
})
export class OrderDetailComponent implements OnInit {
  private readonly route        = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);
  private readonly toast        = inject(ToastService);

  order   = signal<Order | null>(null);
  loading = signal(true);
  cancelReason = '';
  returnReason = '';
  showCancel = signal(false);
  showReturn = signal(false);

  ngOnInit(): void {
    this.route.params.subscribe(p => {
      this.orderService.getOrderById(p['id']).subscribe({
        next: o => { this.order.set(o); this.loading.set(false); },
        error: () => this.loading.set(false)
      });
    });
  }

  cancel(): void {
    if (!this.cancelReason.trim()) return;
    this.orderService.cancelOrder(this.order()!.id, { reason: this.cancelReason }).subscribe({
      next: () => {
        this.toast.success('Order cancelled.');
        this.orderService.getOrderById(this.order()!.id).subscribe(o => this.order.set(o));
        this.showCancel.set(false);
      }
    });
  }

  returnOrder(): void {
    if (!this.returnReason.trim()) return;
    this.orderService.returnOrder(this.order()!.id, { reason: this.returnReason }).subscribe({
      next: () => {
        this.toast.success('Return request submitted.');
        this.orderService.getOrderById(this.order()!.id).subscribe(o => this.order.set(o));
        this.showReturn.set(false);
      }
    });
  }

  downloadInvoice(): void {
    const o = this.order();
    if (!o) return;

    this.toast.info('Generating PDF tax invoice...');
    this.orderService.downloadInvoice(o.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `ShopVerse-Invoice-${o.orderNumber}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.toast.success('Invoice downloaded successfully!');
      },
      error: () => {
        this.toast.error('Failed to download PDF invoice. Please try again.');
      }
    });
  }

  get canCancel(): boolean {
    return ['Placed','Confirmed','Packed'].includes(this.order()?.status ?? '');
  }
  get canReturn(): boolean {
    return this.order()?.status === 'Delivered';
  }
}
