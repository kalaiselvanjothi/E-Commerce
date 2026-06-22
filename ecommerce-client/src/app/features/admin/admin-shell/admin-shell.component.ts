import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="admin-layout">
      <aside class="admin-sidebar">
        <div class="sidebar-logo">
          <span class="logo-icon">SV</span>
          <div>
            <div class="logo-title">ShopVerse</div>
            <div class="logo-sub">Admin Panel</div>
          </div>
        </div>
        <nav class="admin-nav">
          @for (item of navItems; track item.path) {
            <a [routerLink]="item.path" routerLinkActive="active" class="nav-item">
              <span class="nav-icon">{{ item.icon }}</span>
              <span>{{ item.label }}</span>
            </a>
          }
        </nav>
        <div class="sidebar-footer">
          <a routerLink="/" class="nav-item">
            <span class="nav-icon">🏠</span>
            <span>Back to Store</span>
          </a>
        </div>
      </aside>
      <main class="admin-main">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    .admin-layout { display: flex; min-height: 100vh; padding-top: 0; }
    .admin-sidebar {
      width: var(--sidebar-width); background: var(--color-header-bg); color: rgba(255,255,255,0.8);
      position: fixed; top: 0; left: 0; bottom: 0; display: flex; flex-direction: column;
      z-index: var(--z-sticky); overflow-y: auto;
    }
    .sidebar-logo { display: flex; align-items: center; gap: 12px; padding: 20px 16px; border-bottom: 1px solid rgba(255,255,255,0.1); }
    .logo-icon { width: 36px; height: 36px; background: var(--color-primary); color: #fff; font-weight: 800; font-size: 13px; border-radius: 8px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .logo-title { font-weight: 700; font-size: 14px; color: #fff; }
    .logo-sub { font-size: 11px; color: rgba(255,255,255,0.4); }
    .admin-nav { flex: 1; padding: 12px 8px; display: flex; flex-direction: column; gap: 2px; }
    .nav-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; border-radius: 8px; font-size: 13px; color: rgba(255,255,255,0.7); text-decoration: none; transition: all 150ms; cursor: pointer; border: none; background: none; font-family: var(--font-sans); width: 100%; &:hover { background: rgba(255,255,255,0.08); color: #fff; } &.active { background: var(--color-primary); color: #fff; } }
    .nav-icon { font-size: 16px; flex-shrink: 0; }
    .sidebar-footer { padding: 8px; border-top: 1px solid rgba(255,255,255,0.1); }
    .admin-main { margin-left: var(--sidebar-width); flex: 1; background: var(--color-bg); padding: var(--space-8); min-height: 100vh; }
  `]
})
export class AdminShellComponent {
  readonly navItems = [
    { path: '/admin/dashboard',  icon: '📊', label: 'Dashboard' },
    { path: '/admin/products',   icon: '📦', label: 'Products' },
    { path: '/admin/categories', icon: '🗂️', label: 'Categories' },
    { path: '/admin/orders',     icon: '🛍️', label: 'Orders' },
    { path: '/admin/customers',  icon: '👥', label: 'Customers' },
    { path: '/admin/coupons',    icon: '🏷️', label: 'Coupons' },
  ];
}
