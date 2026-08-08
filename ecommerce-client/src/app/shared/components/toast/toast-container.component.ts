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
        style({ transform: 'translateX(120%)', opacity: 0 }),
        animate('280ms cubic-bezier(0.34,1.56,0.64,1)', style({ transform: 'translateX(0)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ transform: 'translateX(120%)', opacity: 0 }))
      ])
    ])
  ],
  template: `
    <div class="toast-container" role="region" aria-label="Notifications" aria-live="polite">
      @for (t of toastService.toasts(); track t.id) {
        <div class="toast toast-{{ t.type }}" [@toast] role="alert">
          <span class="toast-icon">
            @switch (t.type) {
              @case ('success') {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22,4 12,14.01 9,11.01"/>
                </svg>
              }
              @case ('error') {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/>
                </svg>
              }
              @case ('warning') {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/>
                  <line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>
                </svg>
              }
              @default {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/>
                  <line x1="12" y1="16" x2="12.01" y2="16"/>
                </svg>
              }
            }
          </span>
          <span class="toast-message">{{ t.message }}</span>
          <button
            class="toast-close"
            (click)="toastService.dismiss(t.id)"
            aria-label="Dismiss notification"
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed; bottom: 24px; right: 24px;
      display: flex; flex-direction: column; gap: 10px;
      z-index: var(--z-toast); max-width: 380px; width: calc(100vw - 48px);
    }
    .toast {
      display: flex; align-items: center; gap: 12px;
      padding: 14px 16px; border-radius: 14px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      box-shadow: 0 8px 32px rgba(0,0,0,0.12), 0 2px 8px rgba(0,0,0,0.06);
      cursor: default;
      font-size: var(--text-sm); font-weight: var(--font-medium);
      border-left: 4px solid transparent;
    }
    .toast-success { border-left-color: #10B981; }
    .toast-success .toast-icon { color: #10B981; }
    .toast-error   { border-left-color: #EF4444; }
    .toast-error   .toast-icon { color: #EF4444; }
    .toast-warning { border-left-color: #F59E0B; }
    .toast-warning .toast-icon { color: #F59E0B; }
    .toast-info    { border-left-color: #3B82F6; }
    .toast-info    .toast-icon { color: #3B82F6; }
    .toast-icon { flex-shrink: 0; display: flex; align-items: center; }
    .toast-message { flex: 1; color: var(--color-text-primary); line-height: 1.4; }
    .toast-close {
      background: none; border: none; cursor: pointer; padding: 4px;
      color: var(--color-text-muted); border-radius: 6px;
      display: flex; align-items: center; flex-shrink: 0;
      transition: all 150ms;
    }
    .toast-close:hover { color: var(--color-text-primary); background: var(--color-surface-2); }
  `]
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);
}
