import { Injectable } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, ReplaySubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  private hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]); 
  onlineUsers$ = this.onlineUsersSource.asObservable();

  unreadMessagesCountSource = new BehaviorSubject<number>(0);
  unreadMessagesCount$ = this.unreadMessagesCountSource.asObservable();

  //when user go to another browser or close window - signalR automatically disconnet user
  //we need only conttrol disconnection in logout function
  constructor(private toastr: ToastrService, private router: Router) { }

  //we cannot use our jwt.interceptor, so we will pass user's token to query string
  //it won't be http request 
  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
      accessTokenFactory: () => user.token //pass token
    })
      .withAutomaticReconnect() //if user lost connection due to low internet - it will automatically reconnect
      .build(); //creates a connection

    this.hubConnection
      .start()
      .catch(error => console.log(error));

      //here we register methods that will be called by server when user connect and disconnect from hub
    this.hubConnection.on('UserIsOnline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
        this.onlineUsersSource.next([...usernames, username]);
      });
    });

    this.hubConnection.on('UserIsOffline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
        usernames.splice(usernames.indexOf(username), 1);
        this.onlineUsersSource.next(usernames);
      })
    });

    this.hubConnection.on("GetOnlineUsers", (onlineUsers: string[]) => this.onlineUsersSource.next(onlineUsers));

    this.hubConnection.on('NotifyAboutNewMessage', username => {
      this.unreadMessagesCount$.pipe(take(1)).subscribe(count => {
        this.unreadMessagesCountSource.next(++count);
      });

      this.toastr.info('You have got a new message from ' + username + '!')
        .onTap
        .pipe(take(1))
        .subscribe(() => {
          this.router.navigateByUrl('/members/' + username + '?tab=3');
        });
    });

    this.hubConnection.on('UnreadMessagesCount', (count: number) => {
      this.unreadMessagesCountSource.next(count);
    });
  } 

  stopHubConnection() {
    this.hubConnection.stop().catch(error => console.log(error));
  }
}
