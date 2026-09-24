import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { DashboardService } from '../../../../core/services/dashboard.service';
import { createWidgetLoader } from '../../widget-loader';
import { WidgetCard } from '../widget-card/widget-card';

/** Widget 01 (table view): the 5 most recently added purchase orders. */
@Component({
  selector: 'app-latest-orders-table',
  imports: [WidgetCard, DecimalPipe],
  templateUrl: './latest-orders-table.html',
  styleUrl: './latest-orders-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LatestOrdersTable {
  private readonly dashboardService = inject(DashboardService);

  protected readonly loader = createWidgetLoader(() =>
    this.dashboardService.getLatestOrders(),
  );

  protected readonly orders = computed(() => {
    const state = this.loader.state();
    return state.status === 'ready' ? state.data : [];
  });
}
