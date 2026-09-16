/**
 * Shape of the error bodies returned by the backend's ExceptionHandlingMiddleware:
 * a plain ProblemDetails for 401/404/500, or a ValidationProblemDetails (with `errors`)
 * for 400s from FluentValidation.
 */
export interface ApiProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

/** Pulls the best single human-readable message out of a ProblemDetails-shaped error body. */
export function extractErrorMessage(body: unknown, fallback: string): string {
  const problem = body as ApiProblemDetails | null;
  if (!problem) {
    return fallback;
  }

  if (problem.errors) {
    const firstError = Object.values(problem.errors)[0]?.[0];
    if (firstError) {
      return firstError;
    }
  }

  return problem.detail ?? problem.title ?? fallback;
}
