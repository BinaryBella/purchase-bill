import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LocationDto } from '../models/auth.models';
import { CreatePurchaseBillRequest, PurchaseBillResponse } from '../models/purchase-bill.models';

@Injectable({ providedIn: 'root' })
export class PurchaseBillService {
  constructor(private readonly http: HttpClient) {}

  getItems(): Observable<string[]> {
    return this.http.get<string[]>(`${environment.apiUrl}/items`);
  }

  getLocations(): Observable<LocationDto[]> {
    return this.http.get<LocationDto[]>(`${environment.apiUrl}/locations`);
  }

  create(request: CreatePurchaseBillRequest): Observable<PurchaseBillResponse> {
    return this.http.post<PurchaseBillResponse>(`${environment.apiUrl}/purchase-bills`, request);
  }
}
