import { Directive, forwardRef } from '@angular/core';
import { NG_VALIDATORS, FormControl } from '@angular/forms';

function validatePhoneFactory() {

  return (c: FormControl) => {
    let PHONE_REGEXP_INTERNATIONAL = /^\(?(?:(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?\(?(?:0\)?[\s-]?\(?)?|0)(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}|\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4}|\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3})|\d{5}\)?[\s-]?\d{4,5}|8(?:00[\s-]?11[\s-]?11|45[\s-]?46[\s-]?4\d))(?:(?:[\s-]?(?:x|ext\.?\s?|\#)\d+)?)$/g; 
    var pc = PHONE_REGEXP_INTERNATIONAL.test(c.value);
    return (pc) ? null : {
      validatePhone: {
        valid: false
      }
    };
  };
}

@Directive({
  selector: '[validatePhone][ngModel],[validatePhone][formControl]',
  providers: [
    { provide: NG_VALIDATORS, useExisting: forwardRef(() => PhoneValidator), multi: true }
  ]
})
export class PhoneValidator {

  validator: Function;

  constructor() {
    this.validator = validatePhoneFactory();
  }

  validate(c: FormControl) {
    return this.validator(c);
  }
}
