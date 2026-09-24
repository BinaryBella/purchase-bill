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

  it('requests the latest orders without any date filter', () => {
    service.getLatestOrders().subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/dashboard/latest-orders`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.keys()).toEqual([]);
    req.flush([]);
  });

  it('requests the oldest items without any date filter', () => {
    service.getOldestItems().subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/dashboard/oldest-items`);
    expect(req.request.params.keys()).toEqual([]);
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
