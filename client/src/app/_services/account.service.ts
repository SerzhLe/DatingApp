import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = 'https://localhost:5001/api/';
  private currentUserSource = new ReplaySubject<User>(1); //buffer for storing User object - 1 amount - size of buffer
  currentUser$ = this.currentUserSource.asObservable();//as it will be an observable - it should have '$' at the end

  constructor(private http: HttpClient) { }

  login(model: any) {
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user = response;
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    );
  }

  register(model: any) {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    )
  }

  setCurrentUser(user: User) {  //узнай завтра зачем мы сделали этот метод! Этот метод мы вызываем при запуске веб приложения в app component
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }

}

//Services are:
//1) injectable - you can inject them in angular components
//2) singelton - data stored in services (such as 'baseUrl') will be disposed ONLY when web app is closed (on contrast - when you move from
// one component to another - first component is destroyed)