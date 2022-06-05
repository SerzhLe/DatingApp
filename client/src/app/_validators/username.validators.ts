import { AbstractControl, ValidationErrors } from "@angular/forms";

export class UsernameValidators {
    static containOnlyLettersAndNumbers(control: AbstractControl): ValidationErrors | null {
        if (/^[A-Za-z0-9]*$/.test(control.value as string)) {
            return null;
        }

        return {canContainOnlyLettersAndNumbers: {
            errorMessage: "Input can contain only English letters or/and numbers"
        }};
    }
}