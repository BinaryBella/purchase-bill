import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ItemEntryForm } from './item-entry-form';
import { LocationDto } from '../../../../core/models/auth.models';

describe('ItemEntryForm', () => {
  let fixture: ComponentFixture<ItemEntryForm>;
  let component: ItemEntryForm;

  const items = ['Mango', 'Apple', 'Banana'];
  const locations: LocationDto[] = [
    { locationCode: 'LOC-1', locationName: 'Head Office' },
    { locationCode: 'LOC-2', locationName: 'Warehouse' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ItemEntryForm, NoopAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ItemEntryForm);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('items', items);
    fixture.componentRef.setInput('locations', locations);
    fixture.detectChanges();
  });

  it('computes Margin/Total Cost/Total Selling live, matching the brief worked example', () => {
    component['form'].patchValue({
      standardCost: 100,
      standardPrice: 150,
      quantity: 5,
      discountPercent: 20,
    });

    expect(component['margin']()).toBe(50);
    expect(component['totalCost']()).toBe(400);
    expect(component['totalSelling']()).toBe(750);
  });

  it('filters the item list as the user types', () => {
    component['form'].patchValue({ itemName: 'an' });

    expect(component['filteredItems']()).toEqual(['Mango', 'Banana']);
  });

  it('does not emit and marks fields touched when the form is invalid', () => {
    const addSpy = vi.fn();
    component.add.subscribe(addSpy);

    component['submit']();

    expect(addSpy).not.toHaveBeenCalled();
    expect(component['form'].controls.itemName.touched).toBe(true);
  });

  it('rejects an item that is not in the catalog', () => {
    component['form'].patchValue({
      itemName: 'Watermelon',
      batchLocationCode: 'LOC-1',
      standardCost: 10,
      standardPrice: 20,
      quantity: 1,
    });

    expect(component['form'].controls.itemName.hasError('notInCatalog')).toBe(true);
    expect(component['itemErrorMessage']()).toBe('Choose an item from the list.');
  });

  it('emits the computed row and resets the form on a valid submit', () => {
    const addSpy = vi.fn();
    component.add.subscribe(addSpy);

    component['form'].setValue({
      itemName: 'Mango',
      batchLocationCode: 'LOC-1',
      standardCost: 100,
      standardPrice: 150,
      quantity: 5,
      freeQuantity: 1,
      discountPercent: 20,
    });

    component['submit']();

    expect(addSpy).toHaveBeenCalledWith({
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
    });
    expect(component['form'].controls.itemName.value).toBe('');
  });
});
