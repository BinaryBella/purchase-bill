import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { DashboardService } from '../../../../core/services/dashboard.service';
import { LatestOrdersTable } from './latest-orders-table';

describe('LatestOrdersTable', () => {
  let dashboardService: { getLatestOrders: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    dashboardService = { getLatestOrders: vi.fn() };
    TestBed.configureTestingModule({
      imports: [LatestOrdersTable],
      providers: [{ provide: DashboardService, useValue: dashboardService }],
    });
  });

  it('renders ID, net amount and item count for each order', async () => {
    dashboardService.getLatestOrders.mockReturnValue(
      of([
        { id: 2, poNumber: 'PO-000002', netAmount: 1234.5, itemCount: 3, createdAt: '2026-09-24T10:00:00Z' },
        { id: 1, poNumber: 'PO-000001', netAmount: 400, itemCount: 1, createdAt: '2026-09-24T09:00:00Z' },
      ]),
    );

    const fixture = TestBed.createComponent(LatestOrdersTable);
    await fixture.whenStable();
    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');

    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('PO-000002');
    expect(rows[0].textContent).toContain('1,234.50');
    expect(rows[0].textContent).toContain('3');
  });

  it('has no date-range dropdown', async () => {
    dashboardService.getLatestOrders.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(LatestOrdersTable);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('select')).toBeNull();
    expect(dashboardService.getLatestOrders).toHaveBeenCalledWith();
  });

  it('shows the empty state when there are no orders', async () => {
    dashboardService.getLatestOrders.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(LatestOrdersTable);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'No purchase orders yet.',
    );
  });
});
