import { Component, Input, OnInit, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

@Component({
  selector: 'app-password-input',
  templateUrl: './password-input.component.html',
  styleUrls: ['./password-input.component.css']
})
export class PasswordInputComponent implements ControlValueAccessor {
  @Input() label: string;
  @Input() id: string;
  isPasswordType = true;

  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this;
   }

   writeValue(obj: any): void {
  }

  registerOnChange(fn: any): void {
  }

  registerOnTouched(fn: any): void {
  }

  togglePasswordVisibility(elementId: string) {
    const element = document.getElementById(elementId);
    const type = element?.getAttribute('type') === 'password' ? 'text' : 'password';
    element.setAttribute('type', type);
    return type;
  }
}
