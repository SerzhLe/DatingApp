import { Component, Input, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  userName: string;

  constructor(public accountService: AccountService) { }

  ngOnInit(): void {
    this.getUserName();
  }

  login() {
    this.accountService.login(this.model).subscribe(response => {
      console.log(response);
      this.getUserName();
    }, error => {
      console.log(error);
    }); //if returns Observable - we need subscribe to apply changes
  }

  logout() {
    this.accountService.logout();
  }

  getUserName() {
    this.userName = JSON.parse(localStorage.getItem('user')).userName;
  }
}
