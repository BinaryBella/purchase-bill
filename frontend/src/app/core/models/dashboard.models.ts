/** Mirrors PurchaseBill.Application.Dtos.Dashboard on the backend. */

/** The date window a widget's dropdown selects; sent verbatim as the `range` query parameter. */
export type DashboardRange = 'Today' | 'Last7Days' | 'Last30Days' | 'All';

export const DASHBOARD_RANGES: readonly { value: DashboardRange; label: string }[] = [
  { value: 'Today', label: 'Today' },
  { value: 'Last7Days', label: 'Last 7 days' },
  { value: 'Last30Days', label: 'Last 30 days' },
  { value: 'All', label: 'All time' },
];

export interface LatestOrder {
  id: number;
  poNumber: string;
  netAmount: number;
  itemCount: number;
  createdAt: string;
}

export interface OldestItem {
  purchaseOrderId: number;
  poNumber: string;
  itemName: string;
  quantity: number;
  createdAt: string;
}

export interface ItemQuantity {
  itemName: string;
  totalQuantity: number;
  percent: number;
}

export interface ItemsByQuantity {
  totalQuantity: number;
  items: ItemQuantity[];
}
