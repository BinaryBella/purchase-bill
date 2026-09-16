import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Login } from './login';
import { AuthService } from '../../../core/services/auth.service';

describe('Login', () => {
  let fixture: ComponentFixture<Login>;
  let component: Login;
  let authService: { login: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authService = { login: vi.fn() };
    router = { navigate: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('does not call the API when the form is invalid', () => {
    component['submit']();

    expect(authService.login).not.toHaveBeenCalled();
    expect(component['form'].controls.email.touched).toBe(true);
    expect(component['form'].controls.password.touched).toBe(true);
  });

  it('shows a required message for an empty email and a format message for a bad one', () => {
    const email = component['form'].controls.email;

    email.setValue('');
    expect(component['emailErrorMessage']()).toBe('Email is required.');

    email.setValue('not-an-email');
    expect(component['emailErrorMessage']()).toBe('Enter a valid email address.');
  });

  it('logs in and navigates to /dashboard on success', () => {
    authService.login.mockReturnValue(
      of({ token: 't', expiresAtUtc: '', username: 'info@enhanzer.com', locations: [] }),
    );
    component['form'].setValue({ email: 'info@enhanzer.com', password: 'Welcome#5' });

    component['submit']();

    expect(authService.login).toHaveBeenCalledWith({
      email: 'info@enhanzer.com',
      password: 'Welcome#5',
    });
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
    expect(component['loading']()).toBe(false);
  });

  it('surfaces the backend error message when login fails', () => {
    authService.login.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 401,
            error: { title: 'Authentication failed', status: 401, detail: 'Invalid Login Details' },
          }),
      ),
    );
    component['form'].setValue({ email: 'info@enhanzer.com', password: 'wrong' });

    component['submit']();

    expect(component['errorMessage']()).toBe('Invalid Login Details');
    expect(component['loading']()).toBe(false);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('toggles password visibility', () => {
    expect(component['showPassword']()).toBe(false);
    component['togglePasswordVisibility']();
    expect(component['showPassword']()).toBe(true);
  });
});
