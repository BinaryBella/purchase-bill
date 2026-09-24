import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { DashboardService } from '../../../../core/services/dashboard.service';
import { DONUT_COLORS, ItemsDonut, buildDonutSlices } from './items-donut';

describe('buildDonutSlices', () => {
  const circumference = 2 * Math.PI * 60;

  it('sizes each arc by its share and chains the offsets', () => {
    const slices = buildDonutSlices(
      [
        { itemName: 'Mango', totalQuantity: 75, percent: 75 },
        { itemName: 'Apple', totalQuantity: 25, percent: 25 },
      ],
      100,
    );

    const [visible, gap] = slices[0].dashArray.split(' ').map(Number);
    expect(visible).toBeCloseTo(circumference * 0.75);
    expect(gap).toBeCloseTo(circumference * 0.25);
    expect(slices[0].dashOffset).toBeCloseTo(0);
    expect(slices[1].dashOffset).toBeCloseTo(-circumference * 0.75);
    expect(slices[0].color).toBe(DONUT_COLORS[0]);
    expect(slices[1].color).toBe(DONUT_COLORS[1]);
  });

  it('cycles colours when there are more slices than palette entries', () => {
    const items = Array.from({ length: DONUT_COLORS.length + 1 }, (_, i) => ({
      itemName: `Item ${i}`,
      totalQuantity: 1,
      percent: 1,
    }));

    const slices = buildDonutSlices(items, items.length);

    expect(slices[DONUT_COLORS.length].color).toBe(DONUT_COLORS[0]);
  });

  it('draws nothing when the total is zero', () => {
    const slices = buildDonutSlices([{ itemName: 'Mango', totalQuantity: 0, percent: 0 }], 0);

    expect(slices[0].dashArray.startsWith('0 ')).toBe(true);
  });
});

describe('ItemsDonut', () => {
  let dashboardService: { getItemsByQuantity: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    dashboardService = { getItemsByQuantity: vi.fn() };
    TestBed.configureTestingModule({
      imports: [ItemsDonut],
      providers: [{ provide: DashboardService, useValue: dashboardService }],
    });
  });

  it('renders the total and a legend entry per item', async () => {
    dashboardService.getItemsByQuantity.mockReturnValue(
      of({
        totalQuantity: 10,
        items: [
          { itemName: 'Mango', totalQuantity: 7, percent: 70 },
          { itemName: 'Apple', totalQuantity: 3, percent: 30 },
        ],
      }),
    );

    const fixture = TestBed.createComponent(ItemsDonut);
    await fixture.whenStable();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(dashboardService.getItemsByQuantity).toHaveBeenCalledWith('All');
    expect(text).toContain('Mango');
    expect(text).toContain('Apple');
    expect(text).toContain('10');
    expect((fixture.nativeElement as HTMLElement).querySelectorAll('circle').length).toBe(2);
  });

  it('shows the empty state when there is no data', async () => {
    dashboardService.getItemsByQuantity.mockReturnValue(of({ totalQuantity: 0, items: [] }));

    const fixture = TestBed.createComponent(ItemsDonut);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('No items for this period.');
  });

  it('shows the error message with a retry that refetches', async () => {
    dashboardService.getItemsByQuantity.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500, error: { detail: 'boom' } })),
    );

    const fixture = TestBed.createComponent(ItemsDonut);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain('boom');

    element.querySelector<HTMLButtonElement>('.widget__message--error button')!.click();
    await fixture.whenStable();

    expect(dashboardService.getItemsByQuantity).toHaveBeenCalledTimes(2);
  });

  it('refetches with the new range when the dropdown changes', async () => {
    dashboardService.getItemsByQuantity.mockReturnValue(of({ totalQuantity: 0, items: [] }));

    const fixture = TestBed.createComponent(ItemsDonut);
    await fixture.whenStable();
    const select = (fixture.nativeElement as HTMLElement).querySelector('select')!;
    select.value = 'Today';
    select.dispatchEvent(new Event('change'));
    await fixture.whenStable();

    expect(dashboardService.getItemsByQuantity).toHaveBeenLastCalledWith('Today');
  });
});
