import {
  calculateMargin,
  calculateTotalCost,
  calculateTotalSelling,
} from './purchase-bill-calculator';

describe('purchase-bill-calculator', () => {
  it('matches the brief worked example (Cost 100, Price 150, Qty 5, Discount 20%)', () => {
    expect(calculateTotalCost(100, 5, 20)).toBe(400);
    expect(calculateTotalSelling(150, 5)).toBe(750);
    expect(calculateMargin(100, 150)).toBe(50);
  });

  it('zeroes out the cost at 100% discount', () => {
    expect(calculateTotalCost(200, 3, 100)).toBe(0);
  });

  it('rounds to two decimal places', () => {
    expect(calculateTotalCost(33.333, 3, 10)).toBe(90);
  });

  it('handles zero quantity and zero cost', () => {
    expect(calculateTotalCost(0, 10, 0)).toBe(0);
    expect(calculateTotalCost(100, 0, 0)).toBe(0);
  });
});
