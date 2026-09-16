/**
 * Mirrors PurchaseBill.Application.Common.PurchaseBillCalculator on the backend, so the grid
 * shows live totals as the user types instead of waiting on a round trip. The backend
 * recomputes the same values when the bill is saved - it is the source of truth, this is only
 * for instant feedback.
 */

function round2(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}

export function calculateMargin(standardCost: number, standardPrice: number): number {
  return round2(standardPrice - standardCost);
}

export function calculateTotalCost(
  standardCost: number,
  quantity: number,
  discountPercent: number,
): number {
  return round2(standardCost * quantity * (1 - discountPercent / 100));
}

export function calculateTotalSelling(standardPrice: number, quantity: number): number {
  return round2(standardPrice * quantity);
}
