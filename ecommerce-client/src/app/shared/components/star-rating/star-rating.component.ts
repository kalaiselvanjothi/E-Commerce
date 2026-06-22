import { Component, Input, Output, EventEmitter, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stars" [class.interactive]="interactive">
      @for (star of stars; track star) {
        <button
          class="star"
          [class.filled]="star <= displayRating()"
          [class.half]="!interactive && star - 0.5 === displayRating()"
          [disabled]="!interactive"
          (mouseenter)="interactive && hover.set(star)"
          (mouseleave)="interactive && hover.set(0)"
          (click)="interactive && select(star)"
          type="button"
        >★</button>
      }
      @if (showCount && count !== undefined) {
        <span class="rating-count">({{ count | number }})</span>
      }
    </div>
  `,
  styles: [`
    .stars { display: inline-flex; align-items: center; gap: 1px; }
    .star {
      background: none; border: none; cursor: default;
      font-size: 16px; color: #CBD5E1; padding: 0; line-height: 1;
      transition: color 150ms ease, transform 150ms ease;
    }
    .star.filled { color: #F59E0B; }
    .interactive .star { cursor: pointer; }
    .interactive .star:hover { transform: scale(1.15); }
    .rating-count { font-size: 12px; color: var(--color-text-muted); margin-left: 4px; }
    :host(.sm) .star { font-size: 13px; }
    :host(.lg) .star { font-size: 22px; }
  `]
})
export class StarRatingComponent {
  @Input() rating = 0;
  @Input() count?: number;
  @Input() showCount = false;
  @Input() interactive = false;
  @Output() ratingChange = new EventEmitter<number>();

  readonly stars = [1, 2, 3, 4, 5];
  readonly hover = signal(0);

  readonly displayRating = computed(() => this.interactive && this.hover() > 0 ? this.hover() : this.rating);

  select(star: number): void {
    this.ratingChange.emit(star);
  }
}
