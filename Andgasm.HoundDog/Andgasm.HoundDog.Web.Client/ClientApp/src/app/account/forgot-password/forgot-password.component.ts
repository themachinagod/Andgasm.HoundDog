// #region Imports
import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../shared/services/user.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { CredentialsViewModel } from '../../shared/models/credentials.viewmodel.interface';
import { UserPasswordService } from '../../shared/services/user.password.service';
import { finalize } from 'rxjs/operators';
// #endregion

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit {

  // #region Form Elements
  @ViewChild("f", null) credentialsForm: NgForm;
  // #endregion

  // #region State Properties
  errors: [];
  isRequesting: boolean;
  submitted: boolean = false;
  requires2fachallenge: boolean = false;
  successfullySentLink: boolean = false;
  hasProvidedUserName: boolean = false;
  currentStepName: string = "AccountId";
  credentials: CredentialsViewModel = {} as CredentialsViewModel;
  // #endregion

  // #region Constructor
  constructor(
    private _passwordService: UserPasswordService,
    private _authenticationService: AuthenticationService,
    private _toastrService: ToastrService,
    private _router: Router) {

    this.errors = [];
  }
  // #endregion

  // #region Initialise & Destroy
  ngOnInit() {

  }
  // #endregion

  // #region Initiate Reset
  
  async provideAccountIdentifier({ valid }: { valid: boolean }) {

    this.submitted = true;
    if (valid) {
      this.hasProvidedUserName = true;
      var needs2fa = await this.determine2faStatus();

      if (!needs2fa) {
        this.sendPasswordReset({ valid })
      }
      else {
        this.requires2fachallenge = true;
        this.currentStepName = "2FA";
      }
    }
  }

  sendPasswordReset({ valid }: { valid: boolean }) {

    if (valid) {
      this.submitted = true;
      this.isRequesting = true;

      this._passwordService.sendPasswordReset(this.credentials.username, this.credentials.verificationCode, this.credentials.email)
        .pipe(finalize(() => this.isRequesting = false))
        .subscribe(
          sendresetrsult => {
            this._toastrService.success('We have sent a password reset link to the email address associated with the specified account. Please follow this link to complete the password reset process. If you do not receive a link to your associated email please check your spam folders before attempting to initiate this process again.', 'Password Reset Link Sent!');
            this.successfullySentLink = true;
            this.resetForm();
          },
          errors => {
            this._toastrService.error('There was an error while trying to send your password reset link to your mail account. Please try again, if this problem continues please contact a member of our suppport team!', 'Password Reset Link Failed!');
            this.errors = errors;
          });
    }
  }

  async determine2faStatus() {

    if (this.credentials.username) {
      return this._authenticationService.userNameRequires2FA(this.credentials.username).toPromise();
    }
  }
  // #endregion

  // #region Form Reset
  resetForm() {
    this.errors = [];
    this.isRequesting = false;
    this.submitted = false;
    this.requires2fachallenge = false;

    this.credentialsForm.form.markAsPristine();
    this.credentialsForm.form.markAsUntouched();

    this.credentials = {} as CredentialsViewModel;
    this.currentStepName = "AccountId";
  }

  cancelResetOperation() {
    this._router.navigate(['/login']); 
  }
  // #endregion
}

