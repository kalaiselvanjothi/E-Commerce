import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Order, OrderSummary, CreateOrderRequest, CancelOrderRequest,
  ReturnOrderRequest, RazorpayOrderResponse, PaymentVerifyRequest
} from '../models/order.models';
import { Address } from '../models/order.models';
import { PagedResult } from '../models/product.models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly api = inject(ApiService);

  createOrder(dto: CreateOrderRequest): Observable<Order> {
    return this.api.post<Order>('/orders', dto);
  }

  getMyOrders(page = 1, pageSize = 10): Observable<PagedResult<OrderSummary>> {
    return this.api.get<PagedResult<OrderSummary>>('/orders/my', { page, pageSize });
  }

  getOrderById(id: string): Observable<Order> {
    return this.api.get<Order>(`/orders/${id}`);
  }

  cancelOrder(id: string, dto: CancelOrderRequest): Observable<void> {
    return this.api.post<void>(`/orders/${id}/cancel`, dto);
  }

  returnOrder(id: string, dto: ReturnOrderRequest): Observable<void> {
    return this.api.post<void>(`/orders/${id}/return`, dto);
  }

  createRazorpayOrder(orderId: string): Observable<RazorpayOrderResponse> {
    return this.api.post<RazorpayOrderResponse>('/payments/razorpay/create', { orderId });
  }

  verifyPayment(dto: PaymentVerifyRequest): Observable<void> {
    return this.api.post<void>('/payments/razorpay/verify', dto);
  }

  getAddresses(): Observable<Address[]> {
    return this.api.get<Address[]>('/addresses');
  }

  addAddress(dto: Omit<Address, 'id'>): Observable<Address> {
    return this.api.post<Address>('/addresses', dto);
  }

  updateAddress(id: string, dto: Omit<Address, 'id'>): Observable<Address> {
    return this.api.put<Address>(`/addresses/${id}`, dto);
  }

  deleteAddress(id: string): Observable<void> {
    return this.api.delete<void>(`/addresses/${id}`);
  }

  setDefaultAddress(id: string): Observable<void> {
    return this.api.patch<void>(`/addresses/${id}/set-default`);
  }
}
