import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { PhoneConfirmationService } from '../../shared/services/phone.confirmation.service';
import { ToastrService } from 'ngx-toastr';

import { finalize } from 'rxjs/operators';


@Component({
  selector: 'verify-phone',
  templateUrl: './verify-phone.component.html',
  styleUrls: ['./verify-phone.component.css']
})
export class VerifyPhoneComponent implements OnInit {

  errors: [];
  isRequesting: boolean;
  verificationCode: string;

  constructor(private phoneService: PhoneConfirmationService, private toastrService: ToastrService, private router: Router) {

    this.errors = [];
  }

  ngOnInit() {

    this.sendPhoneVerificationCode();
  }

  sendPhoneVerificationCode() {

    this.errors = [];
    this.isRequesting = true;

    this.phoneService.sendPhoneVerificationCode()
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
          result => {
              this.toastrService.success('We have successfully send a verification code to your registered phone number. Please ener this code to complete your verification!', 'Verification Code Sent!');
              this.isRequesting = false
        },
          errors => {
              this.toastrService.error('There was an error while trying to send a verification code to your registered phone number. Please confirm your phone number is correct and try the resend code link below.', 'Verification Code Not Sent!');
              this.errors = errors;
          });
  }

  verifyPhoneVerificationCode() {

    this.errors = [];
    this.isRequesting = true;

    this.phoneService.verifyPhoneVerificationCode(this.verificationCode)
      .pipe(finalize(() => this.isRequesting = false))
      .subscribe(
        result => {
          this.toastrService.success('You have successfully verified your phone number, this helps keep your account secure and allows us to know you are in possesion of the number specified!', 'Phone Number Verified!');
          this.router.navigate(['/settings']);
        },
        errors => {
          this.toastrService.error('There was an error while trying to verify your phone number. Please try again, if this problem continues please contact a member of our suppport team!', 'Phone Number Verification Failed!');
          this.errors = errors;
        });
  }
}
