import { Component, OnInit, OnDestroy, inject, signal, HostListener, ElementRef } from '@angular/core';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil, switchMap, of } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { WishlistService } from '../../core/services/wishlist.service';
import { ProductService } from '../../core/services/product.service';
import { Category, SearchSuggestion } from '../../core/models/product.models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit, OnDestroy {
  readonly auth     = inject(AuthService);
  readonly cart     = inject(CartService);
  readonly wishlist = inject(WishlistService);
  private readonly productService = inject(ProductService);
  private readonly router         = inject(Router);
  private readonly el             = inject(ElementRef);

  categories      = signal<Category[]>([]);
  suggestions     = signal<SearchSuggestion[]>([]);
  searchQuery     = signal('');
  showSuggestions = signal(false);
  showUserMenu    = signal(false);
  showMobileMenu  = signal(false);
  isSearching     = signal(false);
  isDark          = signal(false);

  private searchSubject = new Subject<string>();
  private destroy$      = new Subject<void>();

  ngOnInit(): void {
    // Load categories
    this.productService.getCategories().subscribe(cats => this.categories.set(cats));

    // Search suggestions with debounce
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(q => q.length >= 2 ? this.productService.searchSuggestions(q) : of([])),
      takeUntil(this.destroy$)
    ).subscribe(s => {
      this.suggestions.set(s);
      this.showSuggestions.set(s.length > 0);
      this.isSearching.set(false);
    });

    // Initialize theme from localStorage or system preference
    this.initTheme();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ─── Search ────────────────────────────────────────────────────────────────
  onSearchInput(q: string): void {
    this.searchQuery.set(q);
    if (q.length >= 2) this.isSearching.set(true);
    else { this.showSuggestions.set(false); this.isSearching.set(false); }
    this.searchSubject.next(q);
  }

  onSearchSubmit(): void {
    const q = this.searchQuery();
    if (!q.trim()) return;
    this.showSuggestions.set(false);
    this.router.navigate(['/products'], { queryParams: { search: q } });
  }

  selectSuggestion(s: SearchSuggestion): void {
    this.showSuggestions.set(false);
    this.searchQuery.set('');
    this.router.navigate(['/products', s.slug]);
  }

  // ─── Auth ──────────────────────────────────────────────────────────────────
  logout(): void {
    this.showUserMenu.set(false);
    this.auth.logout();
  }

  // ─── Theme ─────────────────────────────────────────────────────────────────
  toggleTheme(): void {
    const next = this.isDark() ? 'light' : 'dark';
    this.isDark.set(!this.isDark());
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('sv_theme', next);
  }

  private initTheme(): void {
    const saved = localStorage.getItem('sv_theme') as 'light' | 'dark' | null;
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const theme = saved ?? (prefersDark ? 'dark' : 'light');
    this.isDark.set(theme === 'dark');
    document.documentElement.setAttribute('data-theme', theme);
  }

  // ─── Click outside to close dropdowns ─────────────────────────────────────
  @HostListener('document:click', ['$event'])
  onDocClick(e: MouseEvent): void {
    if (!this.el.nativeElement.contains(e.target)) {
      this.showSuggestions.set(false);
      this.showUserMenu.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.showSuggestions.set(false);
    this.showUserMenu.set(false);
    this.showMobileMenu.set(false);
  }

  get cartCount(): number     { return this.cart.itemCount(); }
  get wishlistCount(): number { return this.wishlist.itemCount(); }
}
