import { Component, computed, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (totalPages() > 1) {
      <div class="pagination">
        <button class="page-btn" [disabled]="!hasPrev()" (click)="go(currentPage() - 1)">
          <svg width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><polyline points="15,18 9,12 15,6"/></svg>
        </button>

        @for (p of pages(); track p) {
          @if (p === -1) {
            <span class="page-ellipsis">…</span>
          } @else {
            <button class="page-btn" [class.active]="p === currentPage()" (click)="go(p)">{{ p }}</button>
          }
        }

        <button class="page-btn" [disabled]="!hasNext()" (click)="go(currentPage() + 1)">
          <svg width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><polyline points="9,18 15,12 9,6"/></svg>
        </button>
      </div>
    }
  `,
  styles: [`
    .pagination { display: flex; align-items: center; gap: 6px; justify-content: center; margin: var(--space-8) 0; }
    .page-btn {
      width: 36px; height: 36px; border-radius: var(--radius-md);
      border: 1px solid var(--color-border); background: var(--color-surface);
      color: var(--color-text-secondary); cursor: pointer; font-size: var(--text-sm);
      display: flex; align-items: center; justify-content: center;
      transition: all 150ms ease; font-family: var(--font-sans);
      &:hover:not(:disabled):not(.active) { border-color: var(--color-primary); color: var(--color-primary); }
      &.active { background: var(--color-primary); border-color: var(--color-primary); color: #fff; font-weight: 600; }
      &:disabled { opacity: 0.4; cursor: not-allowed; }
    }
    .page-ellipsis { width: 36px; text-align: center; color: var(--color-text-muted); }
  `]
})
export class PaginationComponent {
  currentPage = input<number>(1);
  totalPages  = input<number>(1);
  pageChange  = output<number>();

  hasPrev = computed(() => this.currentPage() > 1);
  hasNext = computed(() => this.currentPage() < this.totalPages());

  pages = computed(() => {
    const total = this.totalPages();
    const cur   = this.currentPage();
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);

    const pages: number[] = [1];
    if (cur > 3) pages.push(-1);
    for (let i = Math.max(2, cur - 1); i <= Math.min(total - 1, cur + 1); i++) pages.push(i);
    if (cur < total - 2) pages.push(-1);
    pages.push(total);
    return pages;
  });

  go(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.pageChange.emit(page);
  }
}
