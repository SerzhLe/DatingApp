import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { GeneralValidators } from '../_validators/general.validators';
import { PasswordValidators } from '../_validators/password.validators';
import { UsernameValidators } from '../_validators/username.validators';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter()
  registerForm: FormGroup;
  maxDate: Date;

  constructor(
    private accountService: AccountService,
    private router: Router,
    private fb: FormBuilder
    ) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', [Validators.required, Validators.maxLength(10), UsernameValidators.containOnlyLettersAndNumbers]],
      knownAs: ['', [Validators.required, Validators.maxLength(15), GeneralValidators.containLettersNumbersHyphenSpace]],
      dateOfBirth: ['', Validators.required],
      city: ['', [Validators.required, GeneralValidators.containLettersNumbersHyphenSpace]],
      country: ['', [Validators.required, GeneralValidators.containLettersNumbersHyphenSpace]],
      password: ['', [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(8),
        PasswordValidators.validatePassword
      ]],
      confirmPassword: ['', [
        Validators.required,
        PasswordValidators.matchValues('password')
      ]]
    });

    this.registerForm.controls.password.valueChanges.subscribe(() => { 
      this.registerForm.controls.confirmPassword.updateValueAndValidity(); 
    })
  }

  register() {
    this.accountService.register(this.registerForm.value).subscribe({
      next: response => this.router.navigateByUrl('/members')
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
