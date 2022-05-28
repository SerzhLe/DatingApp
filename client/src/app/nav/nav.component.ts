import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';
import { PresenceService } from '../_services/presence.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  @ViewChild('loginForm') loginForm: NgForm;
  model: any = {};
  userName: string;

  constructor(public accountService: AccountService, 
    private router: Router, private toastr: ToastrService, 
    private presence: PresenceService) { }

  ngOnInit(): void {
  }

  login() {
    if (!this.loginForm.valid) {
      this.toastr.error("Please, enter the credentials.");
      return;
    }

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
