import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="empty-state">
      <div class="empty-icon-ring">
        <span class="empty-icon">{{ icon }}</span>
      </div>
      <div class="empty-text">
        <h3 class="empty-title">{{ title }}</h3>
        @if (description) {
          <p class="empty-desc">{{ description }}</p>
        }
      </div>
      @if (actionLabel && actionLink) {
        <a [routerLink]="actionLink" class="btn-primary empty-action" id="empty-state-action-btn">
          {{ actionLabel }}
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M5 12h14M12 5l7 7-7 7"/>
          </svg>
        </a>
      }
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      padding: var(--space-20) var(--space-6);
      gap: var(--space-5);
    }
    .empty-icon-ring {
      width: 96px; height: 96px;
      border-radius: 50%;
      background: var(--color-surface-2);
      border: 2px solid var(--color-border);
      display: flex; align-items: center; justify-content: center;
      animation: float 3.5s ease-in-out infinite;
    }
    .empty-icon {
      font-size: 44px; line-height: 1;
    }
    .empty-text { display: flex; flex-direction: column; gap: var(--space-2); }
    .empty-title {
      font-family: var(--font-display);
      font-size: var(--text-xl);
      font-weight: var(--font-extrabold);
      color: var(--color-text-primary);
      letter-spacing: -0.3px;
    }
    .empty-desc {
      font-size: var(--text-sm);
      color: var(--color-text-muted);
      max-width: 340px;
      line-height: 1.6;
    }
    .empty-action {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 11px 24px;
      text-decoration: none;
    }
    @keyframes float {
      0%, 100% { transform: translateY(0); }
      50%       { transform: translateY(-10px); }
    }
  `]
})
export class EmptyStateComponent {
  @Input() icon = '📭';
  @Input() title = 'Nothing here yet';
  @Input() description = '';
  @Input() actionLabel?: string;
  @Input() actionLink?: string;
}
