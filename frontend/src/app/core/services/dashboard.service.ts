import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  DashboardRange,
  ItemsByQuantity,
  LatestOrder,
  OldestItem,
} from '../models/dashboard.models';

/** Thin HTTP client over the backend's /api/dashboard endpoints (one per widget). */
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  getLatestOrders(range: DashboardRange): Observable<LatestOrder[]> {
    return this.http.get<LatestOrder[]>(`${environment.apiUrl}/dashboard/latest-orders`, {
      params: this.rangeParams(range),
    });
  }

  getOldestItems(range: DashboardRange): Observable<OldestItem[]> {
    return this.http.get<OldestItem[]>(`${environment.apiUrl}/dashboard/oldest-items`, {
      params: this.rangeParams(range),
    });
  }

  getItemsByQuantity(range: DashboardRange): Observable<ItemsByQuantity> {
    return this.http.get<ItemsByQuantity>(`${environment.apiUrl}/dashboard/items-by-quantity`, {
      params: this.rangeParams(range),
    });
  }

  private rangeParams(range: DashboardRange): HttpParams {
    return new HttpParams().set('range', range);
  }
}
