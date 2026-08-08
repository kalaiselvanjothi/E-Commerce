import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { CartService } from '../../core/services/cart.service';
import { ToastService } from '../../core/services/toast.service';
import { Address, PaymentMethod, DeliveryOption } from '../../core/models/order.models';
import { Cart } from '../../core/models/cart.models';

declare const Razorpay: any;

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent implements OnInit {
  private readonly fb           = inject(FormBuilder);
  private readonly orderService = inject(OrderService);
  private readonly cartService  = inject(CartService);
  private readonly toast        = inject(ToastService);
  private readonly router       = inject(Router);

  step       = signal<1 | 2 | 3>(1);
  addresses  = signal<Address[]>([]);
  cart       = signal<Cart | null>(null);
  loading    = signal(true);
  placing    = signal(false);
  selectedAddressId = signal<string | null>(null);
  showNewAddress = signal(false);

  deliveryOption: DeliveryOption = 'Standard';
  paymentMethod: PaymentMethod  = 'Razorpay';

  readonly deliveryOptions = [
    { value: 'Standard' as DeliveryOption, label: 'Standard Delivery', subtitle: '5–7 business days', price: 99 },
    { value: 'Express'  as DeliveryOption, label: 'Express Delivery',  subtitle: '2–3 business days', price: 199 },
    { value: 'SameDay'  as DeliveryOption, label: 'Same Day Delivery', subtitle: 'Delivered today',   price: 399 },
  ];

  readonly paymentOptions = [
    { value: 'Razorpay' as PaymentMethod, label: 'Pay Online (Razorpay)', subtitle: 'Cards, UPI, Netbanking, Wallets' },
    { value: 'COD'      as PaymentMethod, label: 'Cash on Delivery', subtitle: 'Pay when your order arrives' },
  ];

  addressForm = this.fb.nonNullable.group({
    fullName:     ['', Validators.required],
    phone:        ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
    addressLine1: ['', Validators.required],
    addressLine2: [''],
    city:         ['', Validators.required],
    state:        ['', Validators.required],
    pincode:      ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    country:      ['India', Validators.required],
    addressType:  ['Home'],
    isDefault:    [false],
  });

  ngOnInit(): void {
    this.orderService.getAddresses().subscribe(a => {
      this.addresses.set(a);
      const def = a.find(x => x.isDefault) ?? a[0];
      if (def) this.selectedAddressId.set(def.id);
    });
    this.cartService.getCart().subscribe({
      next: c => { this.cart.set(c); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  saveAddress(): void {
    if (this.addressForm.invalid) return;
    this.orderService.addAddress(this.addressForm.getRawValue() as any).subscribe({
      next: a => {
        this.addresses.update(list => [...list, a]);
        this.selectedAddressId.set(a.id);
        this.showNewAddress.set(false);
        this.addressForm.reset({ country: 'India', addressType: 'Home' });
        this.toast.success('Address saved!');
      }
    });
  }

  placeOrder(): void {
    if (!this.selectedAddressId() || this.placing()) return;
    this.placing.set(true);

    this.orderService.createOrder({
      addressId:      this.selectedAddressId()!,
      paymentMethod:  this.paymentMethod,
      deliveryOption: this.deliveryOption,
      couponCode:     this.cart()?.couponCode
    }).subscribe({
      next: order => {
        if (this.paymentMethod === 'COD') {
          this.cartService.clearLocal();
          this.toast.success('Order placed successfully!');
          this.router.navigate(['/account/orders', order.id]);
          return;
        }
        // Razorpay flow
        this.orderService.createRazorpayOrder(order.id).subscribe({
          next: rzp => {
            const options = {
              key: rzp.keyId,
              amount: rzp.amount,
              currency: rzp.currency,
              name: 'ShopVerse',
              description: `Order ${order.orderNumber}`,
              order_id: rzp.razorpayOrderId,
              handler: (response: any) => {
                this.orderService.verifyPayment({
                  orderId: order.id,
                  razorpayOrderId: response.razorpay_order_id,
                  razorpayPaymentId: response.razorpay_payment_id,
                  razorpaySignature: response.razorpay_signature
                }).subscribe({
                  next: () => {
                    this.cartService.clearLocal();
                    this.toast.success('Payment successful! Order confirmed.');
                    this.router.navigate(['/account/orders', order.id]);
                  },
                  error: err => {
                    this.placing.set(false);
                    this.toast.error(err?.error?.message || 'Payment verification failed.');
                  }
                });
              },
              modal: { ondismiss: () => this.placing.set(false) }
            };
            const rzpInstance = new Razorpay(options);
            rzpInstance.open();
          },
          error: err => {
            this.placing.set(false);
            this.toast.error(err?.error?.message || 'Failed to initiate Razorpay payment. Please try Cash on Delivery or check Razorpay API keys.');
          }
        });
      },
      error: err => {
        this.placing.set(false);
        this.toast.error(err?.error?.message || 'Failed to place order. Please check your cart or address.');
      }
    });
  }

  get shippingCost(): number {
    return this.deliveryOptions.find(o => o.value === this.deliveryOption)?.price ?? 99;
  }

  get f() { return this.addressForm.controls; }
}
