import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { PurchaseBillPage } from './purchase-bill-page';
import { AuthService } from '../../../core/services/auth.service';
import { PurchaseBillService } from '../../../core/services/purchase-bill.service';

describe('PurchaseBillPage', () => {
  let fixture: ComponentFixture<PurchaseBillPage>;
  let component: PurchaseBillPage;
  let purchaseBillService: {
    getItems: ReturnType<typeof vi.fn>;
    getLocations: ReturnType<typeof vi.fn>;
    create: ReturnType<typeof vi.fn>;
  };
  let authService: { username: ReturnType<typeof vi.fn>; logout: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };
  let openSpy: ReturnType<typeof vi.spyOn>;

  const items = ['Mango', 'Apple'];
  const locations = [{ locationCode: 'LOC-1', locationName: 'Head Office' }];

  function createComponent() {
    fixture = TestBed.createComponent(PurchaseBillPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(async () => {
    purchaseBillService = {
      getItems: vi.fn().mockReturnValue(of(items)),
      getLocations: vi.fn().mockReturnValue(of(locations)),
      create: vi.fn(),
    };
    authService = { username: vi.fn().mockReturnValue('info@enhanzer.com'), logout: vi.fn() };
    router = { navigate: vi.fn() };
    // MatSnackBarModule (imported by the component itself) re-provides MatSnackBar in its own
    // environment injector, which shadows a plain TestBed `useValue` override - spying on the
    // prototype sidesteps that instance-identity issue.
    openSpy = vi
      .spyOn(MatSnackBar.prototype, 'open')
      .mockReturnValue({} as ReturnType<MatSnackBar['open']>);

    await TestBed.configureTestingModule({
      imports: [PurchaseBillPage, NoopAnimationsModule],
      providers: [
        { provide: PurchaseBillService, useValue: purchaseBillService },
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();
  });

  it('loads the item catalog and locations on init', () => {
    createComponent();

    expect(purchaseBillService.getItems).toHaveBeenCalled();
    expect(purchaseBillService.getLocations).toHaveBeenCalled();
    expect(component['items']()).toEqual(items);
    expect(component['locations']()).toEqual(locations);
    expect(component['loading']()).toBe(false);
  });

  it('surfaces an error if the catalog fails to load', () => {
    purchaseBillService.getItems.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500, error: { detail: 'boom' } })),
    );
    createComponent();

    expect(component['loading']()).toBe(false);
    expect(component['loadError']()).toBe('boom');
  });

  it('adds rows with incrementing ids and removes by id', () => {
    createComponent();

    component['addRow']({
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
    component['addRow']({
      itemName: 'Apple',
      batchLocationCode: 'LOC-1',
      batchLocationName: 'Head Office',
      standardCost: 50,
      standardPrice: 80,
      margin: 30,
      quantity: 3,
      freeQuantity: 0,
      discountPercent: 0,
      totalCost: 150,
      totalSelling: 240,
    });

    expect(component['rows']().map((r) => r.rowId)).toEqual([1, 2]);

    component['removeRow'](1);

    expect(component['rows']().map((r) => r.itemName)).toEqual(['Apple']);
  });

  it('warns instead of saving when there are no rows', () => {
    createComponent();

    component['save']();

    expect(purchaseBillService.create).not.toHaveBeenCalled();
    expect(openSpy).toHaveBeenCalledWith('Add at least one item before saving.', 'Dismiss', {
      duration: 4000,
    });
  });

  it('saves the accumulated rows and clears them on success', () => {
    createComponent();
    component['addRow']({
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
    purchaseBillService.create.mockReturnValue(
      of({
        id: 7,
        createdAt: '',
        totalItems: 1,
        totalQuantity: 5,
        totalCost: 400,
        totalSelling: 750,
        items: [],
      }),
    );

    component['save']();

    expect(purchaseBillService.create).toHaveBeenCalledWith({
      items: [
        {
          itemName: 'Mango',
          batchLocationCode: 'LOC-1',
          standardCost: 100,
          standardPrice: 150,
          quantity: 5,
          freeQuantity: 1,
          discountPercent: 20,
        },
      ],
    });
    expect(component['rows']()).toEqual([]);
    expect(openSpy).toHaveBeenCalledWith('Purchase Bill #7 saved.', 'Dismiss', { duration: 4000 });
  });

  it('shows the backend error message and keeps the rows if saving fails', () => {
    createComponent();
    component['addRow']({
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
    purchaseBillService.create.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 404,
            error: { detail: 'Unknown batch location(s): LOC-1.' },
          }),
      ),
    );

    component['save']();

    expect(component['rows']().length).toBe(1);
    expect(openSpy).toHaveBeenCalledWith('Unknown batch location(s): LOC-1.', 'Dismiss', {
      duration: 6000,
    });
  });

  it('logs out and navigates to /login', () => {
    createComponent();

    component['logout']();

    expect(authService.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
