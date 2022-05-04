import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccountService } from '../_services/account.service';
import { User } from '../_models/user';
import { take } from 'rxjs/operators';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(private accountService: AccountService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    let currentUser : User;

    this.accountService.currentUser$.pipe(take(1)).subscribe(user => currentUser = user);
    //use pipe and take 1 to ensure that we complete subscription and dispose resources

    if (currentUser) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${currentUser.token}`
        }
      }) //that will set the token of current user to each request when user is logged in
    }

    return next.handle(request);
  }
}

//Interceptors are also as services - disposes when web app closes and initializing when it starts
