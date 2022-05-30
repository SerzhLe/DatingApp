import { Component, OnInit } from '@angular/core';
import { PageChangedEvent } from 'ngx-bootstrap/pagination';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Message } from '../_models/message';
import { MessageParams } from '../_models/messageParams';
import { Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';
import { PresenceService } from '../_services/presence.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[] = [];
  pagination: Pagination;
  messageParams: MessageParams;
  loading = false;

  constructor(private messageService: MessageService, private toastr: ToastrService) {
    this.messageParams = new MessageParams();
   }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.loading = true;
    this.messageService.getMessages(this.messageParams).subscribe( {
      next: response => {
        this.messages = response.result;
        this.pagination = response.pagination;
        this.loading = false;
      }
    });
  }

  pageChanged(event: PageChangedEvent) {
    if (this.messageParams.pageNumber !== event.page) {
      this.messageParams.pageNumber = event.page;
      this.loadMessages();
    }
  }

  deleteMessage(message: Message) {
    this.messageService.deleteMessage(message, this.messageParams).subscribe({
      next: response => {
        const index = this.messages.indexOf(message);
        this.messages.splice(index, 1);
        this.toastr.success("You successfully deleted the message");
      }
    })
  }

}
