import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, NgForm, Validators } from '@angular/forms';
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
  loginForm: FormGroup;
  userName: string;

  constructor(public accountService: AccountService, 
    private router: Router, private toastr: ToastrService,
    private fb: FormBuilder, public presence: PresenceService) { }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    })
  }

  login() {
    if (!this.loginForm.valid) {
      this.toastr.error("Please, enter the credentials.");
      return;
    }

    this.accountService.login(this.loginForm.value).subscribe({
      next: response => {
        this.router.navigateByUrl('/members');
      }
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  getUserName() {
    this.userName = JSON.parse(localStorage.getItem('user')).userName;
  }
}
