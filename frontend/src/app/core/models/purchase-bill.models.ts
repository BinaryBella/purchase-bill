/** Mirrors PurchaseBill.Application.Dtos.PurchaseBills on the backend. */

export interface PurchaseBillItemRequest {
  itemName: string;
  batchLocationCode: string;
  standardCost: number;
  standardPrice: number;
  quantity: number;
  freeQuantity: number;
  discountPercent: number;
}

export interface CreatePurchaseBillRequest {
  items: PurchaseBillItemRequest[];
}

export interface PurchaseBillItemResponse extends PurchaseBillItemRequest {
  id: number;
  batchLocationName: string;
  margin: number;
  totalCost: number;
  totalSelling: number;
}

export interface PurchaseBillResponse {
  id: number;
  createdAt: string;
  totalItems: number;
  totalQuantity: number;
  totalCost: number;
  totalSelling: number;
  items: PurchaseBillItemResponse[];
}

/**
 * One row of the Items grid while it's still being built up client-side (before "Save"
 * persists the whole bill). Carries the resolved batch name too, so the table doesn't need to
 * re-look it up from the locations list.
 */
export interface DraftPurchaseBillItem {
  rowId: number;
  itemName: string;
  batchLocationCode: string;
  batchLocationName: string;
  standardCost: number;
  standardPrice: number;
  margin: number;
  quantity: number;
  freeQuantity: number;
  discountPercent: number;
  totalCost: number;
  totalSelling: number;
}

/** What the entry form emits on "Add" - the page assigns the rowId. */
export type NewDraftPurchaseBillItem = Omit<DraftPurchaseBillItem, 'rowId'>;
