import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="empty-state">
      <div class="empty-icon">{{ icon }}</div>
      <h3 class="empty-title">{{ title }}</h3>
      <p class="empty-desc">{{ description }}</p>
      @if (actionLabel && actionLink) {
        <a [routerLink]="actionLink" class="btn-primary">{{ actionLabel }}</a>
      }
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex; flex-direction: column; align-items: center;
      text-align: center; padding: var(--space-16) var(--space-4); gap: var(--space-4);
    }
    .empty-icon { font-size: 64px; line-height: 1; opacity: 0.3; }
    .empty-title { font-size: var(--text-xl); font-weight: var(--font-bold); color: var(--color-text-primary); }
    .empty-desc { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 320px; }
  `]
})
export class EmptyStateComponent {
  @Input() icon = '📭';
  @Input() title = 'Nothing here yet';
  @Input() description = '';
  @Input() actionLabel?: string;
  @Input() actionLink?: string;
}
