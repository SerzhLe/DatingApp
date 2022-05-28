import { Container } from '@angular/compiler/src/i18n/i18n_ast';
import { AfterViewChecked, ChangeDetectionStrategy, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { take } from 'rxjs/operators';
import { Message } from 'src/app/_models/message';
import { MessageToCreate } from 'src/app/_models/messageToCreate';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush, //to prevent error when scrollTop = scrollHeight
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() recipientUserName: string;
  message: MessageToCreate;

  constructor(public messageService: MessageService) {
    this.message = new MessageToCreate();
   }

  ngOnInit(): void {
    this.message.recipientUserName = this.recipientUserName;
  }

  sendMessage() {
    this.messageService.createMessage(this.message).then(() => {
      this.message.content = '';
    })
  }

}
