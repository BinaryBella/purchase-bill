import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { forkJoin } from 'rxjs';
import { PurchaseBillService } from '../../../core/services/purchase-bill.service';
import { extractErrorMessage } from '../../../core/models/api-error.model';
import { LocationDto } from '../../../core/models/auth.models';
import {
  DraftPurchaseBillItem,
  NewDraftPurchaseBillItem,
} from '../../../core/models/purchase-bill.models';
import { ItemEntryForm } from '../components/item-entry-form/item-entry-form';
import { ItemTable } from '../components/item-table/item-table';
import { ItemSummary } from '../components/item-summary/item-summary';

/**
 * Task 2: the Purchase Bill page. Loads the item catalog and saved locations, lets the user
 * build up rows client-side (Add), keeps the Item Summary in sync, and saves the whole bill to
 * the backend in one call.
 */
@Component({
  selector: 'app-purchase-bill-page',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatTabsModule,
    MatTooltipModule,
    ItemEntryForm,
    ItemTable,
    ItemSummary,
  ],
  templateUrl: './purchase-bill-page.html',
  styleUrl: './purchase-bill-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PurchaseBillPage {
  private readonly purchaseBillService = inject(PurchaseBillService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly loadError = signal<string | null>(null);

  protected readonly items = signal<string[]>([]);
  protected readonly locations = signal<LocationDto[]>([]);
  protected readonly rows = signal<DraftPurchaseBillItem[]>([]);

  private nextRowId = 1;

  constructor() {
    this.loadCatalog();
  }

  protected loadCatalog(): void {
    this.loading.set(true);
    this.loadError.set(null);

    forkJoin({
      items: this.purchaseBillService.getItems(),
      locations: this.purchaseBillService.getLocations(),
    }).subscribe({
      next: ({ items, locations }) => {
        this.items.set(items);
        this.locations.set(locations);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        const fallback = 'Unable to load the item catalog or batch locations.';
        this.loadError.set(
          error instanceof HttpErrorResponse
            ? extractErrorMessage(error.error, fallback)
            : fallback,
        );
      },
    });
  }

  protected addRow(item: NewDraftPurchaseBillItem): void {
    this.rows.update((rows) => [...rows, { ...item, rowId: this.nextRowId++ }]);
  }

  protected removeRow(rowId: number): void {
    this.rows.update((rows) => rows.filter((row) => row.rowId !== rowId));
  }

  protected save(): void {
    const rows = this.rows();
    if (rows.length === 0) {
      this.snackBar.open('Add at least one item before saving.', 'Dismiss', { duration: 4000 });
      return;
    }

    this.saving.set(true);
    this.purchaseBillService
      .create({
        items: rows.map((row) => ({
          itemName: row.itemName,
          batchLocationCode: row.batchLocationCode,
          standardCost: row.standardCost,
          standardPrice: row.standardPrice,
          quantity: row.quantity,
          freeQuantity: row.freeQuantity,
          discountPercent: row.discountPercent,
        })),
      })
      .subscribe({
        next: (response) => {
          this.saving.set(false);
          this.rows.set([]);
          this.snackBar.open(`Purchase Bill ${response.poNumber} saved.`, 'Dismiss', { duration: 4000 });
        },
        error: (error: unknown) => {
          this.saving.set(false);
          const fallback = 'Unable to save the Purchase Bill. Please try again.';
          const message =
            error instanceof HttpErrorResponse
              ? extractErrorMessage(error.error, fallback)
              : fallback;
          this.snackBar.open(message, 'Dismiss', { duration: 6000 });
        },
      });
  }

  protected close(): void {
    this.router.navigate(['/dashboard']);
  }
}
