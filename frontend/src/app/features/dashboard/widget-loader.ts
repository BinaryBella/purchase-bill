import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { Observable, catchError, combineLatest, map, of, startWith, switchMap } from 'rxjs';
import { extractErrorMessage } from '../../core/models/api-error.model';
import { DashboardRange } from '../../core/models/dashboard.models';

export type WidgetState<T> =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; data: T };

/**
 * Shared loading behaviour for a dashboard widget: refetches whenever the range dropdown
 * changes (cancelling any in-flight request) or `reload()` is called, and exposes the result
 * as a loading/error/ready state so one failing widget never blanks the rest of the page.
 * Must be called from an injection context (a component field initializer).
 */
export function createWidgetLoader<T>(
  fetch: (range: DashboardRange) => Observable<T>,
  initialRange: DashboardRange = 'Today',
) {
  const range = signal<DashboardRange>(initialRange);
  const reloadTick = signal(0);

  const state = toSignal(
    combineLatest([toObservable(range), toObservable(reloadTick)]).pipe(
      switchMap(([selected]) =>
        fetch(selected).pipe(
          map((data): WidgetState<T> => ({ status: 'ready', data })),
          catchError((error: unknown) =>
            of<WidgetState<T>>({
              status: 'error',
              message:
                error instanceof HttpErrorResponse
                  ? extractErrorMessage(error.error, 'Unable to load this widget.')
                  : 'Unable to load this widget.',
            }),
          ),
          startWith<WidgetState<T>>({ status: 'loading' }),
        ),
      ),
    ),
    { initialValue: { status: 'loading' } as WidgetState<T> },
  );

  return {
    range,
    state,
    reload: () => reloadTick.update((tick) => tick + 1),
  };
}
