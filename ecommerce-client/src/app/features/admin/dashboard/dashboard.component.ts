import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { DashboardStats } from '../../../core/models/admin.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly api = inject(ApiService);
  stats   = signal<DashboardStats | null>(null);
  loading = signal(true);

  maxRevenue = computed(() => {
    const pts = this.stats()?.revenueChart ?? [];
    return Math.max(1, ...pts.map(p => p.value));
  });

  maxOrders = computed(() => {
    const pts = this.stats()?.ordersChart ?? [];
    return Math.max(1, ...pts.map(p => p.value));
  });

  ngOnInit(): void {
    this.api.get<DashboardStats>('/admin/dashboard').subscribe({
      next: s => { this.stats.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  change(val: number): string {
    return val >= 0 ? `+${val.toFixed(1)}%` : `${val.toFixed(1)}%`;
  }
}
