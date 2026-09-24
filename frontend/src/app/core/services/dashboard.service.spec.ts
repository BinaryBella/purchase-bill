import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { DashboardService } from './dashboard.service';

describe('DashboardService', () => {
  let service: DashboardService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule] });
    service = TestBed.inject(DashboardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('requests the latest orders for the chosen range', () => {
    service.getLatestOrders('Last7Days').subscribe();

    const req = httpMock.expectOne(
      (r) => r.url === `${environment.apiUrl}/dashboard/latest-orders`,
    );
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('range')).toBe('Last7Days');
    req.flush([]);
  });

  it('requests the oldest items for the chosen range', () => {
    service.getOldestItems('Today').subscribe();

    const req = httpMock.expectOne((r) => r.url === `${environment.apiUrl}/dashboard/oldest-items`);
    expect(req.request.params.get('range')).toBe('Today');
    req.flush([]);
  });

  it('requests the item quantities for the chosen range', () => {
    let result: unknown;
    service.getItemsByQuantity('All').subscribe((r) => (result = r));

    const req = httpMock.expectOne(
      (r) => r.url === `${environment.apiUrl}/dashboard/items-by-quantity`,
    );
    expect(req.request.params.get('range')).toBe('All');
    req.flush({ totalQuantity: 3, items: [] });

    expect(result).toEqual({ totalQuantity: 3, items: [] });
  });
});
