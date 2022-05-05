import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { BusyService } from '../_services/busy.service';
import { delay, finalize } from 'rxjs/operators';

@Injectable()
//this service is for hanadling when requests go in and go out to show spinner loading
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.busyService.busy(); //on each request will call this method
    return next.handle(request).pipe(
      delay(1000), //add some delay
      finalize(() => {
        this.busyService.idle(); //call finalize when https request ends
      })
    )
  }
}
