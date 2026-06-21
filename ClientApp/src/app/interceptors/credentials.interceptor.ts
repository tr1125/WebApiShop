import { HttpInterceptorFn } from '@angular/common/http';

// Automatically attach credentials (HttpOnly JWT cookie) to every API request.
// Required for cross-origin requests from :4200 → :5013.
export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  const withCreds = req.clone({ withCredentials: true });
  return next(withCreds);
};
