import { ChangeDetectionStrategy, Component, input, model, output } from '@angular/core';
import { DASHBOARD_RANGES, DashboardRange } from '../../../../core/models/dashboard.models';
import { WidgetState } from '../../widget-loader';

/**
 * Card chrome shared by every dashboard widget: title, "Today"-style range picker, and the
 * loading / error / empty states. The projected content is only rendered once data is ready
 * and non-empty, so each widget just supplies its own markup for the happy path.
 */
@Component({
  selector: 'app-widget-card',
  templateUrl: './widget-card.html',
  styleUrl: './widget-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WidgetCard {
  readonly title = input.required<string>();
  readonly range = model<DashboardRange>('All');
  /** Widgets that are not date-filtered hide the dropdown. */
  readonly showRange = input(true);
  readonly state = input.required<WidgetState<unknown>>();
  readonly isEmpty = input(false);
  readonly emptyMessage = input('Nothing to show for this period.');
  readonly retry = output<void>();

  protected readonly ranges = DASHBOARD_RANGES;

  protected onRangeChange(event: Event): void {
    this.range.set((event.target as HTMLSelectElement).value as DashboardRange);
  }

  protected errorMessage(): string {
    const current = this.state();
    return current.status === 'error' ? current.message : '';
  }
}
