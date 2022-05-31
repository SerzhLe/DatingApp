import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { MembersService } from './members.service';
import { MessageService } from './message.service';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient, 
    private memberService: MembersService, 
    private presenceService: PresenceService, 
    private messageService: MessageService) { }

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
    return JSON.parse(atob(token.split('.')[1])); 
  }

}