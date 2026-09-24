import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { DashboardService } from '../../../../core/services/dashboard.service';
import { createWidgetLoader } from '../../widget-loader';
import { WidgetCard } from '../widget-card/widget-card';

/** Widget 02 (list view): the 10 oldest purchase order line items. */
@Component({
  selector: 'app-oldest-items-list',
  imports: [WidgetCard, DatePipe, DecimalPipe],
  templateUrl: './oldest-items-list.html',
  styleUrl: './oldest-items-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OldestItemsList {
  private readonly dashboardService = inject(DashboardService);

  protected readonly loader = createWidgetLoader(
    (range) => this.dashboardService.getOldestItems(range),
    'All', // "oldest 10 items" is only meaningful across all orders, not just today's
  );

  protected readonly items = computed(() => {
    const state = this.loader.state();
    return state.status === 'ready' ? state.data : [];
  });
}
