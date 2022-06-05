import { AbstractControl, ValidationErrors } from "@angular/forms";

export class GeneralValidators {
    static containLettersNumbersHyphenSpace(control: AbstractControl): ValidationErrors | null {
        if (/^[A-Za-z]+[A-Za-z- ]*$/.test(control.value as string)) {
            return null;
        }

        return {containOnlyLettersNumbersHyphenSpace: {
            errorMessage: "Input can contain at least one English letter first, then space, hyphen or English letters"
        }};
    }
}