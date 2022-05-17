import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { MembersService } from './members.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1); //buffer for storing User object - 1 amount - size of buffer
  currentUser$ = this.currentUserSource.asObservable();//as it will be an observable - it should have '$' at the end

  constructor(private http: HttpClient, private memberService: MembersService) { }

  login(model: any) {
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user = response;
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  register(model: any) {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    )
  }

  setCurrentUser(user: User) {  //узнай завтра зачем мы сделали этот метод! Этот метод мы вызываем при запуске веб приложения в app component
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.memberService.memberCache = new Map();
    this.memberService.loggedInMember = null;
    this.memberService.userParams = null;
    this.currentUserSource.next(null);
  }

}

//Services are:
//1) injectable - you can inject them in angular components
//2) singelton - data stored in services (such as 'baseUrl') will be disposed ONLY when web app is closed (on contrast - when you move from
// one component to another - first component is destroyed)