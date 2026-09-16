import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { PurchaseBillService } from './purchase-bill.service';
import { PurchaseBillResponse } from '../models/purchase-bill.models';

describe('PurchaseBillService', () => {
  let service: PurchaseBillService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule] });
    service = TestBed.inject(PurchaseBillService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('fetches the item catalog', () => {
    let items: string[] | undefined;
    service.getItems().subscribe((r) => (items = r));

    const req = httpMock.expectOne(`${environment.apiUrl}/items`);
    expect(req.request.method).toBe('GET');
    req.flush(['Mango', 'Apple']);

    expect(items).toEqual(['Mango', 'Apple']);
  });

  it('fetches saved locations', () => {
    let locations: unknown;
    service.getLocations().subscribe((r) => (locations = r));

    const req = httpMock.expectOne(`${environment.apiUrl}/locations`);
    expect(req.request.method).toBe('GET');
    req.flush([{ locationCode: 'LOC-1', locationName: 'Head Office' }]);

    expect(locations).toEqual([{ locationCode: 'LOC-1', locationName: 'Head Office' }]);
  });

  it('posts a new purchase bill', () => {
    const response: PurchaseBillResponse = {
      id: 1,
      createdAt: new Date().toISOString(),
      totalItems: 1,
      totalQuantity: 5,
      totalCost: 400,
      totalSelling: 750,
      items: [],
    };
    let received: PurchaseBillResponse | undefined;

    service
      .create({
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
      })
      .subscribe((r) => (received = r));

    const req = httpMock.expectOne(`${environment.apiUrl}/purchase-bills`);
    expect(req.request.method).toBe('POST');
    req.flush(response);

    expect(received).toEqual(response);
  });
});
