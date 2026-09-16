import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { DraftPurchaseBillItem } from '../../../../core/models/purchase-bill.models';

const COLUMNS = [
  'itemName',
  'batchLocationName',
  'standardCost',
  'standardPrice',
  'margin',
  'quantity',
  'freeQuantity',
  'discountPercent',
  'totalCost',
  'totalSelling',
  'actions',
];

/** The grid of rows added so far, matching the table below the Items entry form in the screenshot. */
@Component({
  selector: 'app-item-table',
  imports: [MatTableModule, MatButtonModule, MatIconModule, DecimalPipe],
  templateUrl: './item-table.html',
  styleUrl: './item-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemTable {
  readonly rows = input.required<DraftPurchaseBillItem[]>();
  readonly remove = output<number>();

  protected readonly displayedColumns = COLUMNS;
  protected readonly dataSource = computed(() => this.rows());
}
