// #region Imports
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { CredentialsViewModel } from '../../shared/models/credentials.viewmodel.interface';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';
import { SignInResultEnum } from '../../shared/enums/sign-in-result.enum';
import { finalize } from 'rxjs/operators';
// #endregion

@Component({
  selector: 'user-login',
  templateUrl: './user-login.component.html',
  styleUrls: ['./user-login.component.scss']
})
export class UserLoginComponent implements OnInit, OnDestroy {

  // #region Form Elements
  @ViewChild("f", null) credentialsForm: NgForm;
  // #endregion

  // #region Form State Fields
  errors: [];
  brandNew: boolean;
  confirmedMail: boolean;
  isRequesting: boolean;
  submitted: boolean = false;
  requires2fachallenge: boolean = false;
  returnUrl: string;
  hasProvidedBasicLogin: boolean = false;
  credentials: CredentialsViewModel = {} as CredentialsViewModel;
  // #endregion

  // #region Constructor
  constructor(
    private _authenticationService: AuthenticationService,
    private _router: Router,
    private _activatedRoute: ActivatedRoute,
    private _toastrService: ToastrService) {

    this.errors = [];
  }
  // #endregion
   
  // #region Initialise & Destroy
  ngOnInit() {

    this._activatedRoute.queryParams.subscribe(
      (param: any) => {
        this.brandNew = param['brandNew'];
        this.confirmedMail = param['confirmedMail'];
        this.credentials.username = param['email'];
      });
    this.returnUrl = this._activatedRoute.snapshot.queryParams['returnUrl'] || '/dashboard';

    this.handleMailConfirmationSuccess();
    this.redirectIfAuthenticated();
    this.determine2faStatus();
  }

  ngOnDestroy() {
  }
  // #endregion

  // #region Authenticate
  async provideBasicAuthDetails({ value, valid }: { value: CredentialsViewModel, valid: boolean }) {

    this.submitted = true;
    if (valid) {
      this.hasProvidedBasicLogin = true;
      var needs2fa = await this.determine2faStatus();

      if (!needs2fa) {
        this.login({ value, valid })
      }
      else {
        this.requires2fachallenge = true;
      }
    }
  }

  login({ value, valid }: { value: CredentialsViewModel, valid: boolean }) {

    if (valid) {
      this.submitted = true;
      this.isRequesting = true;

      this._authenticationService.login(this.credentials)
        .pipe(finalize(() => this.isRequesting = false))
        .subscribe(
          loginresult => {
            if (loginresult.signinResult == SignInResultEnum.Success) {
              this._toastrService.success('You have successfully signed into  your HoundDog account, welcome back and happy HoundDogging!', 'Successfully Authenticated!');
              this._router.navigate([this.returnUrl]);
            }
          },
          errors => {
            this._toastrService.error('Your sign in attempt failed. Please ensure you have entered the correct username and password and if required the up to date authentication code from your authenticator application. If you cant remember your password please follow the forgotten password process. If you cant get access to your authenticator application please follow the account recovery process.', 'Authentication Failed!');
            this.errors = errors;
          });
    }
  }

  async determine2faStatus() {

    if (!this._authenticationService.isLoggedIn && this.credentials.username) {
      return this._authenticationService.userNameRequires2FA(this.credentials.username).toPromise();
    }
  }
  // #endregion

  // #region Redirects
  resetPassword() {

    this._router.navigate(["/forgotpassword"]);
  }

  handleMailConfirmationSuccess() {

    if (this.confirmedMail) {
      this._toastrService.success('Thank you for taking the time to confirm your email address, your account is now fully activated and all site functionality is unlocked for your use!', 'Successfully Confirmed Email!');
    }
  }

  redirectIfAuthenticated() {

    if (this._authenticationService.isLoggedIn) {
      this._router.navigate(['/dashboard']);
    }
  }
  // #endregion

  // #region Reset
  cancelLoginOperation() {
    this.errors = [];
    this.isRequesting = false;
    this.submitted = false;
    this.hasProvidedBasicLogin = false;
    this.requires2fachallenge = false;

    this.credentialsForm.form.markAsPristine();
    this.credentialsForm.form.markAsUntouched();

    this.credentials = {} as CredentialsViewModel;
  }
  // #endregion
}
