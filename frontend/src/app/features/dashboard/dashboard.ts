import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

/**
 * Placeholder landing page reached after a successful login, behind authGuard. Stands in for
 * the Purchase Bill page (Task 2) until that's built - its purpose right now is to prove the
 * login -> protected route -> logout loop works end to end.
 */
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dashboard {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly username = this.authService.username;

  protected logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
