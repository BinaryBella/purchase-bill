import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { LocationDto } from '../../../../core/models/auth.models';
import { NewDraftPurchaseBillItem } from '../../../../core/models/purchase-bill.models';
import {
  calculateMargin,
  calculateTotalCost,
  calculateTotalSelling,
} from '../../../../core/utils/purchase-bill-calculator';

/**
 * The "Items" entry row from the Purchase Bill screenshot: Item autocomplete, Batch dropdown,
 * cost/price/qty/discount inputs, and the computed Margin/Total Cost/Total Selling fields.
 * Purely presentational - the parent page owns the accumulated rows and the catalog data.
 */
@Component({
  selector: 'app-item-entry-form',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './item-entry-form.html',
  styleUrl: './item-entry-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ItemEntryForm {
  readonly items = input.required<string[]>();
  readonly locations = input.required<LocationDto[]>();
  readonly add = output<NewDraftPurchaseBillItem>();

  private readonly formBuilder = inject(FormBuilder);

  private readonly itemInCatalogValidator: ValidatorFn = (control): ValidationErrors | null => {
    const value = control.value as string;
    if (!value) {
      return null;
    }
    return this.items().includes(value) ? null : { notInCatalog: true };
  };

  protected readonly form = this.formBuilder.nonNullable.group({
    itemName: ['', [Validators.required, this.itemInCatalogValidator]],
    batchLocationCode: ['', [Validators.required]],
    standardCost: [0, [Validators.required, Validators.min(0)]],
    standardPrice: [0, [Validators.required, Validators.min(0)]],
    quantity: [0, [Validators.required, Validators.min(0.01)]],
    freeQuantity: [0, [Validators.min(0)]],
    discountPercent: [0, [Validators.min(0), Validators.max(100)]],
  });

  private readonly formValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });

  protected readonly filteredItems = computed(() => {
    const query = (this.formValue().itemName ?? '').toLowerCase();
    return this.items().filter((item) => item.toLowerCase().includes(query));
  });

  protected readonly margin = computed(() =>
    calculateMargin(this.formValue().standardCost ?? 0, this.formValue().standardPrice ?? 0),
  );
  protected readonly totalCost = computed(() =>
    calculateTotalCost(
      this.formValue().standardCost ?? 0,
      this.formValue().quantity ?? 0,
      this.formValue().discountPercent ?? 0,
    ),
  );
  protected readonly totalSelling = computed(() =>
    calculateTotalSelling(this.formValue().standardPrice ?? 0, this.formValue().quantity ?? 0),
  );

  protected itemErrorMessage(): string {
    const control = this.form.controls.itemName;
    if (control.hasError('required')) {
      return 'Item is required.';
    }
    if (control.hasError('notInCatalog')) {
      return 'Choose an item from the list.';
    }
    return '';
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const location = this.locations().find((l) => l.locationCode === value.batchLocationCode);

    this.add.emit({
      itemName: value.itemName,
      batchLocationCode: value.batchLocationCode,
      batchLocationName: location?.locationName ?? value.batchLocationCode,
      standardCost: value.standardCost,
      standardPrice: value.standardPrice,
      margin: this.margin(),
      quantity: value.quantity,
      freeQuantity: value.freeQuantity,
      discountPercent: value.discountPercent,
      totalCost: this.totalCost(),
      totalSelling: this.totalSelling(),
    });

    this.form.reset({
      itemName: '',
      batchLocationCode: '',
      standardCost: 0,
      standardPrice: 0,
      quantity: 0,
      freeQuantity: 0,
      discountPercent: 0,
    });
  }
}
