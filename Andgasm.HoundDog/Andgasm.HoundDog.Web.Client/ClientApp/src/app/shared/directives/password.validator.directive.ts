import { Directive, forwardRef } from '@angular/core';
import { NG_VALIDATORS, FormControl } from '@angular/forms';

function validatePasswordFactory() {
  return (c: FormControl) => {
    let PASSWORD_REGEXP_LC = /.*[a-z].*/g; // must contain lowercase
    let PASSWORD_REGEXP_UC = /.*[A-Z].*/g; // must contain uppercase
    let PASSWORD_REGEXP_NM = /.*[0-9].*/g; // must conatin digit
    let PASSWORD_REGEXP_LEN = /.{6,20}./g; // must be between 6 & 20 chars length

    var lc = PASSWORD_REGEXP_LC.test(c.value);
    var uc = PASSWORD_REGEXP_UC.test(c.value);
    var nm = PASSWORD_REGEXP_NM.test(c.value);
    var len = PASSWORD_REGEXP_LEN.test(c.value);

    return (lc && uc && nm && len) ? null : {
      validatePassword: {
        valid: false
      }
    };
  };
}

@Directive({
  selector: '[validatePassword][ngModel],[validatePassword][formControl]',
  providers: [
    { provide: NG_VALIDATORS, useExisting: forwardRef(() => PasswordValidator), multi: true }
  ]
})
export class PasswordValidator {

  validator: Function;

  constructor() {
    this.validator = validatePasswordFactory();
  }

  validate(c: FormControl) {
    return this.validator(c);
  }
}
