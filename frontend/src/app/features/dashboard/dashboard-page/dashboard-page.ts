import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ItemsDonut } from '../components/items-donut/items-donut';
import { LatestOrdersTable } from '../components/latest-orders-table/latest-orders-table';
import { OldestItemsList } from '../components/oldest-items-list/oldest-items-list';

/**
 * Welcome dashboard: three independent widgets, each fetching its own data so one failing
 * request only affects its own card.
 */
@Component({
  selector: 'app-dashboard-page',
  imports: [LatestOrdersTable, OldestItemsList, ItemsDonut],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPage {}
