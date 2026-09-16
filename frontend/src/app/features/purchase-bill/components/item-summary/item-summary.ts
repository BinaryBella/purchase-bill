import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { DraftPurchaseBillItem } from '../../../../core/models/purchase-bill.models';

/**
 * The "Item Summary" panel: Total Items = row count, Total Quantity = sum of Qty, updated live
 * as rows are added or removed - no backend round trip needed for this, per the brief.
 */
@Component({
  selector: 'app-item-summary',
  imports: [MatIconModule],
  templateUrl: './item-summary.html',
  styleUrl: './item-summary.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemSummary {
  readonly rows = input.required<DraftPurchaseBillItem[]>();

  protected readonly totalItems = computed(() => this.rows().length);
  protected readonly totalQuantity = computed(() =>
    this.rows().reduce((sum, row) => sum + row.quantity, 0),
  );
}
