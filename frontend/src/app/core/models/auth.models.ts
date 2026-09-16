/** Mirrors PurchaseBill.Application.Dtos.Auth on the backend. */

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LocationDto {
  locationCode: string;
  locationName: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  username: string;
  locations: LocationDto[];
}
