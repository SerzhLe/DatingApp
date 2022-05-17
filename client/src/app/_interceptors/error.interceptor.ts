import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError(error => {
        if (error) {
          switch (error.status) {
            case 400:
              if (error.error.errors) { //if the error has array of errors in error object
                const modalStateErrors = [];
                for (const key in error.error.errors) {
                  if (error.error.errors[key]) {
                    modalStateErrors.push(error.error.errors[key]);
                  }
                }
                throw modalStateErrors.flat(); //this is used to show errors below the inputs - if u wan to use flat() - specify in tsconfig.json "es2019" 
              //flat - if we do not use flat thatn we goet array of arrays but because of flat we got array of strings
              }
              else if (typeof(error.error) === 'object') {
                this.toastr.error(error.statusText === 'OK' ? 'Bad Request' : error.statusText, error.status);
              } else {
                this.toastr.error(error.error);
              }
              break;
            case 401:
              if (typeof(error.error) === 'object') {
                this.toastr.error(error.statusText === 'OK' ? 'Unauthorized' : error.statusText, error.status);
              }
              else {
                this.toastr.error(error.error);
              }
              break;
            case 404:
              this.router.navigateByUrl('/not-found');
              break;
            case 500:
              const navigationExtras: NavigationExtras = { state: {error: error.error}}; //эту ошибку мы передаем на маршрут /server-error
              this.router.navigateByUrl('/server-error', navigationExtras);
              break;
            default:
              this.toastr.error('Something unexpected went wrong');
              console.log(error);
              break;
          }
        }
        return throwError(error);
      }) //error - is http response error
    );
  }
}
