import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { ItemTable } from './item-table';
import { DraftPurchaseBillItem } from '../../../../core/models/purchase-bill.models';

describe('ItemTable', () => {
  let fixture: ComponentFixture<ItemTable>;
  let component: ItemTable;

  const row: DraftPurchaseBillItem = {
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
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ItemTable] }).compileComponents();
    fixture = TestBed.createComponent(ItemTable);
    component = fixture.componentInstance;
  });

  it('shows the empty state with no rows', () => {
    fixture.componentRef.setInput('rows', []);
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('No items added yet.');
  });

  it('renders one row per item, with the resolved batch name', () => {
    fixture.componentRef.setInput('rows', [row]);
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Mango');
    expect(text).toContain('Head Office');
    expect(text).toContain('400.00');
    expect(text).toContain('750.00');
  });

  it('emits the rowId when its remove button is clicked', () => {
    fixture.componentRef.setInput('rows', [row]);
    fixture.detectChanges();

    const removeSpy = vi.fn();
    component.remove.subscribe(removeSpy);

    fixture.debugElement.query(By.css('button[aria-label^="Remove"]')).nativeElement.click();

    expect(removeSpy).toHaveBeenCalledWith(1);
  });
});
