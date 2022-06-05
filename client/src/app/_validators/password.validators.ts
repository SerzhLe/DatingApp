import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export class PasswordValidators {
    static validatePassword(control: AbstractControl) : ValidationErrors | null {
        if (/(?=.*[a-z])/.test(control.value) //at least ont small letter
          && /(?=.*[A-Z])/.test(control.value) //at least one capital letter
          && /(?=.*[0-9])/.test(control.value)  //at least one number
          && /(?=.*[!@#\$%\^&\*])/.test(control.value) //at least one special character
          && /^[a-zA-Z0-9!@#\$%\^&\*]*$/.test(control.value)  //containe ONLY all characters specified in []
          ) {
            return null;
        }
    
        return { notValidPassword: true};
    }

    static matchValues(matchTo: string) : ValidatorFn {
        return (control: AbstractControl) => {
          return control?.value === control?.parent?.controls[matchTo].value ? null : { notMatching: true };
        }
      }
}