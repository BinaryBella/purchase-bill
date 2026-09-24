import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ItemQuantity } from '../../../../core/models/dashboard.models';
import { DashboardService } from '../../../../core/services/dashboard.service';
import { createWidgetLoader } from '../../widget-loader';
import { WidgetCard } from '../widget-card/widget-card';

/** Colours cycle through the palette when there are more items than entries. */
export const DONUT_COLORS = [
  '#00a3cf',
  '#0288b5',
  '#0d5c7c',
  '#22c55e',
  '#f5a623',
  '#8e6cef',
  '#e5567b',
  '#7a8794',
] as const;

const RADIUS = 60;
const CIRCUMFERENCE = 2 * Math.PI * RADIUS;

export interface DonutSlice extends ItemQuantity {
  color: string;
  /** stroke-dasharray value: visible arc length, then the gap. */
  dashArray: string;
  /** Negative offset rotates the arc to start where the previous slice ended. */
  dashOffset: number;
}

/**
 * Builds the SVG arcs for a set of slices. Each slice is a full circle stroked with a
 * dasharray, offset so it starts where the previous one ended (starting at 12 o'clock).
 */
export function buildDonutSlices(items: readonly ItemQuantity[], total: number): DonutSlice[] {
  let consumed = 0;
  return items.map((item, index) => {
    const length = total > 0 ? (item.totalQuantity / total) * CIRCUMFERENCE : 0;
    const slice: DonutSlice = {
      ...item,
      color: DONUT_COLORS[index % DONUT_COLORS.length],
      dashArray: `${length} ${CIRCUMFERENCE - length}`,
      dashOffset: -consumed,
    };
    consumed += length;
    return slice;
  });
}

/** Widget 03 (donut chart): total quantity grouped by item name. */
@Component({
  selector: 'app-items-donut',
  imports: [WidgetCard, DecimalPipe],
  templateUrl: './items-donut.html',
  styleUrl: './items-donut.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemsDonut {
  private readonly dashboardService = inject(DashboardService);

  protected readonly radius = RADIUS;

  protected readonly loader = createWidgetLoader(
    (range) => this.dashboardService.getItemsByQuantity(range),
    'All', // the brief asks for all items grouped by name
  );

  protected readonly total = computed(() => {
    const state = this.loader.state();
    return state.status === 'ready' ? state.data.totalQuantity : 0;
  });

  protected readonly slices = computed(() => {
    const state = this.loader.state();
    return state.status === 'ready' ? buildDonutSlices(state.data.items, state.data.totalQuantity) : [];
  });
}
