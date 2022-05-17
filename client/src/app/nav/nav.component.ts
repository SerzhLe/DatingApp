import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
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

  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService) { }

  ngOnInit(): void {
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: response => {
        this.router.navigateByUrl('/members'); //navigate to members element when logged in
      }
    }); //if returns Observable - we need subscribe to apply changes
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  getUserName() {
    this.userName = JSON.parse(localStorage.getItem('user')).userName;
  }
}
