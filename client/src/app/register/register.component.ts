import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NavComponent } from '../nav/nav.component';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  //@Input() usersFromHomeComponent: any; //in order to put users got from home comp to register comp
  @Output() cancelRegister = new EventEmitter(); //in order to pass action from child (register) to parent (home) comp
  registerForm: FormGroup;
  maxDate: Date;
  validationErrors: string[] = [];

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
    private router: Router,
    private fb: FormBuilder //instead of manual creation of form - we use service
    ) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm() {
    this.registerForm = this.fb.group({ //we just mad our code smaller - instead of this.fb we would need to create new FormGroup and new FormControl
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(8),
        this.validatePassword
      ]],
      confirmPassword: ['', [
        Validators.required,
        this.matchValues('password')
      ]]
    });

    //this is useful when you change the password after typing the confirm pass - it will reset the validation to check pass identity

    this.registerForm.controls.password.valueChanges.subscribe(() => { //valueChanges - is observable - emit event every time control changes
      this.registerForm.controls.confirmPassword.updateValueAndValidity(); //every time password is changes - it resets the validation of confirm password
    })
  }


  //if you type any value in password - this error will check to confirm pass 
  //BUT if you then change password - it will say that confirm pass is valid - !!
  matchValues(matchTo: string) : ValidatorFn {
    return (control: AbstractControl) => {
      return control?.value === control?.parent?.controls[matchTo].value ? null : { notMatching: true }; //dynamic value
    }
  }

  validatePassword(control: AbstractControl) : ValidationErrors | null {
  const re = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])/;
    
    if (!re.test(control?.value)) {
      return { notValidPassword: true};
    } 

    return null;
  }

  register() {
    this.accountService.register(this.registerForm.value).subscribe({
      next: response => this.router.navigateByUrl('/members'),
      error: error => {
        this.validationErrors = error;
      } 
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

}
