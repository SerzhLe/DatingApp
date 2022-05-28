import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeader } from './paginationHelper';
import { Message } from '../_models/message'
import { MessageToCreate } from '../_models/messageToCreate';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../_models/user';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { Group } from '../_models/group';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private baseUrl = environment.apiUrl;
  private hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient, private toastr: ToastrService, private presence: PresenceService) { }

  createHubConnection(user: User, anotherUsername: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + anotherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on('ReceiveMessageThread', (messages: Message[]) => {
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on('NewMessage', message => {
      this.messageThread$.pipe(take(1)).subscribe(messages => {
        this.messageThreadSource.next([...messages, message]);
      })
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      this.messageThread$.pipe(take(1)).subscribe(messages => {

        let unreadMessages = messages.filter(m => m.messageRead === '0001-01-01T00:00:00');

        if (unreadMessages.length > 0) {
      
          unreadMessages.forEach(m => {
            m.messageRead = new Date(Date.now()).toUTCString();
          });
  
          this.messageThreadSource.next([...messages]);
        }
      })
    });

    this.hubConnection.on('ReduceUnreadMessages', (unreadMessagesCount : number) => {
      this.presence.unreadMessagesCount$.pipe(take(1)).subscribe(count => { 
        count -= unreadMessagesCount;
        this.presence.unreadMessagesCountSource.next(count);
      });
    })
  }

  stopHubConnection() {
    if (!this.hubConnection || this.hubConnection?.state === 'Disconnected') return;
    this.hubConnection.stop().catch(error => console.log(error));
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeader(pageNumber, pageSize);

    params = params.append('container', container);

    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http);
  }

  async createMessage(message: MessageToCreate) { //async because returns Promise
    return this.hubConnection.invoke('SendMessage', message)
      .catch(error => console.log(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
