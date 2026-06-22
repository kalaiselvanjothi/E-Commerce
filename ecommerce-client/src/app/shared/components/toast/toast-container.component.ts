import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  animations: [
    trigger('toast', [
      transition(':enter', [
        style({ transform: 'translateX(110%)', opacity: 0 }),
        animate('250ms ease', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease', style({ transform: 'translateX(110%)', opacity: 0 }))
      ])
    ])
  ],
  template: `
    <div class="toast-container">
      @for (t of toastService.toasts(); track t.id) {
        <div class="toast toast-{{ t.type }}" [@toast] (click)="toastService.dismiss(t.id)">
          <span class="toast-icon">
            @switch (t.type) {
              @case ('success') { ✓ }
              @case ('error') { ✕ }
              @case ('warning') { ⚠ }
              @default { ℹ }
            }
          </span>
          <span class="toast-message">{{ t.message }}</span>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed; bottom: 24px; right: 24px;
      display: flex; flex-direction: column; gap: 10px;
      z-index: var(--z-toast); max-width: 360px;
    }
    .toast {
      display: flex; align-items: center; gap: 10px;
      padding: 12px 16px; border-radius: var(--radius-md);
      box-shadow: var(--shadow-lg); cursor: pointer;
      font-size: var(--text-sm); font-weight: var(--font-medium);
      border-left: 4px solid transparent;
    }
    .toast-success { background: #ECFDF5; color: #065F46; border-color: #10B981; }
    .toast-error   { background: #FEF2F2; color: #991B1B; border-color: #EF4444; }
    .toast-warning { background: #FFFBEB; color: #92400E; border-color: #F59E0B; }
    .toast-info    { background: #EFF6FF; color: #1E40AF; border-color: #3B82F6; }
    .toast-icon { font-size: 16px; flex-shrink: 0; }
    .toast-message { flex: 1; }
  `]
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);
}
