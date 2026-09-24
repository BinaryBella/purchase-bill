import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  /** Only modules that exist in this app link anywhere; the rest are visual placeholders. */
  route?: string;
}

/**
 * Layout wrapper for every authenticated page: the gradient top bar and the dark sidebar from
 * the welcome-dashboard design, with the active page rendered in the router outlet. Sidebar
 * modules other than Dashboard and Procurement (Purchase Bill) are not built yet, so they are
 * shown but disabled rather than pointing at dead routes.
 */
@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatIconModule],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShell {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly navItems: readonly NavItem[] = [
    { label: 'Shortcuts', icon: 'star_border', route: '/dashboard' },
    { label: 'Core', icon: 'apartment' },
    { label: 'Finance', icon: 'attach_money' },
    { label: 'Assets', icon: 'domain' },
    { label: 'Procurement', icon: 'receipt_long', route: '/purchase-bill' },
    { label: 'Inventory', icon: 'inventory_2' },
    { label: 'Manufacturing', icon: 'precision_manufacturing' },
    { label: 'Sales', icon: 'shopping_cart' },
    { label: 'CRM', icon: 'contacts' },
    { label: 'Services', icon: 'public' },
    { label: 'Docs', icon: 'description' },
    { label: 'Analytics', icon: 'trending_up' },
    { label: 'Admin', icon: 'admin_panel_settings' },
  ];

  protected readonly menuOpen = signal(false);
  protected readonly username = this.authService.username;
  protected readonly initial = computed(() => (this.username() ?? '?').charAt(0).toUpperCase());

  protected toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  protected logout(): void {
    this.menuOpen.set(false);
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
