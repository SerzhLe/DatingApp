import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { MembersService } from './members.service';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1); //buffer for storing User object - 1 amount - size of buffer
  currentUser$ = this.currentUserSource.asObservable();//as it will be an observable - it should have '$' at the end

  constructor(private http: HttpClient, private memberService: MembersService, private presenceService: PresenceService) { }

  login(model: any) {
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map((user: User) => {
        if (user) {
          this.setCurrentUser(user);
          this.presenceService.createHubConnection(user);
        }
      })
    );
  }

  register(model: any) {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if (user) {
          this.setCurrentUser(user);
          this.presenceService.createHubConnection(user);
        }
      })
    )
  }

  setCurrentUser(user: User) { 
    if (!user) return; 
    user.roles = [];
    const roles = this.getDecodedToken(user.token)["role"];
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);

    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.memberService.memberCache = new Map();
    this.memberService.loggedInMember = null;
    this.memberService.userParams = null;
    this.presenceService.stopHubConnection(); 
  }

  getDecodedToken(token: string) {
    return JSON.parse(atob(token.split('.')[1])); //returning the payload of token
    //atob - decode string, btoa - encode. Only signature of token is enctypted!
  }

}

//Services are:
//1) injectable - you can inject them in angular components
//2) singelton - data stored in services (such as 'baseUrl') will be disposed ONLY when web app is closed (on contrast - when you move from
// one component to another - first component is destroyed)