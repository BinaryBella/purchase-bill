import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { LoginResponse } from '../models/auth.models';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const response: LoginResponse = {
    token: 'fake-token',
    expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    username: 'info@enhanzer.com',
    locations: [{ locationCode: 'LOC-1', locationName: 'Head Office' }],
  };

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule] });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('starts unauthenticated with no stored session', () => {
    expect(service.isAuthenticated()).toBe(false);
    expect(service.getToken()).toBeNull();
  });

  it('posts to /auth/login and stores the session on success', () => {
    let received: LoginResponse | undefined;
    service
      .login({ email: response.username, password: 'Welcome#5' })
      .subscribe((r) => (received = r));

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(response);

    expect(received).toEqual(response);
    expect(service.isAuthenticated()).toBe(true);
    expect(service.username()).toBe('info@enhanzer.com');
    expect(service.getToken()).toBe('fake-token');
    expect(JSON.parse(sessionStorage.getItem('purchase-bill.session')!).token).toBe('fake-token');
  });

  it('logout clears the session', () => {
    service.login({ email: response.username, password: 'Welcome#5' }).subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush(response);
    expect(service.isAuthenticated()).toBe(true);

    service.logout();

    expect(service.isAuthenticated()).toBe(false);
    expect(service.getToken()).toBeNull();
    expect(sessionStorage.getItem('purchase-bill.session')).toBeNull();
  });

  it('treats an expired token as not authenticated', () => {
    const expired: LoginResponse = {
      ...response,
      expiresAtUtc: new Date(Date.now() - 1000).toISOString(),
    };
    service.login({ email: expired.username, password: 'Welcome#5' }).subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush(expired);

    expect(service.isAuthenticated()).toBe(false);
  });
});
