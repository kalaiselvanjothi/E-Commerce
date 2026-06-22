import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

export interface BreadcrumbItem {
  label: string;
  link?: string;
}

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [RouterLink],
  template: `
    <nav class="breadcrumb" aria-label="breadcrumb">
      @for (item of items; track item.label; let last = $last) {
        @if (item.link && !last) {
          <a [routerLink]="item.link" class="bc-link">{{ item.label }}</a>
          <span class="bc-sep">›</span>
        } @else {
          <span class="bc-current" [attr.aria-current]="last ? 'page' : null">{{ item.label }}</span>
        }
      }
    </nav>
  `,
  styles: [`
    .breadcrumb { display: flex; align-items: center; gap: var(--space-2); flex-wrap: wrap; margin-bottom: var(--space-6); }
    .bc-link { font-size: var(--text-sm); color: var(--color-text-muted); text-decoration: none; &:hover { color: var(--color-primary); } }
    .bc-sep { color: var(--color-text-muted); font-size: var(--text-sm); }
    .bc-current { font-size: var(--text-sm); color: var(--color-text-secondary); font-weight: 500; }
  `]
})
export class BreadcrumbComponent {
  @Input() items: BreadcrumbItem[] = [];
}
