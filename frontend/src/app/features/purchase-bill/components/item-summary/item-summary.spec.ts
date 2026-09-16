import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ItemSummary } from './item-summary';
import { DraftPurchaseBillItem } from '../../../../core/models/purchase-bill.models';

function row(overrides: Partial<DraftPurchaseBillItem>): DraftPurchaseBillItem {
  return {
    rowId: 1,
    itemName: 'Mango',
    batchLocationCode: 'LOC-1',
    batchLocationName: 'Head Office',
    standardCost: 100,
    standardPrice: 150,
    margin: 50,
    quantity: 5,
    freeQuantity: 1,
    discountPercent: 20,
    totalCost: 400,
    totalSelling: 750,
    ...overrides,
  };
}

describe('ItemSummary', () => {
  let fixture: ComponentFixture<ItemSummary>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ItemSummary] }).compileComponents();
    fixture = TestBed.createComponent(ItemSummary);
  });

  it('shows zero totals with no rows', () => {
    fixture.componentRef.setInput('rows', []);
    fixture.detectChanges();

    expect(fixture.componentInstance['totalItems']()).toBe(0);
    expect(fixture.componentInstance['totalQuantity']()).toBe(0);
  });

  it('sums Total Items and Total Quantity across rows', () => {
    fixture.componentRef.setInput('rows', [
      row({ rowId: 1, quantity: 5 }),
      row({ rowId: 2, quantity: 3 }),
    ]);
    fixture.detectChanges();

    expect(fixture.componentInstance['totalItems']()).toBe(2);
    expect(fixture.componentInstance['totalQuantity']()).toBe(8);

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('2');
    expect(text).toContain('8');
  });
});
