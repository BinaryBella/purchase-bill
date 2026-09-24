import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { DashboardService } from '../../../../core/services/dashboard.service';
import { OldestItemsList } from './oldest-items-list';

describe('OldestItemsList', () => {
  let dashboardService: { getOldestItems: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    dashboardService = { getOldestItems: vi.fn() };
    TestBed.configureTestingModule({
      imports: [OldestItemsList],
      providers: [{ provide: DashboardService, useValue: dashboardService }],
    });
  });

  it('renders purchase order id, item name and quantity for each row', async () => {
    dashboardService.getOldestItems.mockReturnValue(
      of([
        { purchaseOrderId: 1, poNumber: 'PO-000001', itemName: 'Mango', quantity: 5, createdAt: '2026-09-20T10:00:00Z' },
        { purchaseOrderId: 1, poNumber: 'PO-000001', itemName: 'Apple', quantity: 2.5, createdAt: '2026-09-20T10:00:00Z' },
      ]),
    );

    const fixture = TestBed.createComponent(OldestItemsList);
    await fixture.whenStable();
    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('li');

    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('Mango');
    expect(rows[0].textContent).toContain('PO-000001');
    expect(rows[1].textContent).toContain('2.5');
  });

  it('defaults to all orders, not just today', async () => {
    dashboardService.getOldestItems.mockReturnValue(of([]));

    TestBed.createComponent(OldestItemsList);
    await TestBed.inject(ApplicationRef).whenStable();

    expect(dashboardService.getOldestItems).toHaveBeenCalledWith('All');
  });

  it('shows the empty state when there are no items', async () => {
    dashboardService.getOldestItems.mockReturnValue(of([]));

    const fixture = TestBed.createComponent(OldestItemsList);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain(
      'No purchase order items for this period.',
    );
  });
});
