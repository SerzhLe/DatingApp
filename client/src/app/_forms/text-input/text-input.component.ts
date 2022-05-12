import { Component, Input, OnInit, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})

//we build a general component for each input field
export class TextInputComponent implements ControlValueAccessor {
  @Input() label: string;
  @Input() type = 'text';

  constructor(@Self() public ngControl: NgControl) { //create dependency injection of NgControl locally
    this.ngControl.valueAccessor = this;
   }

  writeValue(obj: any): void {
  }

  registerOnChange(fn: any): void {
  }

  registerOnTouched(fn: any): void {
  }

  
}
