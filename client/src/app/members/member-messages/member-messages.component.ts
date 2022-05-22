import { Container } from '@angular/compiler/src/i18n/i18n_ast';
import { AfterViewChecked, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { MessageToCreate } from 'src/app/_models/messageToCreate';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() messages: Message[];
  @Input() recipientUserName: string;
  message: MessageToCreate;
  newMessage: Message;

  constructor(private messageService: MessageService) {
    this.message = new MessageToCreate();
   }

  ngOnInit(): void {
    this.message.recipientUserName = this.recipientUserName;
  }

  sendMessage() {
    this.messageService.createMessage(this.message).subscribe({
      next: response => {
        this.newMessage = response;
        this.messages.push(this.newMessage);
        this.message.content = '';
      } 
    });
  }

}
